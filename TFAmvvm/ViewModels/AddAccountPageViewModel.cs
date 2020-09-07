using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Foundation;
using Windows.UI.Xaml.Navigation;
using ZXing.Mobile;

namespace TFAmvvm.ViewModels
{
    class AddAccountPageViewModel : BaseViewModel
    {
        private MobileBarcodeScanner scanner;

        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    Set(ref name, value);
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string secretKey;

        public string SecretKey
        {
            get { return secretKey; }
            set
            {
                Set(ref secretKey, value);
            }
        }

        private string headerMessage;

        public string HeaderMessage
        {
            get { return headerMessage; }
            set
            {
                Set(ref headerMessage, value);
            }
        }

        private DelegateCommand saveCommand;

        public DelegateCommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new DelegateCommand(async () =>
                    {
                        if (await App.AccountsModel.Add(Name, SecretKey))
                        {
                            //Add new account and go back to main page
                            string tmp = App.loader.GetString("SecretKeyTextBox/Header");
                            if (!HeaderMessage.Equals(tmp))
                            {
                                HeaderMessage = tmp;
                            }
                            Name = SecretKey = "";
                            NavigationService.GoBack();
                        }
                        else
                        {
                            //DO something for invalid input
                            HeaderMessage = App.loader.GetString("SecretKeyTextBoxErrorMessage");
                        }
                    }, () =>
                    {
                        return !string.IsNullOrEmpty(Name);
                    });
                }
                return saveCommand;
            }
        }

        private DelegateCommand cameraCommand;

        public DelegateCommand CameraCommand
        {
            get
            {
                if (cameraCommand == null)
                {
                    cameraCommand = new DelegateCommand(async () =>
                    {
                        try
                        {
                            Task<string> scanResult = ScanQRCode();
                            string result = await scanResult;
                            Debug.WriteLine("Back:" + result);
                            //Decode URL to get nice text for account name
                            Uri url = new Uri(System.Net.WebUtility.UrlDecode(result));
                            WwwFormUrlDecoder decoder = new WwwFormUrlDecoder(url.Query);
                            //Remove prefix forward slash from AbsolutePath
                            Name = url.AbsolutePath.Substring(1, url.AbsolutePath.Length - 1);
                            SecretKey = decoder.GetFirstValueByName("secret");
                        }
                        catch(Exception e)
                        {
                            /* DO Nothing, scan was probably cancelled. */
                            Debug.WriteLine("camera crashed");
                            Debug.WriteLine(e.Message);
                            Debug.WriteLine(e.StackTrace);
                        }
                    });
                }
                return cameraCommand;
            }
        }

        private async System.Threading.Tasks.Task<string> ScanQRCode()
        {
            var result = "";
            try
            {
                scanner = new MobileBarcodeScanner()
                {
                    UseCustomOverlay = false,
                    TopText = App.loader.GetString("TitleApp"),
                    BottomText = App.loader.GetString("ScannerBottomText")
                };
                await scanner.Scan().ContinueWith(t => {
                    if (t.Result != null)
                    {
                        result = t.Result.Text;
                    }
                });
            }
            catch(Exception e)
            {
                /* DO Nothing, probably no webcam access */
                result = "Bad QR Code";
                Debug.WriteLine("camera crashed");
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
            return result;
        }

        public AddAccountPageViewModel()
        {
            HeaderMessage = App.loader.GetString("SecretKeyTextBox/Header");
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.Any())
            {
                Name = state[nameof(Name)].ToString();
                SecretKey = state[nameof(SecretKey)].ToString();
                state.Clear();
            }
            else
            {
                // use parameter
            }
            return Task.CompletedTask;
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            if (suspending)
            {
                if (scanner != null)
                {
                    scanner.Cancel();
                    scanner = null;
                }
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    state.Add(nameof(Name), Name);
                }
                if (!string.IsNullOrWhiteSpace(SecretKey))
                {
                    state.Add(nameof(SecretKey), SecretKey);
                }
            }
            return Task.CompletedTask;
        }
    }
}