using NextPlayer.Constants;
using NextPlayer.Common;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextPlayerDataLayer.Helpers;
using NextPlayer.Converters;
using NextPlayerDataLayer.Enums;
using Windows.ApplicationModel.Resources;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NextPlayer.ViewModel
{
    public class NewSmartPlaylistViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        ResourceLoader loader;
        int smartId;

        public NewSmartPlaylistViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            loader = new ResourceLoader();
            //selectedComparison = "";
            //selectedItem = "";
            //selectedSorting = "";
            //selectedTimeUnit = "";
            //playlistName = "";
            //songsNumber = "25";
            //valueTextBox = "";
        }

        public class ComboBoxItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public ComboBoxItem(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }

        /// <summary>
        /// The <see cref="SortByItems" /> property's name.
        /// </summary>
        public const string SortByItemsPropertyName = "SortByItems";

        private ObservableCollection<ComboBoxItem> sortByItems = new ObservableCollection<ComboBoxItem>();

        /// <summary>
        /// Sets and gets the SortByItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ComboBoxItem> SortByItems
        {
            get
            {
                if (sortByItems.Count == 0)
                {
                    if (IsInDesignMode)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            sortByItems.Add(new ComboBoxItem("abcde" + i.ToString(), "abcdev" + i.ToString()));
                        }
                    }
                    else
                    {
                        foreach (var k in typeof(SPUtility.SortBy).GetRuntimeFields())
                        {
                            object o = k.GetValue(null);
                            if (o is string)
                            {
                                //loader.GetString(o as string)
                                sortByItems.Add(new ComboBoxItem(loader.GetString(o as string), o as string));
                            }
                        }
                    }
                    
                }
                
                return sortByItems;
            }

            set
            {
                if (sortByItems == value)
                {
                    return;
                }

                sortByItems = value;
                RaisePropertyChanged(SortByItemsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedSorting" /> property's name.
        /// </summary>
        public const string SelectedSortingPropertyName = "SelectedSorting";

        private string selectedSorting = "";

        /// <summary>
        /// Sets and gets the SelectedSorting property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedSorting
        {
            get
            {
                return selectedSorting;
            }

            set
            {
                if (selectedSorting == value)
                {
                    return;
                }

                selectedSorting = value;
                RaisePropertyChanged(SelectedSortingPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="SongsNumber" /> property's name.
        /// </summary>
        public const string SongsNumberPropertyName = "SongsNumber";

        private string songsNumber = "";

        /// <summary>
        /// Sets and gets the SongsNumber property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SongsNumber
        {
            get
            {
                return songsNumber;
            }

            set
            {
                if (songsNumber == value)
                {
                    return;
                }
                songsNumber = value;
                //SongsNumberValidation(value as string);
                
                RaisePropertyChanged(SongsNumberPropertyName);
                
            }
        }

        /// <summary>
        /// The <see cref="PlaylistName" /> property's name.
        /// </summary>
        public const string PlaylistNamePropertyName = "PlaylistName";

        private string  playlistName = "";

        /// <summary>
        /// Sets and gets the PlaylistName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string  PlaylistName
        {
            get
            {
                return playlistName;
            }

            set
            {
                if (playlistName == value)
                {
                    return;
                }
                playlistName = value;
                RaisePropertyChanged(PlaylistNamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Items" /> property's name.
        /// </summary>
        public const string ItemsPropertyName = "Items";

        private List<ComboBoxItem> items = new List<ComboBoxItem>();

        /// <summary>
        /// Sets and gets the Items property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<ComboBoxItem> Items
        {
            get
            {
                if (items.Count == 0)
                {
                    if (IsInDesignMode)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            items.Add(new ComboBoxItem("abcde" + i.ToString(), "abcdev" + i.ToString()));
                        }
                    }
                    else
                    {
                        foreach (var k in typeof(SPUtility.Item).GetRuntimeFields())
                        {
                            object o = k.GetValue(null);
                            if (o is string)
                            {
                                items.Add(new ComboBoxItem(loader.GetString(o as string), o as string));
                            }
                        }
                    }

                }
                
                return items;
            }

            set
            {
                if (items == value)
                {
                    return;
                }

                items = value;
                RaisePropertyChanged(ItemsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedItem" /> property's name.
        /// </summary>
        public const string SelectedItemPropertyName = "SelectedItem";

        private string selectedItem = "";

        /// <summary>
        /// Sets and gets the SelectedItem property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedItem
        {
            get
            {
                return selectedItem;
            }

            set
            {
                if (selectedItem == value)
                {
                    return;
                }
                //default  - number. Year, PlayCount, Rating
                DatePickerVisibility = false;
                TextBoxVisibility = true;
                TimeUnitsVisibility = false;
                //ValueInputScope = "Number";
                if (value == SPUtility.Item.Album || value == SPUtility.Item.Artist || value == SPUtility.Item.Genre || value == SPUtility.Item.Title || value == SPUtility.Item.FilePath || value == SPUtility.Item.AlbumArtist || value == SPUtility.Item.Composer)
                {
                    comparisonItems.Clear();
                    SetComparisonItems(true);
                    //ValueInputScope = "String";
                }
                else
                {
                    comparisonItems.Clear();
                    SetComparisonItems(false);
                    if (value == SPUtility.Item.LastPlayed || value == SPUtility.Item.DateAdded)
                    {
                        DatePickerVisibility = true;
                        TextBoxVisibility = false;
                        TimeUnitsVisibility = false;
                        //ValueInputScope = "String";
                    }
                    else if (value == SPUtility.Item.Duration)
                    {
                        DatePickerVisibility = false;
                        TextBoxVisibility = true;
                        TimeUnitsVisibility = true;
                        //ValueInputScope = "Number";
                    }

                }
                selectedItem = value;
                RaisePropertyChanged(SelectedItemPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedComparison" /> property's name.
        /// </summary>
        public const string SelectedComparisonPropertyName = "SelectedComparison";

        private string selectedComparison = "";

        /// <summary>
        /// Sets and gets the SelectedComparison property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedComparison
        {
            get
            {
                return selectedComparison;
            }

            set
            {
                if (selectedComparison == value)
                {
                    return;
                }

                selectedComparison = value;
                RaisePropertyChanged(SelectedComparisonPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ComparisonItems" /> property's name.
        /// </summary>
        public const string ComparisonItemsPropertyName = "ComparisonItems";

        private ObservableCollection<ComboBoxItem> comparisonItems = new ObservableCollection<ComboBoxItem>();

        /// <summary>
        /// Sets and gets the ComparisonItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ComboBoxItem> ComparisonItems
        {
            get
            {
                return comparisonItems;
            }

            set
            {
                if (comparisonItems == value)
                {
                    return;
                }

                comparisonItems = value;
                RaisePropertyChanged(ComparisonItemsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="TextBoxVisibility" /> property's name.
        /// </summary>
        public const string TextBoxVisibilityPropertyName = "TextBoxVisibility";

        private bool textBoxVisibility = true;

        /// <summary>
        /// Sets and gets the TextBoxVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool TextBoxVisibility
        {
            get
            {
                return textBoxVisibility;
            }

            set
            {
                if (textBoxVisibility == value)
                {
                    return;
                }

                textBoxVisibility = value;
                RaisePropertyChanged(TextBoxVisibilityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ValueTextBox" /> property's name.
        /// </summary>
        public const string ValueTextBoxPropertyName = "ValueTextBox";

        private string valueTextBox = "";

        /// <summary>
        /// Sets and gets the ValueTextBox property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ValueTextBox
        {
            get
            {
                return valueTextBox;
            }

            set
            {
                if (valueTextBox == value)
                {
                    return;
                }

                valueTextBox = value;
                RaisePropertyChanged(ValueTextBoxPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="DatePickerVisibility" /> property's name.
        /// </summary>
        public const string DatePickerVisibilityPropertyName = "DatePickerVisibility";

        private bool datePickerVisibility = false;

        /// <summary>
        /// Sets and gets the DatePickerVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool DatePickerVisibility
        {
            get
            {
                return datePickerVisibility;
            }

            set
            {
                if (datePickerVisibility == value)
                {
                    return;
                }

                datePickerVisibility = value;
                RaisePropertyChanged(DatePickerVisibilityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Date" /> property's name.
        /// </summary>
        public const string DatePropertyName = "Date";

        private DateTime date = DateTime.Now;

        /// <summary>
        /// Sets and gets the Date property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public DateTime Date
        {
            get
            {
                return date;
            }

            set
            {
                if (date == value)
                {
                    return;
                }

                date = value;
                RaisePropertyChanged(DatePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="TimeUnits" /> property's name.
        /// </summary>
        public const string TimeUnitsPropertyName = "TimeUnits";

        private ObservableCollection<ComboBoxItem> timeUnits = new ObservableCollection<ComboBoxItem>();

        /// <summary>
        /// Sets and gets the TimeUnits property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ComboBoxItem> TimeUnits
        {
            get
            {
                if (timeUnits.Count == 0)
                {
                    timeUnits.Add(new ComboBoxItem(loader.GetString("Seconds"), "Seconds"));
                    timeUnits.Add(new ComboBoxItem(loader.GetString("Minutes"), "Minutes"));
                    timeUnits.Add(new ComboBoxItem(loader.GetString("Hours"), "Hours"));
                }
                return timeUnits;
            }

            set
            {
                if (timeUnits == value)
                {
                    return;
                }

                timeUnits = value;
                RaisePropertyChanged(TimeUnitsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedTimeUnit" /> property's name.
        /// </summary>
        public const string SelectedTimeUnitPropertyName = "SelectedTimeUnit";

        private string selectedTimeUnit = "";

        public string SelectedTimeUnit
        {
            get
            {
                return selectedTimeUnit;
            }

            set
            {
                if (selectedTimeUnit == value)
                {
                    return;
                }

                selectedTimeUnit = value;
                RaisePropertyChanged(SelectedTimeUnitPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="TimeUnitsVisibility" /> property's name.
        /// </summary>
        public const string TimeUnitsVisibilityPropertyName = "TimeUnitsVisibility";

        private bool timeUnitsVisibility = true;

        public bool TimeUnitsVisibility
        {
            get
            {
                return timeUnitsVisibility;
            }

            set
            {
                if (timeUnitsVisibility == value)
                {
                    return;
                }

                timeUnitsVisibility = value;
                RaisePropertyChanged(TimeUnitsVisibilityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ValueInputScope" /> property's name.
        /// </summary>
        public const string ValueInputScopePropertyName = "ValueInputScope";

        private string valueInputScope = "Default";

        /// <summary>
        /// Sets and gets the ValueInputScope property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ValueInputScope
        {
            get
            {
                return valueInputScope;
            }

            set
            {
                if (valueInputScope == value)
                {
                    return;
                }

                valueInputScope = value;
                RaisePropertyChanged(ValueInputScopePropertyName);
            }
        }

        private void SetComparisonItems(bool stringCompare)
        {
            if (stringCompare)
            {
                comparisonItems.Add(new ComboBoxItem(loader.GetString(SPUtility.Comparison.Contains), SPUtility.Comparison.Contains));
                comparisonItems.Add(new ComboBoxItem(loader.GetString(SPUtility.Comparison.DoesNotContain), SPUtility.Comparison.DoesNotContain));
                comparisonItems.Add(new ComboBoxItem(loader.GetString(SPUtility.Comparison.StartsWith), SPUtility.Comparison.StartsWith));
                comparisonItems.Add(new ComboBoxItem(loader.GetString(SPUtility.Comparison.EndsWith), SPUtility.Comparison.EndsWith));
            }
            else
            {
                comparisonItems.Add(new ComboBoxItem(loader.GetString(SPUtility.Comparison.Is), SPUtility.Comparison.Is));
                comparisonItems.Add(new ComboBoxItem(loader.GetString(SPUtility.Comparison.IsNot), SPUtility.Comparison.IsNot));
                comparisonItems.Add(new ComboBoxItem(loader.GetString(SPUtility.Comparison.IsLess), SPUtility.Comparison.IsLess));
                comparisonItems.Add(new ComboBoxItem(loader.GetString(SPUtility.Comparison.IsGreater), SPUtility.Comparison.IsGreater));
            }
        }


        private RelayCommand cancel_Click;
        
        public RelayCommand Cancel_Click
        {
            get
            {
                return cancel_Click
                    ?? (cancel_Click = new RelayCommand(
                    () =>
                    {
                        navigationService.GoBack();
                    }));
            }
        }









        private bool IsStringType(string value)
        {
            return (value == SPUtility.Item.Album || value == SPUtility.Item.Artist || value == SPUtility.Item.Genre || value == SPUtility.Item.Title || value == SPUtility.Item.FilePath || value == SPUtility.Item.AlbumArtist || value == SPUtility.Item.Composer);
        }
        private bool IsDateTimeType(string value)
        {
            return (value == SPUtility.Item.LastPlayed || value == SPUtility.Item.DateAdded);
        }
        private bool IsTimeSpanType(string value)
        {
            return (value == SPUtility.Item.Duration);
        }
        private bool IsNumberType(string value)
        {
            return (value == SPUtility.Item.PlayCount || value == SPUtility.Item.Rating || value == SPUtility.Item.Year);
        }

        private bool AreComboBoxesSelected()
        {
            bool selected = true;
            if (selectedComparison == "") selected = false;
            if (selectedItem == "") selected = false;
            if (selectedSorting == "") selected = false;
            if (selectedItem == SPUtility.Item.Duration && selectedTimeUnit == "") selected = false;
            return selected;
        }

        private bool NumberValidation(string value)
        {
            bool isNumber = false;
            int number;
            isNumber = Int32.TryParse(value,out number);
            if (isNumber && number < 0) isNumber = false;
            return isNumber;
        }

        public bool IsCorrect()
        {
            bool correct = true;
            if (!NumberValidation(songsNumber)) correct=false;
            if (!AreComboBoxesSelected()) correct = false;
            if (playlistName == "") correct = false;

            if ((IsNumberType(selectedItem) || IsTimeSpanType(selectedItem)) && !NumberValidation(valueTextBox)) correct = false;
            //if (IsDateTimeType(selectedItem))
            //{
            //    if (date) correct = false;
            //}
            if (IsStringType(selectedItem) && valueTextBox == "") correct = false;
            return correct;
        }

        public async Task SaveSmartPlaylist()
        {
            //int number = Int32.Parse(songsNumber);
            int id = await DatabaseManager.InsertSmartPlaylist(playlistName, Int32.Parse(songsNumber), selectedSorting);
            string value = "";
            if (IsStringType(selectedItem) || IsNumberType(selectedItem))
            {
                value = valueTextBox;
                await DatabaseManager.InsertSmartPlaylistEntry(id, selectedItem, selectedComparison, value);
            }
            else if (IsDateTimeType(selectedItem))
            {
                if (selectedComparison.Equals(SPUtility.Comparison.Is.ToString()))
                {
                    // low < date < high
                    var lowDate = date.Date.Ticks.ToString();
                    var highDate = (date.Date.AddDays(1).Ticks-1).ToString();
                    await DatabaseManager.InsertSmartPlaylistEntry(id, selectedItem, SPUtility.Comparison.IsGreater, lowDate);
                    await DatabaseManager.InsertSmartPlaylistEntry(id, selectedItem, SPUtility.Comparison.IsLess, highDate);
                }
                else if (selectedComparison.Equals(SPUtility.Comparison.IsNot.ToString()))//!
                {
                    // date < low || date > high
                    var lowDate = date.Date.Ticks.ToString();
                    var highDate = (date.Date.AddDays(1).Ticks-1).ToString();
                    await DatabaseManager.InsertSmartPlaylistEntry(id, selectedItem, SPUtility.ComparisonEx.IsLessOR, lowDate);
                    await DatabaseManager.InsertSmartPlaylistEntry(id, selectedItem, SPUtility.ComparisonEx.IsGreaterOR, highDate);
                }
                else if (selectedComparison.Equals(SPUtility.Comparison.IsGreater.ToString()))
                {
                    // high < date
                    value = date.Date.AddDays(1).Ticks.ToString();
                    await DatabaseManager.InsertSmartPlaylistEntry(id, selectedItem, selectedComparison, value);
                }
                else if (selectedComparison.Equals(SPUtility.Comparison.IsLess.ToString()))
                {
                    // date < low
                    value = date.Date.Ticks.ToString();
                    await DatabaseManager.InsertSmartPlaylistEntry(id, selectedItem, selectedComparison, value);
                }
            }
            else if (IsTimeSpanType(selectedItem))
            {
                TimeSpan time;
                if (selectedTimeUnit == "Seconds") time = TimeSpan.FromSeconds(Int32.Parse(valueTextBox));
                else if (selectedTimeUnit == "Minutes") time = TimeSpan.FromMinutes(Int32.Parse(valueTextBox));
                else if (selectedTimeUnit == "Hours") time = TimeSpan.FromHours(Int32.Parse(valueTextBox));

                if (selectedComparison.Equals(SPUtility.Comparison.Is.ToString()))
                {
                    string lowTime, highTime;
                    lowTime = time.Ticks.ToString();
                    if (selectedTimeUnit == "Seconds")
                    {
                        highTime = (time.Add(TimeSpan.FromSeconds(1)).Ticks - 1).ToString();
                    }
                    else if (selectedTimeUnit == "Minutes")
                    {
                        highTime = (time.Add(TimeSpan.FromMinutes(1)).Ticks - 1).ToString();
                    }
                    else// if (selectedTimeUnit == "Hours")
                    {
                        highTime = (time.Add(TimeSpan.FromHours(1)).Ticks - 1).ToString();
                    }
                    await DatabaseManager.InsertSmartPlaylistEntry(id, selectedItem, SPUtility.Comparison.IsGreater, lowTime);
                    await DatabaseManager.InsertSmartPlaylistEntry(id, selectedItem, SPUtility.Comparison.IsLess, highTime);
                }
                else if (selectedComparison.Equals(SPUtility.Comparison.IsNot.ToString()))//!
                {
                    string lowTime, highTime;
                    lowTime = time.Ticks.ToString();
                    if (selectedTimeUnit == "Seconds")
                    {
                        highTime = (time.Add(TimeSpan.FromSeconds(1)).Ticks - 1).ToString();
                    }
                    else if (selectedTimeUnit == "Minutes")
                    {
                        highTime = (time.Add(TimeSpan.FromMinutes(1)).Ticks - 1).ToString();
                    }
                    else// if (selectedTimeUnit == "Hours")
                    {
                        highTime = (time.Add(TimeSpan.FromHours(1)).Ticks - 1).ToString();
                    }
                    await DatabaseManager.InsertSmartPlaylistEntry(id, selectedItem, SPUtility.ComparisonEx.IsLessOR, lowTime);
                    await DatabaseManager.InsertSmartPlaylistEntry(id, selectedItem, SPUtility.ComparisonEx.IsGreaterOR, highTime);
                }
                else
                {
                    value = time.Ticks.ToString();
                    await DatabaseManager.InsertSmartPlaylistEntry(id, selectedItem, selectedComparison, value);
                }
            }
            navigationService.GoBack();
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            selectedComparison = "";
            selectedItem = "";
            selectedSorting = "";
            selectedTimeUnit = "";
            playlistName = "";
            songsNumber = "";
            valueTextBox = "";
            Date = DateTime.Now.Date;
            smartId = -1;
            //if (parameter != null)
            //{
            //    smartId = Int32.Parse(parameter.ToString());
            //}
        }

        public void Deactivate(Dictionary<string, object> state)
        {
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}