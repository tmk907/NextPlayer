using GalaSoft.MvvmLight.Threading;
using NextPlayer.Common;
using NextPlayer.Converters;
using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace NextPlayer
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private TransitionCollection transitions;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.MediaScan);

            if (FirstRun())
            {
                Library.Current.SetDB();
            }
            ManageSecondaryTileImages();
            //Read();
            //NextPlayerDataLayer.Diagnostics.Logger.Clear();
            UnhandledException += App_UnhandledException;
        }

        private async void Read()
        {
            string s = await NextPlayerDataLayer.Diagnostics.Logger.ReadAll();
            //NextPlayerDataLayer.Diagnostics.Logger.Clear();
        }

        void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //NextPlayerDataLayer.Diagnostics.Logger.Save(Library.Current.Read());
            NextPlayerDataLayer.Diagnostics.Logger.Save(e.Exception.ToString() + "\nMessage:\n" + e.Message);
            NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();

            ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.MediaScan);
            ApplicationSettingsHelper.SaveSongIndex(-1);
            if (Library.Current.NowPlayingList.Count >= 0)
            {
                ApplicationSettingsHelper.SaveSongIndex(0);
            }
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

                            //NextPlayerDataLayer.Diagnostics.Logger.Save("resumed terminate app");
                            //NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
                            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ResumePlayback, "");
                            await SuspensionManager.RestoreAsync();
                        }
                        catch (SuspensionManagerException)
                        {
                            // Something went wrong restoring state.
                            // Assume there is no state and continue.
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

            // Ensure the current window is active
            Window.Current.Activate();
            DispatcherHelper.Initialize();
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
            //NextPlayerDataLayer.Diagnostics.Logger.Save("on suspending" + Library.Current.Read());
            //NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
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
                var store = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(filename);
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
    }
}