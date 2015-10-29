using Bluetooth.Core.Services;
using Bluetooth.Services.Obex;
using BluetoothDemo;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using NextPlayer.Converters;
using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Helpers;
using NextPlayerDataLayer.Model;
using NextPlayerDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;

namespace NextPlayer.ViewModel
{
    public class BluetoothShareViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private List<int> songIds;
        ObexService obexService;
        BluetoothDevice BtDevice;
        private String[] s;
        StatusBar systemTray;
        ResourceLoader loader;

        public BluetoothShareViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            songIds = new List<int>();
            systemTray = StatusBar.GetForCurrentView();
            loader = new ResourceLoader();
            App.Current.Suspending += ForegroundApp_Suspending;
            App.Current.Resuming += ForegroundApp_Resuming;
        }

        private void ForegroundApp_Resuming(object sender, object e)
        {
            foreach (var file in BluetoothManager.Current.FilesToShare)
            {
                FilesToShare.Add(file);
            }
            BluetoothManager.FailedHandler += BluetoothManager_FailedHandler;
            BluetoothManager.ChangeProgressHandler += BluetoothManager_ChangeProgressHandler;
            BluetoothManager.ChangeStatusHandler += BluetoothManager_ChangeStatusHandler;
            BluetoothManager.RemoveFirstFileHandler += BluetoothManager_RemoveFirstFileHandler;
            BluetoothManager.SetDevicesListHandler += BluetoothManager_SetDevicesListHandler;
            BluetoothManager.TransferCompletedHandler += BluetoothManager_TransferCompletedHandler;
        }

        private void ForegroundApp_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            BluetoothManager.FailedHandler -= BluetoothManager_FailedHandler;
            BluetoothManager.ChangeProgressHandler -= BluetoothManager_ChangeProgressHandler;
            BluetoothManager.ChangeStatusHandler -= BluetoothManager_ChangeStatusHandler;
            BluetoothManager.RemoveFirstFileHandler -= BluetoothManager_RemoveFirstFileHandler;
            BluetoothManager.SetDevicesListHandler -= BluetoothManager_SetDevicesListHandler;
            BluetoothManager.TransferCompletedHandler -= BluetoothManager_TransferCompletedHandler;
            FilesToShare.Clear();
        }

        #region Properties

        /// <summary>
        /// The <see cref="DeviceListVisibility" /> property's name.
        /// </summary>
        public const string DeviceListVisibilityPropertyName = "DeviceListVisibility";

        private bool devicelistVisibility = true;

        /// <summary>
        /// Sets and gets the DeviceListVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool DeviceListVisibility
        {
            get
            {
                if (IsInDesignMode)
                {
                    devicelistVisibility = false;
                }
                return devicelistVisibility;
            }

            set
            {
                if (devicelistVisibility == value)
                {
                    return;
                }

                devicelistVisibility = value;
                RaisePropertyChanged(DeviceListVisibilityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="FileListVisibility" /> property's name.
        /// </summary>
        public const string FileListVisibilityPropertyName = "FileListVisibility";

        private bool filelistVisibility = false;

        /// <summary>
        /// Sets and gets the FileListVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool FileListVisibility
        {
            get
            {
                if (IsInDesignMode) filelistVisibility = true;
                return filelistVisibility;
            }

            set
            {
                if (filelistVisibility == value)
                {
                    return;
                }

                filelistVisibility = value;
                RaisePropertyChanged(FileListVisibilityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="DeviceList" /> property's name.
        /// </summary>
        public const string DeviceListPropertyName = "DeviceList";

        private List<BluetoothDevice> deviceList = new List<BluetoothDevice>();

        /// <summary>
        /// Sets and gets the DeviceList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<BluetoothDevice> DeviceList
        {
            get
            {
                return deviceList;
            }

            set
            {
                if (deviceList == value)
                {
                    return;
                }

                deviceList = value;
                RaisePropertyChanged(DeviceListPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="FilesToShare" /> property's name.
        /// </summary>
        public const string FilesToSharePropertyName = "FilesToShare";

        private ObservableCollection<FileItemToShare> filesToShare = new ObservableCollection<FileItemToShare>();

        /// <summary>
        /// Sets and gets the FilesToShare property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<FileItemToShare> FilesToShare
        {
            get
            {
                if (IsInDesignMode)
                {
                    filesToShare.Add(new FileItemToShare("File name.mp3", "path"));
                    filesToShare.Add(new FileItemToShare("File name name name.m4a", "path"));
                }
                return filesToShare;
            }

            set
            {
                if (filesToShare == value)
                {
                    return;
                }

                filesToShare = value;
                RaisePropertyChanged(FilesToSharePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ScanResult" /> property's name.
        /// </summary>
        public const string ScanResultPropertyName = "ScanResult";

        private string scanResult = "";

        /// <summary>
        /// Sets and gets the ScanResult property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ScanResult
        {
            get
            {
                return scanResult;
            }

            set
            {
                if (scanResult == value)
                {
                    return;
                }

                scanResult = value;
                RaisePropertyChanged(ScanResultPropertyName);
            }
        }

        #endregion

        private RelayCommand<BluetoothDevice> itemClicked;

        /// <summary>
        /// Gets the ItemClicked.
        /// </summary>
        public RelayCommand<BluetoothDevice> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<BluetoothDevice>(
                    p =>
                    {
                        DeviceListVisibility = false;
                        FileListVisibility = true;
                        //StartService(p);
                        BluetoothManager.Current.StartService(p);
                    }));
            }
        }

        private RelayCommand scan;

        /// <summary>
        /// Gets the Scan.
        /// </summary>
        public RelayCommand Scan
        {
            get
            {
                return scan
                    ?? (scan = new RelayCommand(
                    () =>
                    {
                        //EnumerateDevicesAsync();
                        StartScan();
                    }));
            }
        }

        private RelayCommand clear;

        /// <summary>
        /// Gets the Clear.
        /// </summary>
        public RelayCommand Clear
        {
            get
            {
                return clear
                    ?? (clear = new RelayCommand(
                    () =>
                    {
                        BluetoothManager.Current.Clear();
                        FilesToShare.Clear();
                        foreach (var file in BluetoothManager.Current.FilesToShare) FilesToShare.Add(file); //add 0 or 1 file
                    }));
            }
        }

      
        public async Task StartScan()
        {
            ScanResult = loader.GetString("SearchForDevices");
            systemTray.ProgressIndicator.Text = loader.GetString("SearchForDevices");
            await systemTray.ProgressIndicator.ShowAsync();
            await BluetoothManager.Current.EnumerateDevicesAsync();
        }

        public async Task SearchForPairedDevicesSucceeded()
        {
            ScanResult = loader.GetString("Devices");
            await systemTray.ProgressIndicator.HideAsync();
            DeviceList = BluetoothManager.Current.PairedDevices;
        }

        public async Task SearchForPairedDevicesFailed()
        {
            ScanResult = loader.GetString("DevicesNotFound");
            await systemTray.ProgressIndicator.HideAsync();
            DeviceList = BluetoothManager.Current.PairedDevices;
        }
        #region BT

        private async void StartService(BluetoothDevice BTDevice)
        {
            await GetFiles();
            BtDevice = BTDevice;
            obexService = ObexService.GetDefaultForBluetoothDevice(BtDevice);
            obexService.Aborted += obexService_Aborted;
            obexService.ConnectionFailed += obexService_ConnectionFailed;
            obexService.DataTransferFailed += obexService_DataTransferFailed;
            obexService.DataTransferProgressed += obexService_DataTransferProgressed;
            obexService.DataTransferSucceeded += obexService_DataTransferSucceeded;
            obexService.DeviceConnected += obexService_DeviceConnected;
            obexService.Disconnected += obexService_Disconnected;
            obexService.Disconnecting += obexService_Disconnecting;
            obexService.ServiceConnected += obexService_ServiceConnected;
            await obexService.ConnectAsync();
        }
       
        async void obexService_DataTransferFailed(object sender, DataTransferFailedEventArgs e)
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                this.filesToShare[0].ShareStatus = FileShareStatus.Error;
                this.filesToShare[0].Progress = 0;
                FileItemToShare fileToShare = this.filesToShare[0];
                this.filesToShare.RemoveAt(0);
                this.filesToShare.Add(fileToShare);
            });
        }

        async void obexService_Aborted(object sender, EventArgs e)
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                if (!filesToShare.Count.Equals(0))
                {
                    filesToShare.RemoveAt(0);
                }
            });
        }
        async void obexService_Disconnected(object sender, EventArgs e)
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                obexService.Aborted -= obexService_Aborted;
                obexService.ConnectionFailed -= obexService_ConnectionFailed;
                obexService.DataTransferFailed -= obexService_DataTransferFailed;
                obexService.DataTransferProgressed -= obexService_DataTransferProgressed;
                obexService.DataTransferSucceeded -= obexService_DataTransferSucceeded;
                obexService.DeviceConnected -= obexService_DeviceConnected;
                obexService.Disconnected -= obexService_Disconnected;
                obexService.Disconnecting -= obexService_Disconnecting;
                obexService.ServiceConnected -= obexService_ServiceConnected;
                obexService = null;

                if (this.filesToShare.Count.Equals(0))
                {
                    SendToast(AppConstants.FilesSharedOK);
                    navigationService.GoBack();
                }
                else if (this.filesToShare[0].ShareStatus.Equals(FileShareStatus.Error) || this.filesToShare[0].ShareStatus.Equals(FileShareStatus.Cancelled))
                {
                    SendToast(AppConstants.FilesSharedError);
                    navigationService.GoBack();
                }
                else
                {
                    obexService = ObexService.GetDefaultForBluetoothDevice(BtDevice);
                    obexService.DeviceConnected += obexService_DeviceConnected;
                    obexService.ServiceConnected += obexService_ServiceConnected;
                    obexService.ConnectionFailed += obexService_ConnectionFailed;
                    obexService.DataTransferProgressed += obexService_DataTransferProgressed;
                    obexService.DataTransferSucceeded += obexService_DataTransferSucceeded;
                    obexService.DataTransferFailed += obexService_DataTransferFailed;
                    obexService.Disconnecting += obexService_Disconnecting;
                    obexService.Disconnected += obexService_Disconnected;
                    obexService.Aborted += obexService_Aborted;
                    await obexService.ConnectAsync();
                }
            });
        }
        async void obexService_Disconnecting(object sender, EventArgs e)
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                //System.Diagnostics.Debug.WriteLine("disconnecting from remote device");
            });
        }

        async void obexService_DataTransferSucceeded(object sender, EventArgs e)
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine("Data transfer of current file succeeded");
                this.filesToShare.RemoveAt(0);
            });
        }

        async void obexService_DataTransferProgressed(object sender, DataTransferProgressedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("{0} bytes sent, {1}% transfer progressed", e.TransferInBytes, e.TransferInPercentage * 100);
            await DispatcherHelper.RunAsync(async () =>
            {
                FileItemToShare fileToShare = this.filesToShare[0];
                fileToShare.Progress = e.TransferInBytes;

            });
        }

        async void obexService_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine("Connection to obex service on target device failed");
                FileItemToShare fileToShare = this.filesToShare[0];
                fileToShare.ShareStatus = FileShareStatus.Error;
                fileToShare.Progress = 0;
                this.filesToShare.RemoveAt(0);
                this.filesToShare.Add(fileToShare);
            });
        }

        async void obexService_ServiceConnected(object sender, EventArgs e)
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine("Device connected to remote obex service on target device");
                this.filesToShare[0].ShareStatus = FileShareStatus.Sharing;
            });
        }

        async void obexService_DeviceConnected(object sender, EventArgs e)
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine("Device connected to remote socket");
                if (this.filesToShare.Count > 0)
                {
                    this.filesToShare[0].ShareStatus = FileShareStatus.Connecting;
                    await obexService.SendFileAsync(this.filesToShare[0].FileToShare);
                }
                else
                {
                    obexService.Aborted -= obexService_Aborted;
                    obexService.ConnectionFailed -= obexService_ConnectionFailed;
                    obexService.DataTransferFailed -= obexService_DataTransferFailed;
                    obexService.DataTransferProgressed -= obexService_DataTransferProgressed;
                    obexService.DataTransferSucceeded -= obexService_DataTransferSucceeded;
                    obexService.DeviceConnected -= obexService_DeviceConnected;
                    obexService.Disconnected -= obexService_Disconnected;
                    obexService.Disconnecting -= obexService_Disconnecting;
                    obexService.ServiceConnected -= obexService_ServiceConnected;
                    obexService = null;
                }
            });
        }

        #endregion

        private static void SendToast(string message)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(loader.GetString(message)));
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private async Task<IEnumerable<FileItemToShare>> GetFiles()
        {
            List<FileItemToShare> list = new List<FileItemToShare>();
            if (s[0].Equals("song"))
            {
                string path = DatabaseManager.GetFileInfo(Int32.Parse(s[1])).FilePath;
                try
                {
                    IStorageFile file = await StorageFile.GetFileFromPathAsync(path);
                    list.Add(new FileItemToShare(Path.GetFileName(path), path, file));
                }
                catch (Exception e)
                {
                    NextPlayerDataLayer.Diagnostics.Logger.Save("BT get files" + "\n" + e.Message);
                    NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
                }
            }
            else if (s[0].Equals("album"))
            {
                foreach(var song in DatabaseManager.GetSongItemsFromAlbum(s[1], s[2]))
                {
                    try
                    {
                        IStorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
                        list.Add(new FileItemToShare(Path.GetFileName(song.Path), song.Path, file));
                    }
                    catch (Exception e)
                    {
                        NextPlayerDataLayer.Diagnostics.Logger.Save("BT get files" + "\n" + e.Message);
                        NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
                    }
                }
            }
            else if (s[0].Equals("artist"))
            {
                foreach(var song in DatabaseManager.GetSongItemsFromArtist(s[1]))
                {
                    try
                    {
                        IStorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
                        list.Add(new FileItemToShare(Path.GetFileName(song.Path), song.Path, file));
                    }
                    catch (Exception e)
                    {
                        NextPlayerDataLayer.Diagnostics.Logger.Save("BT get files" + "\n" + e.Message);
                        NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
                    }
                }
            }
            else if (s[0].Equals("genre"))
            {
                foreach (var song in DatabaseManager.GetSongItemsFromGenre(s[1]))
                {
                    try
                    {
                        IStorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
                        list.Add(new FileItemToShare(Path.GetFileName(song.Path), song.Path, file));
                    }
                    catch (Exception e)
                    {
                        NextPlayerDataLayer.Diagnostics.Logger.Save("BT get files" + "\n" + e.Message);
                        NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
                    }
                }
            }
            else if (s[0].Equals("folder"))
            {
                foreach (var song in DatabaseManager.GetSongItemsFromFolder(s[1]))
                {
                    try
                    {
                        IStorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
                        list.Add(new FileItemToShare(Path.GetFileName(song.Path), song.Path, file));
                    }
                    catch (Exception e)
                    {
                        NextPlayerDataLayer.Diagnostics.Logger.Save("BT get files" + "\n" + e.Message);
                        NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
                    }
                }
            }
            else if (s[0].Equals("playlist"))
            {
                var songList = new ObservableCollection<SongItem>();
                if (s[1].Equals("smart"))
                {
                    songList =  DatabaseManager.GetSongItemsFromSmartPlaylist(Int32.Parse(s[2]));
                }
                else
                {
                    songList = DatabaseManager.GetSongItemsFromPlainPlaylist(Int32.Parse(s[2]));
                }
                foreach (var song in songList)
                {
                    try
                    {
                        IStorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
                        list.Add(new FileItemToShare(Path.GetFileName(song.Path), song.Path, file));
                    }
                    catch (Exception e)
                    {
                        NextPlayerDataLayer.Diagnostics.Logger.Save("BT get files" + "\n" + e.Message);
                        NextPlayerDataLayer.Diagnostics.Logger.SaveToFile();
                    }
                }
            }
            return list;
        }

        public void Activate(object parameter, Dictionary<string, object> state)
        {
            DeviceListVisibility = true;
            FileListVisibility = false;
            obexService = null;
            BtDevice = null;
            if (parameter != null)
            {
                if (parameter.GetType() == typeof(string))
                {
                    s = ParamConvert.ToStringArray(parameter as string);
                }
            }
            Start();
            BluetoothManager.FailedHandler += BluetoothManager_FailedHandler;
            BluetoothManager.ChangeProgressHandler += BluetoothManager_ChangeProgressHandler;
            BluetoothManager.ChangeStatusHandler += BluetoothManager_ChangeStatusHandler;
            BluetoothManager.RemoveFirstFileHandler += BluetoothManager_RemoveFirstFileHandler;
            BluetoothManager.SetDevicesListHandler += BluetoothManager_SetDevicesListHandler;
            BluetoothManager.TransferCompletedHandler += BluetoothManager_TransferCompletedHandler;
        }

        async void BluetoothManager_TransferCompletedHandler(string status)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                SendToast(status);
                navigationService.GoBack();
            });
        }

        async void BluetoothManager_SetDevicesListHandler()
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (BluetoothManager.Current.PairedDevices.Count == 0)
                {
                    SearchForPairedDevicesFailed();
                }
                else
                {
                    SearchForPairedDevicesSucceeded();
                }
            });
        }

        async void BluetoothManager_RemoveFirstFileHandler()
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (!filesToShare.Count.Equals(0))
                {
                    FilesToShare.RemoveAt(0);
                }
            });
        }

        async void BluetoothManager_ChangeStatusHandler(FileShareStatus status)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (!filesToShare.Count.Equals(0))
                {
                    FilesToShare[0].ShareStatus = status;
                }
            });
        }

        async void BluetoothManager_ChangeProgressHandler(ulong progress)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (!filesToShare.Count.Equals(0))
                {
                    FilesToShare[0].Progress = progress;
                }
            });
        }

        async void BluetoothManager_FailedHandler(FileItemToShare f)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                FileItemToShare file = filesToShare[0];
                file.ShareStatus = FileShareStatus.Error;
                file.Progress = 0;
                FilesToShare.RemoveAt(0);
                FilesToShare.Add(file);
            });
        }

        private async void Start()
        {
            var list = await GetFiles();
            int count = BluetoothManager.Current.FilesToShare.Count;
            foreach (var file in BluetoothManager.Current.FilesToShare)
            {
                FilesToShare.Add(file);
            }
            BluetoothManager.Current.AddFiles(list);
            foreach (var f in list)
            {
                FilesToShare.Add(f);
            }
            if (count == 0)
            {
                await StartScan();
            }
            else
            {
                DeviceListVisibility = false;
                FileListVisibility = true;
                if (filesToShare[0].ShareStatus == FileShareStatus.Error)
                {
                    BluetoothManager.Current.StartService();
                }
            }
        }

        public void Deactivate(Dictionary<string, object> state)
        {
            
            BluetoothManager.FailedHandler -= BluetoothManager_FailedHandler;
            BluetoothManager.ChangeProgressHandler -= BluetoothManager_ChangeProgressHandler;
            BluetoothManager.ChangeStatusHandler -= BluetoothManager_ChangeStatusHandler;
            BluetoothManager.RemoveFirstFileHandler -= BluetoothManager_RemoveFirstFileHandler;
            BluetoothManager.SetDevicesListHandler -= BluetoothManager_SetDevicesListHandler;
            BluetoothManager.TransferCompletedHandler -= BluetoothManager_TransferCompletedHandler;
            filesToShare.Clear();
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            
        }
    }
}
