﻿using NextPlayer.Constants;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextPlayerDataLayer.Helpers;
using NextPlayer.Converters;
using Windows.UI.Xaml.Controls;

namespace NextPlayer.ViewModel
{
    public class PlaylistViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private string genre;
        private string folderName;
        private string directory;
        private string name;
        private bool isSmart;
        private int id;
        private int index;
        private bool ascending;
        private ListView listView;

        public PlaylistViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;

            App.SongUpdated += new SongUpdatedHandler(OnSongUpdated);
        }
        
        private void OnSongUpdated(int id)
        {
            bool find = false;
            foreach (var song in playlist)
            {
                if (song.SongId == id)
                {
                    find = true;
                    break;
                }
            }
            if (!find) return;

            Playlist.Clear();
            if (isNowPlaying)
            {
                LoadNowPlayingPlaylist();
            }
            else
            {
                if (genre == null && folderName == null)
                {
                    if (isSmart)
                    {
                        LoadSmartPlaylist();
                    }
                    else
                    {
                        Playlist = DatabaseManager.GetSongItemsFromPlainPlaylist(id);
                    }
                }
                else if (genre != null)
                {
                    LoadGenrePlaylist();
                }
                else if (folderName != null)
                {
                    LoadFolderPlaylist();
                }
            }
        }

        /// <summary>
        /// The <see cref="PageTitle" /> property's name.
        /// </summary>
        public const string PageTitlePropertyName = "PageTitle";

        private string pageTitle = "now playing";

        /// <summary>
        /// Sets and gets the PageTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PageTitle
        {
            get
            {
                return pageTitle;
            }

            set
            {
                if (pageTitle == value)
                {
                    return;
                }

                pageTitle = value;
                RaisePropertyChanged(PageTitlePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Playlist" /> property's name.
        /// </summary>
        public const string PlaylistPropertyName = "Playlist";

        private ObservableCollection<SongItem> playlist = new ObservableCollection<SongItem>();

        /// <summary>
        /// Sets and gets the Playlist property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SongItem> Playlist
        {
            get
            {
                if (IsInDesignMode)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        playlist.Add(new SongItem());
                    }
                }
                return playlist;
            }

            set
            {
                if (playlist == value)
                {
                    return;
                }

                playlist = value;
                RaisePropertyChanged(PlaylistPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsNowPlaying" /> property's name.
        /// </summary>
        public const string IsNowPlayingPropertyName = "IsNowPlaying";

        private bool isNowPlaying = false;

        /// <summary>
        /// Sets and gets the IsNowPlaying property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsNowPlaying
        {
            get
            {
                return isNowPlaying;
            }

            set
            {
                if (isNowPlaying == value)
                {
                    return;
                }

                isNowPlaying = value;
                RaisePropertyChanged(IsNowPlayingPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsDeletable" /> property's name.
        /// </summary>
        public const string IsDeletablePropertyName = "IsDeletable";

        private bool isDeletable = false;

        /// <summary>
        /// Sets and gets the IsDeletable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsDeletable
        {
            get
            {
                return isDeletable;
            }

            set
            {
                if (isDeletable == value)
                {
                    return;
                }

                isDeletable = value;
                RaisePropertyChanged(IsDeletablePropertyName);
            }
        }

        private RelayCommand<SongItem> addToNowPlaying;

        /// <summary>
        /// Gets the AddToNowPlaying.
        /// </summary>
        public RelayCommand<SongItem> AddToNowPlaying
        {
            get
            {
                return addToNowPlaying
                    ?? (addToNowPlaying = new RelayCommand<SongItem>(
                    item =>
                    {
                        Library.Current.AddToNowPlaying(item);
                    }));
            }
        }

        private RelayCommand<SongItem> addToPlaylist;

        /// <summary>
        /// Gets the AddToPlaylist.
        /// </summary>
        public RelayCommand<SongItem> AddToPlaylist
        {
            get
            {
                return addToPlaylist
                    ?? (addToPlaylist = new RelayCommand<SongItem>(
                    item =>
                    {
                        index = SelectedIndex(item);
                        String[] s = new String[2];
                        s[0] = "song";
                        s[1] = item.SongId.ToString();
                        navigationService.NavigateTo(ViewNames.AddToPlaylistView, ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand<SongItem> deleteFromPlaylist;

        /// <summary>
        /// Gets the DeleteFromPlaylist.
        /// </summary>
        public RelayCommand<SongItem> DeleteFromPlaylist
        {
            get
            {
                return deleteFromPlaylist
                    ?? (deleteFromPlaylist = new RelayCommand<SongItem>(
                    item =>
                    {
                        if (genre == null && folderName == null && isSmart == false)//jest np lub plain
                        {
                            if (isNowPlaying)
                            {
                                int i = playlist.IndexOf(item);
                                if (i <= index)
                                {
                                    index--;
                                    ApplicationSettingsHelper.SaveSongIndex(index);
                                }
                                Playlist.Remove(item);
                                Library.Current.SetNowPlayingList(Playlist);
                                NPChange.SendMessageNPSorted();
                            }
                            else//plain playlist
                            {
                                Playlist.Remove(item);
                                DatabaseManager.DeletePlainPlaylistEntryById(item.SongId);
                            }
                        }
                        
                    }));
            }
        }

        private RelayCommand<SongItem> showDetails;

        /// <summary>
        /// Gets the ShowDetails.
        /// </summary>
        public RelayCommand<SongItem> ShowDetails
        {
            get
            {
                return showDetails
                    ?? (showDetails = new RelayCommand<SongItem>(
                    item =>
                    {
                        index = SelectedIndex(item);
                        navigationService.NavigateTo(ViewNames.FileInfoView, item.SongId);
                    }));
            }
        }

        private RelayCommand<SongItem> editTags;

        /// <summary>
        /// Gets the EditTags.
        /// </summary>
        public RelayCommand<SongItem> EditTags
        {
            get
            {
                return editTags
                    ?? (editTags = new RelayCommand<SongItem>(
                    item =>
                    {
                        index = SelectedIndex(item);
                        navigationService.NavigateTo(ViewNames.TagsEditor, item.SongId);
                    }));
            }
        }

        private RelayCommand<SongItem> share;

        /// <summary>
        /// Gets the Share.
        /// </summary>
        public RelayCommand<SongItem> Share
        {
            get
            {
                return share
                    ?? (share = new RelayCommand<SongItem>(
                    item =>
                    {
                        index = SelectedIndex(item);
                        String[] s = new String[2];
                        s[0] = "song";
                        s[1] = item.SongId.ToString();
                        navigationService.NavigateTo(ViewNames.BluetoothShare, ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand<object> loadAndScroll;

        /// <summary>
        /// Gets the LoadAndScroll.
        /// </summary>
        public RelayCommand<object> LoadAndScroll
        {
            get
            {
                return loadAndScroll 
                    ?? (loadAndScroll = new RelayCommand<object>(
                    p =>
                    {
                        listView = (ListView)p;
                        var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                        if (isNowPlaying)
                        {
                            
                            //Playlist = await DatabaseManager.SelectAllSongItemsFromNowPlaying();
                            LoadNowPlayingPlaylist();
                            PageTitle = loader.GetString("NowPlayingPlaylistPageTitle");
                        }
                        else
                        {
                            if (genre == null && folderName == null)
                            {
                                if (isSmart)
                                {
                                    LoadSmartPlaylist();
                                }
                                else
                                {
                                    Playlist = DatabaseManager.GetSongItemsFromPlainPlaylist(id);
                                }
                                PageTitle = name.ToLower();
                            }
                            else if (genre != null)
                            {
                                LoadGenrePlaylist();
                                PageTitle = genre.ToLower();
                                if (genre == "") PageTitle = loader.GetString("UnknownGenre").ToLower();
                            }
                            else if (folderName != null)
                            {
                                LoadFolderPlaylist();
                                PageTitle = folderName.ToLower();
                            }
                        }
                    }));
            }
        }

        private void Scroll()
        {
            if (listView.Items.Count > 0 && index > 0)
            {
                listView.SelectedIndex = index;
                listView.UpdateLayout();
                listView.ScrollIntoView(listView.SelectedItem, ScrollIntoViewAlignment.Leading);
            }
        }

        private RelayCommand<string> sortBy;

        /// <summary>
        /// Gets the SortBy.
        /// </summary>
        public RelayCommand<string> SortBy
        {
            get
            {
                return sortBy
                    ?? (sortBy = new RelayCommand<string>(
                    item =>
                    {
                        ascending = true;
                        switch (item)
                        {
                            case "Title":
                                Sort(s => s.Title);
                                break;
                            case "Artist":
                                Sort(s => s.Artist);
                                break;
                            case "Album":
                                Sort(s => s.Album);
                                break;
                            case "Track":
                                Sort(s => s.TrackNumber);
                                break;
                            case "Rating":
                                Sort(s => s.Rating);
                                break;
                            case "Duration":
                                Sort(s => s.Duration);
                                break;
                            case "Composer":
                                Sort(s => s.Composer);
                                break;
                            case "Year":
                                Sort(s => s.Year);
                                break;
                        }
                    }));
            }
        }

        private RelayCommand changeOrder;

        /// <summary>
        /// Gets the ChangeOrder.
        /// </summary>
        public RelayCommand ChangeOrder
        {
            get
            {
                return changeOrder
                    ?? (changeOrder = new RelayCommand(
                    () =>
                    {
                        ascending = !ascending;
                        Playlist = new ObservableCollection<SongItem>(playlist.Reverse());
                        if (isNowPlaying)
                        {
                            index = (playlist.Count - 1) - index;
                            ApplicationSettingsHelper.SaveSongIndex(index);
                            Library.Current.SetNowPlayingList(Playlist);
                            NPChange.SendMessageNPSorted();
                        }
                    }));
            }
        }

        private void Sort(Func<SongItem, object> selector)
        {
            int id = -1;
            if (isNowPlaying)
            {
                if (playlist.Count == 0) return;
                id = playlist.ElementAt(index).SongId;
            }
            if (ascending)
            {
                Playlist = new ObservableCollection<SongItem>(playlist.OrderBy(selector));
            }
            else
            {
                Playlist = new ObservableCollection<SongItem>(playlist.OrderByDescending(selector));
            }
            if (isNowPlaying)
            {
                int i = 0;
                foreach (var item in playlist)
                {
                    if (item.SongId==id) break;
                    i++;
                }
                index = i;
                ApplicationSettingsHelper.SaveSongIndex(index);
                Library.Current.SetNowPlayingList(Playlist);
                NPChange.SendMessageNPSorted();
            }
        }

        private RelayCommand saveAsPlaylist;

        /// <summary>
        /// Gets the SaveAsPlaylist.
        /// </summary>
        public RelayCommand SaveAsPlaylist
        {
            get
            {
                return saveAsPlaylist
                    ?? (saveAsPlaylist = new RelayCommand(
                    () =>
                    {
                        String[] s = new String[1];
                        s[0] = "nowPlaying";
                        navigationService.NavigateTo(ViewNames.AddToPlaylistView, ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand<SongItem> itemClicked;

        /// <summary>
        /// Gets the ItemClicked.
        /// </summary>
        public RelayCommand<SongItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<SongItem>(
                    item =>
                    {
                        int index = 0;
                        foreach(var song in Playlist)
                        {
                            if (song.SongId == item.SongId) break;
                            index++;
                        }
                        ApplicationSettingsHelper.SaveSongIndex(index);
                        Library.Current.SetNowPlayingList(Playlist);
                        if (NextPlayer.Common.SuspensionManager.SessionState.ContainsKey("nplist"))
                        {
                            NextPlayer.Common.SuspensionManager.SessionState.Remove("nplist");
                            navigationService.GoBack();
                        }
                        else
                        {
                            navigationService.NavigateTo(ViewNames.NowPlayingView, item.SongId);
                        }
                    }));
            }
        }

        private RelayCommand loadItems;

        /// <summary>
        /// Gets the LoadItems.
        /// </summary>
        public RelayCommand LoadItems
        {
            get
            {
                return loadItems
                    ?? (loadItems = new RelayCommand(
                    () =>
                    {
                        
                        if (isNowPlaying)
                        {
                            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                            //Playlist = await DatabaseManager.SelectAllSongItemsFromNowPlaying();
                            LoadNowPlayingPlaylist();
                            PageTitle = loader.GetString("NowPlayingPlaylistPageTitle");
                        }
                        else
                        {
                            if (genre == null && folderName==null)
                            {
                                if (isSmart) 
                                {
                                    LoadSmartPlaylist();
                                }
                                else
                                {
                                    Playlist = DatabaseManager.GetSongItemsFromPlainPlaylist(id);
                                }
                                PageTitle = name.ToLower();
                            }
                            else if (genre!=null)
                            {
                                LoadGenrePlaylist();   
                                PageTitle = genre.ToLower();
                            }
                            else if (folderName != null)
                            {
                                LoadFolderPlaylist();
                                PageTitle = folderName.ToLower();
                            }
                        }
                    }));
            }
        }
        private async void LoadNowPlayingPlaylist()
        {
            foreach (var x in Library.Current.NowPlayingList)
            {
                Playlist.Add(x);
            }
            Scroll();
            //Playlist = Library.Current.NowPlayingList;
            //Playlist = await DatabaseManager.SelectAllSongItemsFromNowPlayingAsync();
        }
        private async void LoadFolderPlaylist()
        {
            Playlist = await DatabaseManager.GetSongItemsFromFolderAsync(directory);
            Scroll();
        }
        private async void LoadGenrePlaylist()
        {
            Playlist = await DatabaseManager.GetSongItemsFromGenreAsync(genre);
            Scroll();
        }
        private async void LoadSmartPlaylist()
        {
            Playlist = await DatabaseManager.GetSongItemsFromSmartPlaylistAsync(id);
            Scroll();
        }
        private async void LoadPlainPlaylist()
        {
            Playlist = await DatabaseManager.GetSongItemsFromPlainPlaylistAsync(id);
            Scroll();
        }

        private int SelectedIndex(SongItem item)
        {
            int i = 0;
            foreach (var song in Playlist)
            {
                if (song.SongId == item.SongId) break;
                i++;
            }
            return i;
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            ascending = true;
            Playlist.Clear();
            genre = null;
            directory = null;
            folderName = null;
            isSmart = false;
            IsNowPlaying = true;
            IsDeletable = true;
            id = 0;
            name = "";
            index = 0;
            
            if (parameter != null)
            {
                if (parameter.GetType() == typeof(string))
                {
                    String[] s = ParamConvert.ToStringArray(parameter as string);
                    if (s.Length >= 2 && s[0].Equals("genre"))
                    {
                        genre = s[1];
                        IsDeletable = false;
                    }
                    else if (s.Length >= 3 && s[0].Equals("folder"))
                    {
                        folderName = s[1];
                        directory = s[2];
                        IsDeletable = false;
                    }
                    else
                    {
                        if (s.Length >= 2 && s[0].Equals("smart"))
                        {
                            isSmart = true;
                            IsDeletable = false;
                        }
                        if (s.Length >= 3 && (s[0].Equals("smart") || s[0].Equals("plain")))
                        {
                            id = Int32.Parse(s[1]);
                            name = s[2];
                        }
                    }
                    IsNowPlaying = false;
                }
            }
            if (isNowPlaying)
            {
                index = ApplicationSettingsHelper.ReadSongIndex();
            }
            if (state != null)
            {
                if (state.ContainsKey("index"))
                {
                    index = (int)state["index"];
                }
            }
        }

        public void Deactivate(Dictionary<string, object> state)
        {
            state["index"] = index;
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
        }
    }
}