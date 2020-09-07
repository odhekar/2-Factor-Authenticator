using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace TFAmvvm.ViewModels
{
    public class BaseViewModel : ViewModelBase
    {
        public Visibility ShowAppTitle
        {
            get
            {
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop"
                    && UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse)
                {
                    return Visibility.Collapsed;
                }
                else
                    return Visibility.Visible;
            }
        }

        private DelegateCommand cancelCommand;

        public DelegateCommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand = new DelegateCommand(() =>
                    {
                        NavigationService.GoBack();
                    });
                }
                return cancelCommand;
            }
        }



    }
}
