using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;
using Callisto.Controls;
using Windows.UI;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Pomidoro
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        //private Color _background = Color.FromArgb(255, 0, 77, 96);
        
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                Pomidoro.Common.SuspensionManager.RegisterFrame(rootFrame, "appFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                    await Pomidoro.Common.SuspensionManager.RestoreAsync();
                }

                // Register handler for CommandsRequested events from the settings pane
                SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            // Add an About command
            var about = new SettingsCommand("about", "About", (handler) =>
            {
                var settings = new SettingsFlyout();
                settings.Content = new AboutUserControl();
                //settings.HeaderBrush = new SolidColorBrush(_background);
                //settings.Background = new SolidColorBrush(_background);                
                settings.HeaderText = "About";
                settings.IsOpen = true;
            });

            args.Request.ApplicationCommands.Add(about);

            // Add an Pomidoro settings
            var pomidoro = new SettingsCommand("pomidoro", "Pomidoro image", (handler) =>
            {
                var settings = new SettingsFlyout();
                settings.Content = new PomidoroUserControl();
                //settings.HeaderBrush = new SolidColorBrush(_background);
                //settings.Background = new SolidColorBrush(_background);
                //settings.Foreground = new SolidColorBrush(Colors.Black);
                settings.HeaderText = "Pomidoro";
                settings.IsOpen = true;
            });

            args.Request.ApplicationCommands.Add(pomidoro);

        }



        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            //TODO: Save application state and stop any background activity
            await Pomidoro.Common.SuspensionManager.SaveAsync();

            deferral.Complete();
        }
    }
}
