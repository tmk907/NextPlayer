using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace NextPlayer.Converters
{
    public class StringToInputScopeConverter : IValueConverter
    {
        // nie działa
        // https://social.msdn.microsoft.com/Forums/windowsapps/en-us/21adb8bc-2ecd-40d6-819a-1e632133a150/textbox-inputscope-is-not-working
        public object Convert(object value, Type targetType, object parameter, string language)  
        {
            string type = value as string;
            InputScope scope = new InputScope();
            InputScopeName name;

            if (type == "String")
            {
                name = new InputScopeName(InputScopeNameValue.AlphanumericFullWidth);
            }
            else if (type == "Number")
            {
                name = new InputScopeName(InputScopeNameValue.Number);
            }
            else
            {
                name = new InputScopeName(InputScopeNameValue.Default);
            }
            scope.Names.Add(name);
            return scope;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
