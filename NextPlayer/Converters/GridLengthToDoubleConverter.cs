using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace NextPlayer.Converters
{
    public class GridLengthToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            GridLength val = (GridLength)value;

            return val.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double val = (double)value;
            GridLength gridLength = new GridLength(val);

            return gridLength;
        }
    }
}
