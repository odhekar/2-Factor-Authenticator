using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.SettingsService;
using TFAmvvm.ThirdParty;
using TFAmvvm.ViewModels;

namespace TFAmvvm.Models
{
    public class AccountsModel
    {
        private CodeGenerator codeGenerator;

        private ObservableCollection<Account> accountsCollection;

        private int index
        {
            get
            {
                if (accountsCollection != null && accountsCollection.Count > 0)
                {
                    IEnumerable<int> indexArray = accountsCollection.Select(acc => acc.Index);
                    return indexArray.Max() + 1;
                }
                else
                    return 1;
            }
        }

        //Can't do async constructor and hence make it private and use another method to get instance
        private AccountsModel()
        {
            codeGenerator = new CodeGenerator();
        }

        //Use this as constructor
        public static async Task<AccountsModel> Init()
        {
            AccountsModel accountsModel = new AccountsModel();
            await accountsModel.Read();
            return accountsModel;
        }

        #region CRUD methods

        public async Task<bool> Add(string name, string secretKey)
        {
            string code = codeGenerator.ComputeCode(secretKey);
            int tmpCode;
            if (Int32.TryParse(code, out tmpCode))
            {
                Account acc = new Account(index, name, secretKey, code);
                accountsCollection.Add(acc);
                Sort();
                await Write();
                return true;
            }
            else
                return false;
        }

        public Account Get(string name)
        {
            return accountsCollection.FirstOrDefault(acc => acc.Name.Equals(name));
        }

        public Account Get(string name, string secretKey)
        {
            return accountsCollection.FirstOrDefault(acc => acc.Name.Equals(name) && acc.SecretKey.Equals(secretKey));
        }

        public ObservableCollection<Account> Get()
        {
            Sort();
            return accountsCollection;
        }

        public async Task Read()
        {
            accountsCollection = await DataFile.ReadDataFileAsync();
            ProcessName();
            Sort();
            await UpdateCodesAsync();
        }

        public async Task ReadChanged()
        {
            ObservableCollection<Account> newCollection = await DataFile.ReadDataFileAsync();
            ProcessName();
            if (SettingsService.Roaming.Read(nameof(SettingsPageViewModel.SortByName), true))
            {
                newCollection = new ObservableCollection<Account>(newCollection.OrderBy(Account => Account.Name));
            }
            else
            {
                newCollection = new ObservableCollection<Account>(newCollection.OrderBy(Account => Account.Index));
            }
            accountsCollection.Clear();
            foreach(Account acc in newCollection)
            {
                accountsCollection.Add(acc);
            }
            await UpdateCodesAsync();
        }

        public async Task UpdateCodesAsync()
        {
            if (accountsCollection != null)
            {
                await Task.WhenAll(accountsCollection.Select(acc => UpdateCode(acc)));
            }
        }

        public async Task UpdateCode(Account acc)
        {
            acc.Code = await codeGenerator.ComputeCodeAsync(acc.SecretKey);
        }

        public async Task<bool> DeleteAccount(Account acc)
        {
            if (acc != null)
            {
                accountsCollection.Remove(acc);
                await Write();
                return true;
            }
            else
                return false;
        }

        public async Task DeleteAccounts(IList<object> selection)
        {
            List<object> removeList = selection.Where(x => selection.Contains(x)).ToList();
            removeList.ForEach(x => accountsCollection.Remove((Account)x));
            await Write();
        }

        public async Task<bool> DeleteAccount(string name)
        {
            return await DeleteAccount(Get(name));
        }

        public async Task<bool> DeleteAccount(string name, string secretKey)
        {
            return await DeleteAccount(Get(name, secretKey));
        }

        #endregion

        #region Helper methods

        public async Task Write()
        {
            await DataFile.WriteDataFileAsync(this.accountsCollection);
        }

        private void Sort()
        {
            if (SettingsService.Roaming.Read(nameof(SettingsPageViewModel.SortByName), true))
            {
                accountsCollection = new ObservableCollection<Account>(accountsCollection.OrderBy(Account => Account.Name));
            }
            else
            {
                accountsCollection = new ObservableCollection<Account>(accountsCollection.OrderBy(Account => Account.Index));
            }
        }

        public async Task<int> NumberOfSecondsLeft()
        {
            return await codeGenerator.NumberSecondsLeft();
        }

        private void ProcessName()
        {
            foreach(Account acc in accountsCollection) 
            {
                acc.ShortName = acc.Name;
                acc.AccId = acc.Name;
            }
        }
        #endregion
    }
}
