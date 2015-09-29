using NextPlayer.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Windows.Input;
using Bluetooth.Core.Services;
using System.Threading.Tasks;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace NextPlayer.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class testBT : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        ObservableCollection<PairedDeviceInfo> _pairedDevices;  // a local copy of paired device information
        StreamSocket _socket; // socket object used to communicate with the device

        public testBT()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Bluetooth is not available in the emulator. 
            //if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator)
            //{
            //    MessageBox.Show(AppResources.Msg_EmulatorMode,"Warning",MessageBoxButton.OK);
            //}

            _pairedDevices = new ObservableCollection<PairedDeviceInfo>();
            PairedDevicesList.ItemsSource = _pairedDevices;
        }

        private void FindPairedDevices_Tap(object sender, RoutedEventArgs e)
        {
            RefreshPairedDevicesList();
            
        }
        public async Task EnumerateDevicesAsync()
        {
            BluetoothService btService = BluetoothService.GetDefault();
            btService.SearchForPairedDevicesFailed +=
              btService_SearchForPairedDevicesFailed;
            btService.SearchForPairedDevicesSucceeded +=
              btService_SearchForPairedDevicesSucceeded;
            await btService.SearchForPairedDevicesAsync();
        }
        void btService_SearchForPairedDevicesSucceeded(object sender,
          SearchForPairedDevicesSucceededEventArgs e)
        {
            (sender as BluetoothService).SearchForPairedDevicesFailed -=
              btService_SearchForPairedDevicesFailed;
            (sender as BluetoothService).SearchForPairedDevicesSucceeded -=
              btService_SearchForPairedDevicesSucceeded;
            
        }
        void btService_SearchForPairedDevicesFailed(object sender,
          SearchForPairedDevicesFailedEventArgs e)
        {
            (sender as BluetoothService).SearchForPairedDevicesFailed -=
              btService_SearchForPairedDevicesFailed;
            (sender as BluetoothService).SearchForPairedDevicesSucceeded -=
              btService_SearchForPairedDevicesSucceeded;
            //txtblkErrorBtDevices.Text = e.FailureReason.ToString();
        }
        /// <summary>
        /// Asynchronous call to re-populate the ListBox of paired devices.
        /// </summary>
        private async void RefreshPairedDevicesList()
        {
            await EnumerateDevicesAsync();
            try
            {
                // Search for all paired devices
                PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
                var peers = await PeerFinder.FindAllPeersAsync();

                // By clearing the backing data, we are effectively clearing the ListBox
                _pairedDevices.Clear();

                if (peers.Count == 0)
                {
                    //MessageBox.Show(AppResources.Msg_NoPairedDevices);
                }
                else
                {
                    // Found paired devices.
                    foreach (var peer in peers)
                    {
                        _pairedDevices.Add(new PairedDeviceInfo(peer));
                    }
                }
            }
            catch (Exception ex)
            {
                //if ((uint)ex.HResult == 0x8007048F)
                //{
                //    var result = MessageBox.Show(AppResources.Msg_BluetoothOff, "Bluetooth Off", MessageBoxButton.OKCancel);
                //    if (result == MessageBoxResult.OK)
                //    {
                      ShowBluetoothcControlPanel();
                //    }
                //}
                //else if ((uint)ex.HResult == 0x80070005)
                //{
                //    MessageBox.Show(AppResources.Msg_MissingCaps);
                //}
                //else
                //{
                //    MessageBox.Show(ex.Message);
                //}
            }
        }

        private void ConnectToSelected_Tap_1(object sender, RoutedEventArgs e)
        {
            // Because I enable the ConnectToSelected button only if the user has
            // a device selected, I don't need to check here whether that is the case.

            // Connect to the device
            PairedDeviceInfo pdi = PairedDevicesList.SelectedItem as PairedDeviceInfo;
            PeerInformation peer = pdi.PeerInfo;

            // Asynchronous call to connect to the device
            ConnectToDevice(peer);
        }

        private async void ConnectToDevice(PeerInformation peer)
        {
            if (_socket != null)
            {
                // Disposing the socket with close it and release all resources associated with the socket
                _socket.Dispose();
            }

            try
            {
                _socket = new StreamSocket();
                string serviceName = (String.IsNullOrWhiteSpace(peer.ServiceName)) ? tbServiceName.Text : peer.ServiceName;

                // Note: If either parameter is null or empty, the call will throw an exception
                await _socket.ConnectAsync(peer.HostName, serviceName);

                // If the connection was successful, the RemoteAddress field will be populated
               string a =  _socket.Information.RemoteAddress.DisplayName;
            }
            catch (Exception ex)
            {
                // In a real app, you would want to take action dependent on the type of 
                // exception that occurred.
                //MessageBox.Show(ex.Message);

                _socket.Dispose();
                _socket = null;
            }
        }

        private void PairedDevicesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check whether the user has selected a device
            if (PairedDevicesList.SelectedItem == null)
            {
                // No - hide these fields
                ConnectToSelected.IsEnabled = false;
                ServiceNameInput.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Yes - enable the connect button
                ConnectToSelected.IsEnabled = true;

                // Show the service name field, if the ServiceName associated with this device is currently empty
                PairedDeviceInfo pdi = PairedDevicesList.SelectedItem as PairedDeviceInfo;
                ServiceNameInput.Visibility = (String.IsNullOrWhiteSpace(pdi.ServiceName)) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ShowBluetoothcControlPanel()
        {
            //ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
            //connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.Bluetooth;
            //connectionSettingsTask.Show();
        }
    }

    /// <summary>
    ///  Class to hold all paired device information
    /// </summary>
    public class PairedDeviceInfo
    {
        internal PairedDeviceInfo(PeerInformation peerInformation)
        {
            this.PeerInfo = peerInformation;
            this.DisplayName = this.PeerInfo.DisplayName;
            this.HostName = this.PeerInfo.HostName.DisplayName;
            this.ServiceName = this.PeerInfo.ServiceName;
        }

        public string DisplayName { get; private set; }
        public string HostName { get; private set; }
        public string ServiceName { get; private set; }
        public PeerInformation PeerInfo { get; private set; }
    }
    
}
