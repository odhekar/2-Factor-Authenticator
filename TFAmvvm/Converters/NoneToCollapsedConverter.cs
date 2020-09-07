using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TFAmvvm.Converters
{
    public class NoneToCollapsedConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            switch (value.ToString().ToLower())
            {
                case "none":
                    return Visibility.Collapsed;
                case "multiple":
                case "single":
                    return Visibility.Visible;
            }
            return false;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
