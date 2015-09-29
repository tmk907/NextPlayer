using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUniversal.Converters
{
    public class ParamConvert
    {
        private const string delimiter = "#$%";

        public static string ToString(String[] array)
        {
            string result = "";
            foreach (string s in array)
            {
                result += s;
                result += delimiter;
            }
            return result;
        }

        public static string[] ToStringArray(string s)
        {
            return s.Split(new string[] { delimiter }, 5, System.StringSplitOptions.None);
        }
    }
}
