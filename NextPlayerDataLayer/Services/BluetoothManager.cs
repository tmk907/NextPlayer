using Bluetooth.Core.Services;
using Bluetooth.Services.Obex;
using BluetoothDemo;
using NextPlayerDataLayer.Constants;
using NextPlayerDataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerDataLayer.Services
{
    public delegate void RemoveFirstFile();
    public delegate void Failed(FileItemToShare file);
    public delegate void SetDevicesList();
    public delegate void ChangeProgress(ulong progress);
    public delegate void ChangeStatus(FileShareStatus status);
    public delegate void TransferCompleted(string status);

    public sealed class BluetoothManager
    {
        public static event RemoveFirstFile RemoveFirstFileHandler;
        public static event Failed FailedHandler;
        public static event SetDevicesList SetDevicesListHandler;
        public static event ChangeProgress ChangeProgressHandler;
        public static event ChangeStatus ChangeStatusHandler;
        public static event TransferCompleted TransferCompletedHandler;

        public static void OnRemoveFirstFile()
        {
            if (RemoveFirstFileHandler != null)
            {
                RemoveFirstFileHandler();
            }
        }
        public static void OnFailed(FileItemToShare file)
        {
            if (FailedHandler != null)
            {
                FailedHandler(file);
            }
        }
        public static void OnSetDevicesList()
        {
            if (SetDevicesListHandler != null)
            {
                SetDevicesListHandler();
            }
        }
        public static void OnChangeStatus(FileShareStatus status)
        {
            if (ChangeStatusHandler != null)
            {
                ChangeStatusHandler(status);
            }
        }
        public static void OnChangeProgress(ulong progress)
        {
            if (ChangeProgressHandler != null)
            {
                ChangeProgressHandler(progress);
            }
        }
        public static void OnTransferCompleted(string status)
        {
            if (TransferCompletedHandler != null)
            {
                TransferCompletedHandler(status);
            }
        }

        private static BluetoothManager instance = null;
        public static BluetoothManager Current
        {
            get
            {
                if (instance == null)
                {
                    instance = new BluetoothManager();
                }
                return instance;
            }
        }
        private BluetoothManager()
        {
            filesToShare = new List<FileItemToShare>();
            pairedDevices = new List<BluetoothDevice>();
        }

        private List<FileItemToShare> filesToShare;
        public List<FileItemToShare> FilesToShare { get { return filesToShare; } }
        ObexService obexService;
        BluetoothDevice BtDevice;
        private List<BluetoothDevice> pairedDevices;
        public List<BluetoothDevice> PairedDevices { get { return pairedDevices; } }
        private string failureReason = "";
        public string FailureReason { get { return failureReason; } }

        public void AddFiles(IEnumerable<FileItemToShare> files)
        {
            foreach (var f in files)
            {
                filesToShare.Add(f);
            }
        }

        public void RemoveFile(string filePath)
        {
            try
            {
                int index = filesToShare.FindIndex(f => f.FilePath.Equals(filePath));
                if (index > 0)
                {
                    filesToShare.RemoveAt(index);
                }
            }
            catch (System.ArgumentNullException ex)
            {

            }
        }

        public void Clear()
        {
            if (filesToShare.Count>0 && (filesToShare[0].ShareStatus == FileShareStatus.Sharing || filesToShare[0].ShareStatus == FileShareStatus.Connecting))
            {
                FileItemToShare file = filesToShare[0];
                filesToShare.Clear();
                filesToShare.Add(file);
            }
            else
            {
                filesToShare.Clear();
            }
        }



        #region Find devices
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
            pairedDevices = e.PairedDevices;
            OnSetDevicesList();
            failureReason = "";
        }

        void btService_SearchForPairedDevicesFailed(object sender, SearchForPairedDevicesFailedEventArgs e)
        {
            (sender as BluetoothService).SearchForPairedDevicesFailed -= btService_SearchForPairedDevicesFailed;
            (sender as BluetoothService).SearchForPairedDevicesSucceeded -=  btService_SearchForPairedDevicesSucceeded;
            pairedDevices.Clear();
            OnSetDevicesList();
            failureReason = e.FailureReason.ToString();
        }

        #endregion

        public async void StartService()
        {
            if (BtDevice!=null)
            {
                StartService(BtDevice);
            }
        }

        public async void StartService(BluetoothDevice BTDevice)
        {
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

        private void obexService_Aborted(object sender, EventArgs e)
        {
            if (!filesToShare.Count.Equals(0))
            {
                filesToShare.RemoveAt(0);
                OnRemoveFirstFile();
            }
        }

        private void obexService_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            filesToShare[0].SetStatus(FileShareStatus.Error);
            filesToShare[0].SetProgress(0);
            FileItemToShare fileToShare = filesToShare[0];
            filesToShare.RemoveAt(0);
            filesToShare.Add(fileToShare);
            //OnRemoveFirstFile();
            OnFailed(fileToShare);
        }

        private void obexService_DataTransferFailed(object sender, DataTransferFailedEventArgs e)
        {
            filesToShare[0].SetStatus(FileShareStatus.Error);
            filesToShare[0].SetProgress(0);
            FileItemToShare fileToShare = filesToShare[0];
            filesToShare.RemoveAt(0);
            filesToShare.Add(fileToShare);
            //OnRemoveFirstFile();
            OnFailed(fileToShare);
        }

        private void obexService_DataTransferProgressed(object sender, DataTransferProgressedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("{0} bytes sent, {1}% transfer progressed", e.TransferInBytes, e.TransferInPercentage * 100);
            //FileItemToShare fileToShare = filesToShare[0];
            //fileToShare.Progress = e.TransferInBytes;
            OnChangeProgress(e.TransferInBytes);
            //filesToShare[0].Progress = e.TransferInBytes;
        }

        private void obexService_DataTransferSucceeded(object sender, EventArgs e)
        {
            OnTransferCompleted("fileSent" + filesToShare[0].FileName);
            filesToShare.RemoveAt(0);
            OnRemoveFirstFile();
        }

        private void obexService_Disconnecting(object sender, EventArgs e)
        {
            
        }

        async void obexService_Disconnected(object sender, EventArgs e)
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

            if (filesToShare.Count.Equals(0))
            {
                OnTransferCompleted(AppConstants.FilesSharedOK);
            }
            else if (this.filesToShare[0].ShareStatus.Equals(FileShareStatus.Error) || this.filesToShare[0].ShareStatus.Equals(FileShareStatus.Cancelled))
            {
                OnTransferCompleted(AppConstants.FilesSharedError);
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
        }

        async void obexService_DeviceConnected(object sender, EventArgs e)
        {
            if (this.filesToShare.Count > 0)
            {
                //filesToShare[0].ShareStatus = FileShareStatus.Connecting;
                OnChangeStatus(FileShareStatus.Connecting);
                await obexService.SendFileAsync(filesToShare[0].FileToShare);
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
        }

        private void obexService_ServiceConnected(object sender, EventArgs e)
        {
            //filesToShare[0].ShareStatus = FileShareStatus.Sharing;
            OnChangeStatus(FileShareStatus.Sharing);
        }
    }
}
