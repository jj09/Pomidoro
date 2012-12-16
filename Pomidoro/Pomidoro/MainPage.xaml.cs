using Pomidoro.ThreadPool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Pomidoro
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Pomidoro.Common.LayoutAwarePage
    {
        public Image PomidoroImage { get { return image; } set { image.Source = value.Source; } }

        public string MruToken = null;

        public MainPage()
        {
            this.InitializeComponent();
            ThreadPoolSample.MainPage = this;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // Restore values stored in app data.
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Values.ContainsKey("MruToken"))
            {
                object value = roamingSettings.Values["MruToken"];

                if (value != null)
                {
                    MruToken = value.ToString();

                    try
                    {
                        // Open the file via the token that you stored when adding this file into the MRU list.
                        Windows.Storage.StorageFile file =
                            await Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.GetFileAsync(MruToken);

                        if (file != null)
                        {
                            // Open a stream for the selected file.
                            Windows.Storage.Streams.IRandomAccessStream fileStream =
                                await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                            // Set the image source to a bitmap.
                            Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage();

                            bitmapImage.SetSource(fileStream);
                            PomidoroImage.Source = bitmapImage;

                            // Set the data context for the page.
                            this.DataContext = file;
                        }
                    }
                    catch (FileNotFoundException ex)
                    {
                        // Restore default image if custom image was deleted from HDD
                        roamingSettings.Values["MruToken"] = null;

                        BitmapImage defaultImage = new BitmapImage();
                        defaultImage.UriSource = new Uri("ms-appx:///Images/Pomidoro.png");
                        ThreadPoolSample.MainPage.PomidoroImage.Source = defaultImage;
                    }
                }

            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Create a periodic timer that fires every time the period elapses.
        /// When the timer expires, its callback handler is called and the timer is reset.
        /// This behavior continues until the periodic timer is cancelled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreatePeriodicTimer(object sender, RoutedEventArgs args)
        {
            ThreadPoolSample.PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(
                async (timer) =>
                {
                    System.Threading.Interlocked.Decrement(ref ThreadPoolSample.PeriodicTimerCount);
                    await Dispatcher.RunAsync(
                        CoreDispatcherPriority.High, () =>
                        {
                            if (ThreadPoolSample.PeriodicTimerCount == 0)
                            {
                                NotifyUser();
                                CancelPeriodicTimer(sender, args);
                            }
                            else
                            {
                                ThreadPoolSample.MainPage.UpdateUI(Status.Completed);
                            }
                        });
                },
                TimeSpan.FromMilliseconds(ThreadPoolSample.PeriodicTimerMilliseconds));

            UpdateUI(Status.Started);

        }

        private void CancelPeriodicTimer(object sender, RoutedEventArgs args)
        {
            if (ThreadPoolSample.PeriodicTimer != null)
            {
                ThreadPoolSample.PeriodicTimer.Cancel();
                ThreadPoolSample.PeriodicTimerCount = 1500;
                UpdateUI(Status.Canceled);
            }
        }

        public void UpdateUI(Status status)
        {
            ThreadPoolSample.PeriodicTimerStatus = status;

            var secs = ThreadPoolSample.PeriodicTimerCount % 60;
            var mins = ThreadPoolSample.PeriodicTimerCount / 60;
            PeriodicTimerStatus.Text = string.Format("{0}:{1}", mins, secs.ToString("00"));

            var startButtonEnabled = ((status != Status.Started) && (status != Status.Completed));
            if (startButtonEnabled)
            {
                StartButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                StartButton.Visibility = Visibility.Collapsed;
                CancelButton.Visibility = Visibility.Visible;
            }

            // tile notification
            NotifyTile();
        }

        private void NotifyTile()
        {
            TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideImageAndText01);

            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareBlock);
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            var secs = ThreadPoolSample.PeriodicTimerCount % 60;
            var mins = ThreadPoolSample.PeriodicTimerCount / 60;
            tileTextAttributes[0].InnerText = mins.ToString();
            tileTextAttributes[1].InnerText = "mins";
            //tileTextAttributes[1].InnerText = secs.ToString("00");

            TileNotification tileNotification = new TileNotification(tileXml);

            tileNotification.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(10);

            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        private async void NotifyUser()
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            // Make sure notifications are enabled
            if (notifier.Setting != NotificationSetting.Enabled)
            {
                var dialog = new MessageDialog("Notifications are currently disabled");
                await dialog.ShowAsync();
                return;
            }

            // Get a toast template and insert a text node containing a message
            var template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var element = template.GetElementsByTagName("text")[0];
            element.AppendChild(template.CreateTextNode("Pomidoro's time is up!"));

            // Schedule the toast to appear 1 seconds from now
            var date = DateTimeOffset.Now.AddSeconds(1);
            var stn = new ScheduledToastNotification(template, date);
            notifier.AddToSchedule(stn);
        }
    }
}
