using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Template10.Services.SettingsService;
using Windows.Storage;

namespace TFAmvvm.Models
{
    class DataFile
    {
        private static string FileName = "appdata.json";

        public static StorageFolder dataFolder
        {
            get
            {
                if (SettingsService.Roaming.Read(nameof(ViewModels.SettingsPageViewModel.RoamData), true))
                {
                    return ApplicationData.Current.RoamingFolder;
                }
                else
                {
                    return ApplicationData.Current.LocalFolder;
                }
            }
        }


        public static async Task WriteDataFileAsync(ObservableCollection<Account> accounts)
        {
            StorageFile DataFile = await dataFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ObservableCollection<Account>));
            using (Stream stream = await DataFile.OpenStreamForWriteAsync())
            {
                jsonSerializer.WriteObject(stream, accounts);
            }
        }

        public static async Task<ObservableCollection<Account>> ReadDataFileAsync()
        {
            ObservableCollection<Account> accounts;
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ObservableCollection<Account>));
            System.Diagnostics.Debug.WriteLine(Path.Combine(dataFolder.Path, FileName));
            if (File.Exists(Path.Combine(dataFolder.Path, FileName)))
            {
                try
                {
                    using (Stream stream = await dataFolder.OpenStreamForReadAsync(FileName))
                    {
                        accounts = (ObservableCollection<Account>)jsonSerializer.ReadObject(stream);
                    }
                }
                catch (Exception e)
                {
                    //Something bad happened, now return a null collection
                    var dialog = new Windows.UI.Popups.MessageDialog(
                        "Please close the app and restart."
                        + Environment.NewLine
                        + e.Message);
                    dialog.Title = "Something bad happened";
                    accounts = null;
                }
            }
            else
            {
                accounts = new ObservableCollection<Account>();
                System.Diagnostics.Debug.WriteLine("file not found:" + accounts.Count);
            }
            return accounts;
        }

        public static async Task PurgeDataFileAsync()
        {
            var DataFile = await dataFolder.TryGetItemAsync(FileName) as StorageFile;
            if (DataFile != null)
            {
                await DataFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }
    }
}
