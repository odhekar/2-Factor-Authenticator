using Template10.Services.SettingsService;

namespace TFAmvvm.ViewModels
{
    class SettingsPageViewModel : BaseViewModel
    {
        private bool roamData;

        public bool RoamData
        {
            get
            {
                return roamData;
            }
            set
            {
                Set(ref roamData, value);
                SettingsService.Roaming.Write(nameof(RoamData), roamData);
            }
        }

        private bool sortByName;

        public bool SortByName
        {
            get
            {
                return sortByName;
            }
            set
            {
                Set(ref sortByName, value);
                SettingsService.Roaming.Write(nameof(SortByName), sortByName);
            }
        }

        public SettingsPageViewModel()
        {
            RoamData = SettingsService.Roaming.Read(nameof(RoamData), true);
            SortByName = SettingsService.Roaming.Read(nameof(SortByName), true);
        }
    }
}
