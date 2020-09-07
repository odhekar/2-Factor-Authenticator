using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TFAmvvm.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            //Define this style here instead of xaml so that acrylic can be added below
            Style itemStyle = new Style(typeof(GridViewItem));
            itemStyle.Setters.Add(new Setter(GridViewItem.MarginProperty, "0,0,0,8"));
            if (App.isFluent)
            {
                Debug.WriteLine("Device is fluent compatible");
                MainGrid.Background = Application.Current.Resources["SystemControlAcrylicWindowBrush"] as AcrylicBrush;
                MainCommandBar.Background = Application.Current.Resources["SystemControlAcrylicElementBrush"] as AcrylicBrush;
                AddAccountButton.Style = Application.Current.Resources["AppBarButtonRevealStyle"] as Style;
                ManageAccountsButton.Style = Application.Current.Resources["AppBarButtonRevealStyle"] as Style;
                DeleteAccountsButton.Style = Application.Current.Resources["AppBarButtonRevealStyle"] as Style;
                SelectAllButton.Style = Application.Current.Resources["AppBarButtonRevealStyle"] as Style;
                CancelDeleteActionButton.Style = Application.Current.Resources["AppBarButtonRevealStyle"] as Style;               
                itemStyle.Setters.Add(new Setter(GridViewItem.BackgroundProperty, Application.Current.Resources["SystemControlChromeHighAcrylicElementMediumBrush"] as AcrylicBrush));
            }
            TwoFactorGrid.ItemContainerStyle = itemStyle;
        }

        #region Helper Methods

        private void ShowAddAccountHint()
        {
            if (TwoFactorGrid.Items.Count == 0 && !ViewModel.IsLoading)
            {
                DispatcherTimer timer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                timer.Tick += delegate (object sender, object e)
                {
                    timer.Stop();
                    if (AddAccountButton.Visibility == Visibility.Visible)
                    {
                        var bounds1 = GetElementBounds(AddAccountButton, MainCommandBar);
                        var bounds2 = GetElementBounds(MainCommandBar, MainGrid);
                        PopupHints.VerticalOffset = bounds2.Top - 38;
                        PopupHints.HorizontalOffset = bounds1.Left + bounds1.Width / 2 - 80;
                        if (TwoFactorGrid.Items.Count == 0)
                            PopupHints.IsOpen = true;
                    }
                };
                timer.Start();
            }
        }

        private Rect GetElementBounds(FrameworkElement element, FrameworkElement parent)
        {
            if (element == null || parent == null)
            {
                return Rect.Empty;
            }
            if (element.Visibility != Visibility.Visible)
            {
                return Rect.Empty;
            }
            return element.TransformToVisual(parent).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
        }

        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ShowAddAccountHint();
        }
        
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            TwoFactorGrid.SelectAll();
        }
    }
}
