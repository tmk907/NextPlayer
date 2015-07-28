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
using NextPlayer.Converters;
using Windows.UI.Xaml.Controls;

namespace NextPlayer.ViewModel
{
    public class GenresViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private int index;

        public GenresViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        /// <summary>
        /// The <see cref="Playlists" /> property's name.
        /// </summary>
        public const string GenresPropertyName = "Genres";

        private ObservableCollection<GenreItem> genres = new ObservableCollection<GenreItem>();

        /// <summary>
        /// Sets and gets the Playlists property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<GenreItem> Genres
        {
            get
            {
                if (genres.Count == 0)
                {
                    if (IsInDesignMode)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            genres.Add(new GenreItem(TimeSpan.Zero, i.ToString(), i));
                        }
                    }
                    else
                    {
                        LoadGenres();
                    }
                }
                return genres;
            }

            set
            {
                if (genres == value)
                {
                    return;
                }

                genres = value;
                RaisePropertyChanged(GenresPropertyName);
            }
        }

        private RelayCommand<GenreItem> itemClicked;

        /// <summary>
        /// Gets the ItemClicked.
        /// </summary>
        public RelayCommand<GenreItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<GenreItem>(
                    item =>
                    {
                        index = Genres.IndexOf(item);
                        String[] s = new String[2];
                        s[0] = "genre";
                        s[1] = item.Genre;
                        navigationService.NavigateTo(ViewNames.PlaylistView, ParamConvert.ToString(s));
                    }));
            }
        }

        private RelayCommand<object> scrollListView;

        /// <summary>
        /// Gets the ScrollListView.
        /// </summary>
        public RelayCommand<object> ScrollListView
        {
            get
            {
                return scrollListView
                    ?? (scrollListView = new RelayCommand<object>(
                    p =>
                    {
                        ListView l = (ListView)p;

                        if (l.Items.Count > 0 && index > 0)
                        {
                            SemanticZoomLocation loc = new SemanticZoomLocation();
                            l.SelectedIndex = index;
                            loc.Item = l.SelectedItem;
                            l.UpdateLayout();
                            l.MakeVisible(loc);
                            l.ScrollIntoView(l.SelectedItem, ScrollIntoViewAlignment.Leading);
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
                        Genres = DatabaseManager.GetGenreItems();
                    }));
            }
        }

        private async void LoadGenres()
        {
            Genres = await DatabaseManager.GetGenreItemsAsync();
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            index = 0;
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