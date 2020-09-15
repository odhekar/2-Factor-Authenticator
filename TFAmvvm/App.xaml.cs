using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Template10.Common;
using TFAmvvm.Models;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace TFAmvvm
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    sealed partial class App : Template10.Common.BootStrapper
    {
        public static ResourceLoader loader;
        public static AccountsModel AccountsModel { get; set; }
        public static bool isFluent = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush");

        public enum Pages
        {
            MainPage,
            AddAccountPage,
            SettingsPage,
            AboutPage
        }

        public App()
        {
            Debug.WriteLine("FLUENT: " + Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"));
            InitializeComponent();

            loader = new ResourceLoader();

            //Create Page stack for navigation
            var keys = PageKeys<Pages>();
            keys.Add(Pages.MainPage, typeof(Views.MainPage));
            keys.Add(Pages.AddAccountPage, typeof(Views.AddAccountPage));
            keys.Add(Pages.SettingsPage, typeof(Views.SettingsPage));
            keys.Add(Pages.AboutPage, typeof(Views.AboutPage));
        }

        public override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            //Set minimum Window size
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 480));
            return base.OnInitializeAsync(args);
        }

        public override Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            //extendAcrylicIntoTitleBar();
            NavigationService.Navigate(Pages.MainPage);
            return Task.CompletedTask;
        }

        private void extendAcrylicIntoTitleBar()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        public override void OnResuming(object s, object e, AppExecutionState previousExecutionState)
        {
            if (previousExecutionState == AppExecutionState.Suspended && NavigationService.CurrentPageType == typeof(Views.MainPage))
            {
                Debug.WriteLine("Resuming on MainPage");
                NavigationService.LoadAsync();
                Debug.WriteLine("Resumed on MainPage");
            }
        }

    }
}

