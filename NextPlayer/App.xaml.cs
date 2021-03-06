﻿using GalaSoft.MvvmLight.Threading;
using NextPlayer.Common;
using NextPlayer.Converters;
using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Enums;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.ApplicationInsights;
using NextPlayerDataLayer.Diagnostics;
using Microsoft.HockeyApp;
using System.Diagnostics;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace NextPlayer
{
    public delegate void SongUpdatedHandler(int id);
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// Allows tracking page views, exceptions and other telemetry through the Microsoft Application Insights service.
        /// </summary>
        public static TelemetryClient TelemetryClient;

        private TransitionCollection transitions;
#if DEBUG
        private bool dev = true;
#else
        private bool dev = false;
#endif
        public static bool LastFmRateOn;
        public static int LastFmUnLove;
        public static int LastFmLove;
        public static bool LastFmSendNP;

        public static int res = 0;
        
        public static event SongUpdatedHandler SongUpdated;
        public static void OnSongUpdated(int id)
        {
            if (SongUpdated != null)
            {
                SongUpdated(id);
            }
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            HockeyClient.Current.Configure("");

            //App.Current.RequestedTheme = ApplicationTheme.Dark;
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.MediaScan);
            var settings = ApplicationData.Current.LocalSettings;
            
            if (FirstRun())
            {
                //jesli jest DB jest tworzone po wersji 1.5.1.0 przy tworzeniu bazy trzeba zapisac jej wersje
                Library.Current.SetDB();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppTheme, AppThemeEnum.Dark.ToString());
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IsPhoneAccentSet, true);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppAccent, "#FF008A00");
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IsBGImageSet, false);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.BackgroundImagePath, "");
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ShowCoverAsBackground, true);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileAppTransparent, "yes");
                //DiagnosticHelper.TrackTrace("New instalation",Microsoft.HockeyApp.SeverityLevel.Information);
            }
            else
            {
                ManageSecondaryTileImages();

                if (!settings.Values.ContainsKey(AppConstants.BackgroundImagePath))
                {
                    settings.Values.Add(AppConstants.BackgroundImagePath, "");
                }
                if (!settings.Values.ContainsKey(AppConstants.IsBGImageSet))
                {
                    settings.Values.Add(AppConstants.IsBGImageSet, false);
                }
                if (!settings.Values.ContainsKey(AppConstants.AppTheme))
                {
                    settings.Values.Add(AppConstants.AppTheme, AppThemeEnum.Dark.ToString());
                }
                if (!settings.Values.ContainsKey(AppConstants.IsPhoneAccentSet))
                {
                    settings.Values.Add(AppConstants.IsPhoneAccentSet, true);
                }
                if (!settings.Values.ContainsKey(AppConstants.AppAccent))
                {
                    settings.Values.Add(AppConstants.AppAccent, "#FF008A00");
                }
                if (!settings.Values.ContainsKey(AppConstants.ShowCoverAsBackground))
                {
                    settings.Values.Add(AppConstants.ShowCoverAsBackground, true);
                }

                string theme = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppTheme) as string;
                if (theme.Equals(AppThemeEnum.Dark.ToString()))
                {
                    App.Current.RequestedTheme = ApplicationTheme.Dark;
                    
                }
                else if (theme.Equals(AppThemeEnum.Light.ToString()))
                {
                    App.Current.RequestedTheme = ApplicationTheme.Light;
                    
                }

                SendLogs();
                
                //SaveLater.Current.SaveAllNow();
                UpdateDB();
                Logger.SaveFromSettingsToFile();
                CreateTask();
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TileAppTransparent) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileAppTransparent, "yes");
            }
#region LastFm

            //aplikacja jest uruchomiona 1 raz lub po aktualizacji
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmRateSongs) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmRateSongs, true);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmLove, 5);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmUnLove, 1);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmSendNP, false);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmLogin, "");
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmPassword, "");
            }

            LastFmLove = Int32.Parse(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmLove).ToString());
            LastFmUnLove = Int32.Parse(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmUnLove).ToString());
            LastFmRateOn = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmRateSongs);
            LastFmSendNP = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmSendNP);

            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LastFmDBVersion) == null)
            {
                DatabaseManager.CreateLastFmDB();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LastFmDBVersion, 1);
            }
            else if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LastFmDBVersion).ToString() == "1")
            {
                DatabaseManager.DeleteLastFmDB();
                DatabaseManager.CreateLastFmDB();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LastFmDBVersion, 2);
            }

