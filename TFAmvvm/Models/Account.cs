using GalaSoft.MvvmLight;
using System.Runtime.Serialization;

namespace TFAmvvm.Models
{
    public class Account : ObservableObject
    {
        [IgnoreDataMember]
        private string name;

        [IgnoreDataMember]
        private string secretKey;

        [IgnoreDataMember]
        private string shortName;

        [IgnoreDataMember]
        private string accId;

        [IgnoreDataMember]
        private string code;

        public string Name
        {
            get { return name; }
            set
            {
                Set(ref name, value);
            }
        }

        public string SecretKey
        {
            get { return secretKey; }
            set
            {
                Set(ref secretKey, value);
            }
        }

        public int Index { get; set; }

        [IgnoreDataMember]
        public string ShortName
        {
            get { return shortName; }
            set
            {
                shortName = value.Contains(":") ? value.Split(':')[0].Trim() : value;
            }
        }

        [IgnoreDataMember]
        public string AccId
        {
            get { return accId; }
            set
            {
                if (value.Contains(":"))
                {
                    string[] arr = value.Split(':');
                    accId = (arr.Length == 2) ? arr[1].Trim() : arr[0].Trim();
                }
                else
                {
                    accId = value;
                }
            }
        }

        [IgnoreDataMember]
        public string Code
        {
            get { return code; }
            set
            {
                Set(ref code, value);
            }
        }


        //Required for serliazing
        public Account()
        { }

        public Account(int index, string name, string secretKey, string code)
        {
            Index = index;
            Name = name;
            SecretKey = secretKey;
            Code = code;
            ShortName = name;
            AccId = name;
        }

    }
}
