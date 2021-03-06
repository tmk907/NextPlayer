﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Globalization.Collation;

namespace NextPlayerDataLayer.Common
{
    public class Grouped
    {
        public static ObservableCollection<GroupedOC<T>> CreateGrouped<T>(IEnumerable<T> InitialItemsList, Func<T, string> selector)
        {
            ObservableCollection<GroupedOC<T>> GroupedItems = new ObservableCollection<GroupedOC<T>>();
            //ObservableCollection<IGrouping<string, T>> GroupedItems2 = new ObservableCollection<IGrouping<string, T>>();
            //ObservableCollection<GroupedOC<T>> GroupedItems2 = new ObservableCollection<GroupedOC<T>>;

            var characterGroupings = new CharacterGroupings();
            //System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            //st.Start();
            //foreach (var q in InitialItemsList.GroupBy(s => characterGroupings.Lookup(selector(s))).Select(s => s).OrderBy(t => t.Key))
            //{
            //    GroupedItems2.Add(q);
            //}
            //foreach (var c in characterGroupings)
            //{
            //    bool found = false;
            //    foreach(var g in GroupedItems2)
            //    {
            //        if (g.Key == c.Label) found = true;
            //    }
            //    if (!found)
            //    {
            //        GroupedItems2.Add(new GroupedOC<T>(c.Label));
            //    }
            //}
            //st.Stop();
            //st.Restart();
            foreach (var c in characterGroupings)
            {
                GroupedItems.Add(new GroupedOC<T>(c.Label));
            }
            foreach (var item in InitialItemsList)
            {
                string a = characterGroupings.Lookup(selector(item));
                GroupedItems.FirstOrDefault(e => e.Key.Equals(a)).Add(item);
            }
            //st.Stop();
            return GroupedItems;
        }
    }

    /// <summary>
    /// Grouped ObservableCollection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GroupedOC<T> : ObservableCollection<T>, IGrouping<string,T>
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