#endregion LastFm
            
            UnhandledException += App_UnhandledException;
        }

        void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.SaveInSettings(e.Exception.ToString() + "\nMessage:\n" + e.Message);

            if (e != null)
            {
                Exception exception = e.Exception;
                if (exception is NullReferenceException && exception.ToString().ToUpper().Contains("SOMA"))
                {
                    Debug.WriteLine("Handled Smaato null reference exception {0}", exception);
                    e.Handled = true;
                    return;
                }
                else if (exception.ToString().Contains("AdDuplex"))
                {
                    e.Handled = true;
                    return;
                }
            }

            ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.MediaScan);
            int i = ApplicationSettingsHelper.ReadSongIndex();
            int npc = Library.Current.NowPlayingList.Count;
            if (i>=0 && npc>0 && npc > i)
            {
                ApplicationSettingsHelper.SaveSongIndex(i);
            }
            else
            {
                ApplicationSettingsHelper.SaveSongIndex(-1);
                if (npc >= 0)
                {
                    ApplicationSettingsHelper.SaveSongIndex(0);
                }
            }
            DiagnosticHelper.TrackTrace("Unhandled Exception " + e.Exception.ToString() + "\n" + e.Message, Microsoft.HockeyApp.SeverityLevel.Error);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            string theme = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppTheme) as string;
            if (theme.Equals(AppThemeEnum.Dark.ToString()))
            {
                statusBar.BackgroundColor = Windows.UI.Colors.Black;
                statusBar.ForegroundColor = Windows.UI.Colors.White;
            }
            else if (theme.Equals(AppThemeEnum.Light.ToString()))
            {
                statusBar.BackgroundColor = Windows.UI.Colors.White;
                statusBar.ForegroundColor = Windows.UI.Colors.Black;
            }  
            bool isPhoneAccent = (bool) ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IsPhoneAccentSet);
            try
            {
                if (isPhoneAccent)
                {
                    Windows.UI.Color color = ((SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"]).Color;
                    ((SolidColorBrush)App.Current.Resources["UserAccentBrush"]).Color = color;
                }
                else
                {
                    string hexColor = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppAccent) as string;
                    byte a = byte.Parse(hexColor.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                    byte r = byte.Parse(hexColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                    byte g = byte.Parse(hexColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                    byte b = byte.Parse(hexColor.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
                    Windows.UI.Color color = Windows.UI.Color.FromArgb(a, r, g, b);
                    ((SolidColorBrush)App.Current.Resources["UserAccentBrush"]).Color = color;
                }
            }
            catch(Exception ex)
            {
                DiagnosticHelper.TrackTrace("User accent set" + Environment.NewLine + ex.Message, SeverityLevel.Information);
            }
            if ((bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IsBGImageSet))
            {
                Helpers.StyleHelper.EnableBGImage();
            }
            Helpers.StyleHelper.ChangeMainPageButtonsBackground();
            Helpers.StyleHelper.ChangeAlbumViewTransparency();

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppState, AppConstants.ForegroundAppActive);
            Frame rootFrame = Window.Current.Content as Frame;
            
            bool fromTile = false;
            if (e.TileId.Contains(AppConstants.TileId))
            {
                string[] s = ParamConvert.ToStringArray(e.Arguments);

                if (s[0].Equals("album"))
                {
                    var a = DatabaseManager.GetSongItemsFromAlbum(s[1], s[2]);
                    Library.Current.SetNowPlayingList(a);
                }
                else if (s[0].Equals("playlist"))
                {
                    if (s[2].Equals(true.ToString()))
                    {
                        Library.Current.SetNowPlayingList(DatabaseManager.GetSongItemsFromSmartPlaylist(Int32.Parse(s[1])));
                    }
                    else
                    {
                        Library.Current.SetNowPlayingList(DatabaseManager.GetSongItemsFromPlainPlaylist(Int32.Parse(s[1])));
                    }
                }
                else if (s[0].Equals("artist"))
                {
                    Library.Current.SetNowPlayingList(DatabaseManager.GetSongItemsFromArtist(s[1]));
                }
                else if (s[0].Equals("genre"))
                {
                    Library.Current.SetNowPlayingList(DatabaseManager.GetSongItemsFromGenre(s[1]));
                }
                else if (s[0].Equals("folder"))
                {
                    Library.Current.SetNowPlayingList(DatabaseManager.GetSongItemsFromFolder(s[1])); 
                }
                ApplicationSettingsHelper.SaveSongIndex(0);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TilePlay, true);
                rootFrame = null;
                fromTile = true;
            }
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Associate the frame with a SuspensionManager key.
                if (fromTile)
                {
                    SuspensionManager.SessionState.Clear();
                }
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;
                
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate.
                    if (!fromTile)
                    {
                        try
                        {

                            //Logger.Save("resumed terminate app");
                            //Logger.SaveToFile();
                            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ResumePlayback, "");
                            await SuspensionManager.RestoreAsync();
                        }
                        catch (SuspensionManagerException ex)
                        {
                            // Something went wrong restoring state.
                            // Assume there is no state and continue.
                            Logger.Save("App OnLaunched() SuspensionManagerException" + "\n" + ex.Message);
                            Logger.SaveToFileBG();
                        }
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(View.MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            //rootFrame.Background = (ImageBrush)Resources["BgImage"];
            // Ensure the current window is active

            Window.Current.Activate();
            //SetDimensions();
            DispatcherHelper.Initialize();

            await HockeyClient.Current.SendCrashesAsync(true);
        }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //Logger.Save("on suspending" + Library.Current.Read());
            //Logger.SaveToFile();
            await SuspensionManager.SaveAsync();

            if (OnNewTilePinned != null)
            {
                // Perform the action.
                OnNewTilePinned();

                // Clear the action when finished. 
                OnNewTilePinned = null;
            }

            deferral.Complete();
        }

        public async Task<bool> CheckFileExists(string filename)
        {
            try
            {
                var store = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool FirstRun()
        {
            object o = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.FirstRun);
            if (o == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.FirstRun, false);
                return true;
            }
            else
            {
                return false;
            }
        }

        private async void ManageSecondaryTileImages()
        {
            var tiles = await SecondaryTile.FindAllAsync();
            StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var files = await localFolder.GetFilesAsync();
            bool exist;
            foreach(var file in files)
            {
                // image name = id + ".jpg"
                // secondary tile id = AppConstants.TileId + id.ToString()
                if (file.FileType.Equals(".jpg") && file.DisplayName.StartsWith(AppConstants.TileId))
                {
                    exist = false;
                    foreach (var tile in tiles)
                    {
                        if (tile.TileId == file.DisplayName) exist = true;
                    }
                    if (!exist)
                    {
                        await file.DeleteAsync(StorageDeleteOption.Default);
                    }
                }
            }

        }

        public static Action OnNewTilePinned { get; set; }

        private void UpdateDB()
        {
            //var settings = ApplicationData.Current.LocalSettings;

            //if (!settings.Values.ContainsKey(AppConstants.DBVersion))
            //{
            //    DatabaseManager.UpdateDBToVersion2();
            //    settings.Values.Add(AppConstants.DBVersion, "2");
            //}
            int v = (int)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.DBVersion);
            if (v == 2)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 3);
                ApplicationSettingsHelper.ReadResetSettingsValue("savelaterlyrics");
                DatabaseManager.UpdateDBToVersion3();
            }
        }

        private async Task SendLogs()
        {
            string bg = await Logger.ReadBG();
            string fg = await Logger.Read();
            string lastfm = await Logger.ReadLastFm();
            if (bg != "")
            {
                DiagnosticHelper.TrackTrace(bg, Microsoft.HockeyApp.SeverityLevel.Error);
            }
            if (fg != "")
            {
                DiagnosticHelper.TrackTrace(fg, Microsoft.HockeyApp.SeverityLevel.Error);
            }
            if (lastfm != "")
            {
                DiagnosticHelper.TrackTrace(lastfm, Microsoft.HockeyApp.SeverityLevel.Error);
            }
            Logger.ClearAll();
            Logger.ClearLastFm();
        }

        private async void CreateTask()
        {
            CheckAppVersion();
            var taskRegistered = false;
            var taskName = "ScrobbleSenderNetworkTask";

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    //task.Value.Unregister(true);
                    taskRegistered = true;
                    break;
                }
            }

            if (!taskRegistered)
            {
                await BackgroundExecutionManager.RequestAccessAsync();
                
                var builder = new BackgroundTaskBuilder();
                builder.Name = taskName;
                builder.TaskEntryPoint = "BackgroundNetworkTask.NetworkTask";
                builder.SetTrigger(new TimeTrigger(30, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                BackgroundTaskRegistration task = builder.Register();
            }
        }

        private async void CheckAppVersion()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var pkgVersion = Package.Current.Id.Version;

            var appVersion = pkgVersion.Build + "." +
                                pkgVersion.Major + "." +
                                pkgVersion.Minor + "." +
                                pkgVersion.Revision;

            if ((localSettings.Values["appVersion"]??"").ToString() != appVersion)
            {
                // Our app has been updated
                localSettings.Values["appVersion"] = appVersion;

                // Call removeAccess
                BackgroundExecutionManager.RemoveAccess();
            }

            await BackgroundExecutionManager.RequestAccessAsync();
        }

        private string GetAppVersion()
        {
            var pkgVersion = Package.Current.Id.Version;
            string appVersion = pkgVersion.Build + "." +
                                pkgVersion.Major + "." +
                                pkgVersion.Minor + "." +
                                pkgVersion.Revision;
            return appVersion;
        }

    }
}