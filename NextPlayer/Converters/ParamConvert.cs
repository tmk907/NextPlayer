using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayer.Converters
{
    public class ParamConvert
    {
        private const string delimiter = "#$%";
        private const string nullString = "@^@";

        public static string ToString(String[] array)
        {
            string result = "";
            foreach (string s in array)
            {
                if (s == null)
                {
                    result += nullString;
                }
                else
                {
                    result += s;
                }
                result += delimiter;
            }
            return result;
        }

        public static string[] ToStringArray(string s)
        {
            string[] table = s.Split(new string[] { delimiter }, 5, System.StringSplitOptions.None);
            int i = 0;
            while (i < table.Length)
            {
                if (table[i].Equals(nullString))
                {
                    table[i] = null;
                }
                i++;
            }
            return table;
        }
    }
}
