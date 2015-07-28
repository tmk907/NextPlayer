using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Windows.Globalization.Collation;

namespace NextPlayerDataLayer.Common
{
    public class Grouped
    {
        public static ObservableCollection<GroupedOC<T>> CreateGrouped<T>(IEnumerable<T> InitialItemsList, Func<T, string> selector)
        {
            ObservableCollection<GroupedOC<T>> GroupedItems = new ObservableCollection<GroupedOC<T>>();
            var characterGroupings = new CharacterGroupings();
            foreach (var c in characterGroupings)
            {
                GroupedItems.Add(new GroupedOC<T>(c.Label));
            }
            
            foreach (var item in InitialItemsList)
            {
                string a = characterGroupings.Lookup(selector(item));
                GroupedItems.FirstOrDefault(e => e.Key.Equals(a)).Add(item);
            }
            return GroupedItems;
        }
    }

    /// <summary>
    /// Grouped ObservableCollection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GroupedOC<T> : ObservableCollection<T>
    {
        /// <summary>
        /// The Group Title
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        public GroupedOC(string name)
        {
            this.Key = name;
        }

        /// <summary>
        /// Returns true if the group has a count more than zero
        /// </summary>
        public bool HasItems
        {
            get
            {
                return (Count != 0);
            }
            private set
            {
            }
        }
    }
}
