using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.ApplicationModel;

namespace TFAmvvm.ViewModels
{
    public class AboutPageViewModel : BaseViewModel
    {
        public string Version
        {
            get
            {
                PackageVersion version = Package.Current.Id.Version;
                return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            }
        }

        private DelegateCommand reviewAppCommand;

        public DelegateCommand ReviewAppCommand
        {
            get
            {
                if (reviewAppCommand == null)
                {
                    reviewAppCommand = new DelegateCommand(async () =>
                    {
                        string storeLink = "ms-windows-store:REVIEW?PFN=" + Package.Current.Id.FamilyName;
                        await Windows.System.Launcher.LaunchUriAsync(new Uri(storeLink));
                    });
                }
                return reviewAppCommand;
            }
        }
    }
}
