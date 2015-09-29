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
using NextPlayerDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace NextPlayer.ViewModel
{
    public class BluetoothShareViewModel : ViewModelBase, INavigable
    {
        private INavigationService navigationService;
        private List<int> songIds;
        ObexService obexService;
        BluetoothDevice BtDevice;
        private String[] s;

        public BluetoothShareViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            songIds = new List<int>();
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
                        StartService(p);
                    }));
            }
        }

        public async Task EnumerateDevicesAsync()
        {
            BluetoothService btService = BluetoothService.GetDefault();
            btService.SearchForPairedDevicesFailed += btService_SearchForPairedDevicesFailed;
            btService.SearchForPairedDevicesSucceeded += btService_SearchForPairedDevicesSucceeded;
            await btService.SearchForPairedDevicesAsync();
        }
        void btService_SearchForPairedDevicesSucceeded(object sender, SearchForPairedDevicesSucceededEventArgs e)
        {
            (sender as BluetoothService).SearchForPairedDevicesFailed -= btService_SearchForPairedDevicesFailed;
            (sender as BluetoothService).SearchForPairedDevicesSucceeded -= btService_SearchForPairedDevicesSucceeded;
            DeviceList = e.PairedDevices;
        }
        void btService_SearchForPairedDevicesFailed(object sender, SearchForPairedDevicesFailedEventArgs e)
        {
            (sender as BluetoothService).SearchForPairedDevicesFailed -= btService_SearchForPairedDevicesFailed;
            (sender as BluetoothService).SearchForPairedDevicesSucceeded -=  btService_SearchForPairedDevicesSucceeded;
            //txtblkErrorBtDevices.Text = e.FailureReason.ToString();
        }
        private async void StartService(BluetoothDevice BTDevice)
        {
            bool a = await GetFiles();
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
                    //MessageDialog messageBox = new MessageDialog("All files shared successfully", "Bluetooth OBEX DemoApp");
                    //messageBox.Commands.Add(new UICommand("OK", (uiCommand) =>
                    //{
                    //    this.Frame.Navigate(typeof(MainPage));
                    //}));
                    //await messageBox.ShowAsync();
                    SendToast(AppConstants.FilesSharedOK);
                    navigationService.GoBack();
                }
                else if (this.filesToShare[0].ShareStatus.Equals(FileShareStatus.Error) || this.filesToShare[0].ShareStatus.Equals(FileShareStatus.Cancelled))
                {
                    //MessageDialog messageBox = new MessageDialog("Some files could not be shared successfully", "Bluetooth OBEX DemoApp");
                    //messageBox.Commands.Add(new UICommand("OK", (uiCommand) =>
                    //{
                    //    this.Frame.Navigate(typeof(MainPage));
                    //}));
                    //await messageBox.ShowAsync();
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

        private async Task<bool> GetFiles()
        {
            if (s[0].Equals("song"))
            {
                string path = DatabaseManager.GetFileInfo(Int32.Parse(s[1])).FilePath;
                try
                {
                    IStorageFile file = await StorageFile.GetFileFromPathAsync(path);
                    filesToShare.Add(new FileItemToShare(Path.GetFileName(path), path, file));
                }
                catch (Exception e) { }
            }
            else if (s[0].Equals("album"))
            {
                foreach(var song in DatabaseManager.GetSongItemsFromAlbum(s[1], s[2]))
                {
                    try
                    {
                        IStorageFile file = await StorageFile.GetFileFromPathAsync(song.Path);
                        FilesToShare.Add(new FileItemToShare(Path.GetFileName(song.Path), song.Path, file));
                    }
                    catch (Exception e)
                    {
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
                        FilesToShare.Add(new FileItemToShare(Path.GetFileName(song.Path), song.Path, file));
                    }
                    catch (Exception e)
                    {
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
                        FilesToShare.Add(new FileItemToShare(Path.GetFileName(song.Path), song.Path, file));
                    }
                    catch (Exception e)
                    {
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
                        FilesToShare.Add(new FileItemToShare(Path.GetFileName(song.Path), song.Path, file));
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            else if (s[0].Equals("playlist"))
            {
                
            }
            return true;
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
            EnumerateDevicesAsync();
        }

        public void Deactivate(Dictionary<string, object> state)
        {
            filesToShare.Clear();
            if (obexService != null)
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
        }

        public void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            
        }
    }
}
