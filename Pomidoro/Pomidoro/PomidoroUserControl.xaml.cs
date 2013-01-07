using Pomidoro.ThreadPool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Pomidoro
{
    public sealed partial class PomidoroUserControl : UserControl
    {
        public PomidoroUserControl()
        {
            this.InitializeComponent();
            this.Time.Text = (ThreadPoolSample.DefaultPeriodicTimerCount / 60).ToString();
        }

        private async void ChoosePomidoroButton_Click(object sender, RoutedEventArgs e)
        {
            // File picker APIs don't work if the app is in a snapped state.
            // If the app is snapped, try to unsnap it first. Only show the picker if it unsnaps.
            if (Windows.UI.ViewManagement.ApplicationView.Value != Windows.UI.ViewManagement.ApplicationViewState.Snapped ||
                 Windows.UI.ViewManagement.ApplicationView.TryUnsnap() == true)
            {
                Windows.Storage.Pickers.FileOpenPicker openPicker = new Windows.Storage.Pickers.FileOpenPicker();
                openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;

                // Filter to include a sample subset of file types.
                openPicker.FileTypeFilter.Clear();
                openPicker.FileTypeFilter.Add(".bmp");
                openPicker.FileTypeFilter.Add(".png");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".jpg");

                // Open the file picker.
                Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();

                // file is null if user cancels the file picker.
                if (file != null)
                {
                    // Open a stream for the selected file.
                    Windows.Storage.Streams.IRandomAccessStream fileStream =
                        await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                    // Set the image source to the selected bitmap.
                    Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage =
                        new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                    bitmapImage.SetSource(fileStream);
                    ThreadPoolSample.MainPage.PomidoroImage.Source = bitmapImage;
                    
                    //displayImage.Source = bitmapImage;
                    this.DataContext = file;

                    // Add picked file to MostRecentlyUsedList.
                    //ThreadPoolSample.MainPage.MruToken = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);

                    // Save file
                    Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                    roamingSettings.Values["MruToken"] = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                }
            }
        }

        private void DefaultPomidoroButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset image
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["MruToken"] = null;

            BitmapImage defaultImage = new BitmapImage();
            defaultImage.UriSource = new Uri("ms-appx:///Images/Pomidoro.png");
            ThreadPoolSample.MainPage.PomidoroImage.Source = defaultImage;
        }

        private void Time_TextChanged(object sender, TextChangedEventArgs e)
        {
            int parsed = 0;
            if (int.TryParse(this.Time.Text, out parsed) && parsed>0)
            {
                ThreadPoolSample.DefaultPeriodicTimerCount = parsed * 60;

                if (ThreadPoolSample.PeriodicTimerStatus != Status.Started && ThreadPoolSample.PeriodicTimerStatus != Status.Completed)
                {
                    ThreadPoolSample.PeriodicTimerCount = ThreadPoolSample.DefaultPeriodicTimerCount;
                    ThreadPoolSample.MainPage.UpdateUI(Status.Canceled);
                }
            }

        }
    }
}
