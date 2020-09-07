using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using TFAmvvm.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace TFAmvvm.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private double tickResolution = 1;

        private DispatcherTimer timer;

        #region properties
        public bool IsLoading { get; set; }

        public ObservableCollection<Account> AccountsCollection { get; set; }

        private ListViewSelectionMode selectionMode;

        public ListViewSelectionMode SelectionMode
        {
            get { return selectionMode; }
            set { Set(ref selectionMode, value); }
        }

        private double numberOfSecondsLeft;

        public double NumberOfSecondsLeft
        {
            get { return numberOfSecondsLeft; }
            set { Set(ref numberOfSecondsLeft, value); }
        }

        private IList<object> selectedItems;

        #endregion

        #region commands

        private DelegateCommand addAccountCommand;

        public DelegateCommand AddAccountCommand
        {
            get
            {
                if (addAccountCommand == null)
                {
                    addAccountCommand = new DelegateCommand(() =>
                    {
                        NavigationService.Navigate(App.Pages.AddAccountPage);
                    });
                }
                return addAccountCommand;
            }
        }

        private DelegateCommand manageAccountsCommand;

        public DelegateCommand ManageAccountsCommand
        {
            get
            {
                if (manageAccountsCommand == null)
                {
                    manageAccountsCommand = new DelegateCommand(() =>
                    {
                        ToggleSelectionMode();
                    }, () =>
                    {
                        if (AccountsCollection != null)
                        {
                            return AccountsCollection.Count > 0;
                        }
                        else
                            return false;
                    });
                }
                return manageAccountsCommand;
            }
        }

        private DelegateCommand cancelCommand;

        public new DelegateCommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand = new DelegateCommand(() =>
                    {
                        ToggleSelectionMode();
                    });
                }
                return cancelCommand;
            }
        }

        private DelegateCommand deleteCommand;

        public DelegateCommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new DelegateCommand(async () =>
                    {
                        await App.AccountsModel.DeleteAccounts(selectedItems);
                        ManageAccountsCommand.RaiseCanExecuteChanged();
                        ToggleSelectionMode();
                    }, () =>
                    {
                        return selectedItems != null && selectedItems.Count > 0;
                    });
                }
                return deleteCommand;
            }
        }


        private DelegateCommand aboutCommand;

        public DelegateCommand AboutCommand
        {
            get
            {
                if (aboutCommand == null)
                {
                    aboutCommand = new DelegateCommand(() =>
                    {
                        NavigationService.Navigate(App.Pages.AboutPage);
                    });
                }
                return aboutCommand;
            }
        }

        private DelegateCommand settingsCommand;

        public DelegateCommand SettingsCommand
        {
            get
            {
                if (settingsCommand == null)
                {
                    settingsCommand = new DelegateCommand(() =>
                    {
                        NavigationService.Navigate(App.Pages.SettingsPage);
                    });
                }
                return settingsCommand;
            }
        }

        #endregion

        public MainPageViewModel()
        {
            IsLoading = true;
            SelectionMode = ListViewSelectionMode.None;
            ApplicationData.Current.DataChanged += new Windows.Foundation.TypedEventHandler<ApplicationData, object>(DataChangedHandler);
        }

        public static async Task Init(MainPageViewModel mainPageViewModel)
        {
            //Async initialize accounts model
            if (App.AccountsModel == null)
            {
                App.AccountsModel = await AccountsModel.Init();
            }
            //Get accounts collection
            mainPageViewModel.AccountsCollection = App.AccountsModel.Get();
            mainPageViewModel.IsLoading = false;
            mainPageViewModel.AccountsCollection.CollectionChanged += async (sender, args) =>
            {
                Debug.WriteLine("Collection changed!");
                await mainPageViewModel.DoCountdown();
            };
            await mainPageViewModel.DoCountdown();
        }

        public async Task DoCountdown()
        {
            Task<int> numberOfSecondsTask = App.AccountsModel.NumberOfSecondsLeft();

            if (AccountsCollection.Count > 0 && timer == null)
            {
                //Set remaining seconds in current 30sec interval
                NumberOfSecondsLeft = await numberOfSecondsTask;
                timer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromSeconds(tickResolution)
                };
                timer.Tick += async delegate (object sender, object te)
                {
                    NumberOfSecondsLeft -= tickResolution;
                    if (NumberOfSecondsLeft < 0 && AccountsCollection.Count > 0)
                    {
                        IsLoading = true;
                        await App.AccountsModel.UpdateCodesAsync();
                        NumberOfSecondsLeft = 30;
                    }
                    else if (AccountsCollection.Count <= 0)
                    {
                        timer.Stop();
                    }

                };
                timer.Start();
            }
            else if (AccountsCollection.Count > 0 && timer != null)
            {
                NumberOfSecondsLeft = await numberOfSecondsTask;
                timer.Start();
            }
            else if (AccountsCollection.Count <= 0 && timer != null && timer.IsEnabled)
            {
                NumberOfSecondsLeft = 0;
                timer.Stop();
            }
        }

        public void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedItems = ((GridView)sender).SelectedItems;
            DeleteCommand.RaiseCanExecuteChanged();
        }

        public void ItemClick(object sender, ItemClickEventArgs e)
        {
            if (SelectionMode == ListViewSelectionMode.None)
            {
                var acc = e.ClickedItem as Account;
                DataPackage dataPackage = new DataPackage();

                dataPackage.SetText(acc.Code);
                Clipboard.SetContent(dataPackage);

                var toastContent = new ToastContent()
                {
                    Visual = new ToastVisual()
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = App.loader.GetString("CodeCopiedMessage")
                                },
                                new AdaptiveText()
                                {
                                    Text = acc.Code
                                },
                                new AdaptiveText()
                                {
                                    Text = acc.ShortName
                                }
                            },
                            Attribution = new ToastGenericAttributionText()
                            {
                                Text = acc.AccId
                            }
                        }
                    }
                };

                // Create the toast notification
                var toastNotif = new ToastNotification(toastContent.GetXml());
                toastNotif.ExpirationTime = DateTime.Now.AddSeconds(30);

                // And send the notification
                ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
            }
        }

        private async void DataChangedHandler(ApplicationData sender, object args)
        {
            Debug.WriteLine("Data changed!");
            await this.Dispatcher.DispatchAsync(async () =>
            {
                if (timer != null && timer.IsEnabled)
                {
                    timer.Stop();
                }
                await App.AccountsModel.ReadChanged();
            });
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            return Init(this);
        }

        #region helper methods

        private void ToggleSelectionMode()
        {
            if (SelectionMode == ListViewSelectionMode.None)
            {
                SelectionMode = ListViewSelectionMode.Multiple;
            }
            else if (SelectionMode == ListViewSelectionMode.Multiple)
            {
                SelectionMode = ListViewSelectionMode.None;
            }
        }

        #endregion
    }
}

