using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TFAmvvm.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddAccountPage : Page
    {
        public AddAccountPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            if (App.isFluent)
            {
                //SystemControlChromeHighAcrylicWindowMediumBrush
                MainGrid.Background = Application.Current.Resources["SystemControlAcrylicWindowBrush"] as AcrylicBrush;
                MainCommandBar.Background = Application.Current.Resources["SystemControlBaseHighAcrylicElementMediumHighBrush"] as AcrylicBrush;
                CameraButton.Style = Application.Current.Resources["AppBarButtonRevealStyle"] as Style;
                SaveButton.Style = Application.Current.Resources["AppBarButtonRevealStyle"] as Style;
                CancelButton.Style = Application.Current.Resources["AppBarButtonRevealStyle"] as Style;
            }
        }
    }
}
