using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.CDPlayer;
using AudioStation.Core.Component.CDPlayer.Interface;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

using static AudioStation.Core.Component.CDPlayer.DeviceWinAPI;

namespace AudioStation
{
    /// <summary>
    /// IDisposable pattern implemented by Application object OnExit(..)
    /// </summary>
    [IocExportDefault]
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Handle CD Events Here
        private readonly ICDDrive _cdDrive;

        #region Windows API Constants
        const int WS_EX_TOOLWINDOW = 0x80;
        const int WS_POPUP = unchecked((int)0x80000000);

        const int WM_DEVICECHANGE = 0x0219;

        const int DBT_APPYBEGIN = 0x0000;
        const int DBT_APPYEND = 0x0001;
        const int DBT_DEVNODES_CHANGED = 0x0007;
        const int DBT_QUERYCHANGECONFIG = 0x0017;
        const int DBT_CONFIGCHANGED = 0x0018;
        const int DBT_CONFIGCHANGECANCELED = 0x0019;
        const int DBT_MONITORCHANGE = 0x001B;
        const int DBT_SHELLLOGGEDON = 0x0020;
        const int DBT_CONFIGMGAPI32 = 0x0022;
        const int DBT_VXDINITCOMPLETE = 0x0023;
        const int DBT_VOLLOCKQUERYLOCK = 0x8041;
        const int DBT_VOLLOCKLOCKTAKEN = 0x8042;
        const int DBT_VOLLOCKLOCKFAILED = 0x8043;
        const int DBT_VOLLOCKQUERYUNLOCK = 0x8044;
        const int DBT_VOLLOCKLOCKRELEASED = 0x8045;
        const int DBT_VOLLOCKUNLOCKFAILED = 0x8046;
        const int DBT_DEVICEARRIVAL = 0x8000;
        const int DBT_DEVICEQUERYREMOVE = 0x8001;
        const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;
        const int DBT_DEVICEREMOVEPENDING = 0x8003;
        const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        const int DBT_DEVICETYPESPECIFIC = 0x8005;
        #endregion

        private readonly IViewModelController _viewModelController;
        private readonly IDialogController _dialogController;
        private readonly IIocEventAggregator _eventAggregator;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Needed by the framework
        public MainWindow()
        {
            InitializeComponent();

            _cdDrive = new CDDrive();

            _cdDrive.TracksLoadedEvent += OnCDTracksLoaded;
        }

        [IocImportingConstructor]
        public MainWindow(IIocEventAggregator eventAggregator, 
                          IDialogController dialogController, 
                          IViewModelController viewModelController,
                          ICDDrive cdDrive)
        {
            _viewModelController = viewModelController;
            _eventAggregator = eventAggregator;
            _dialogController = dialogController;

            InitializeComponent();

            this.DataContext = _viewModelController.GetMainViewModel();

            // CD Drive
            _cdDrive = cdDrive;
            _cdDrive.TracksLoadedEvent += OnCDTracksLoaded;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);            
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // This could use some more documentation; but I'm going to assume very little about WPF's
            // window design will be provided. There are many input events on the window's "Primary Message
            // Pump" (WndProc), and we won't understand the complexity of the window design as it pertains
            // to WPF's handling of the UI; task threads; the dispatcher thread; and normal event flow. 
            //
            // So, this is the documentation! ^_^  Begin posting messages here! ^_^
            // 
            if (msg == WM_DEVICECHANGE)
            {
                DEV_BROADCAST_HDR head;

                switch (wParam.ToInt32())
                {
                    case DBT_DEVICEARRIVAL:

                        head = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_HDR));

                        if (head.dbch_devicetype == DeviceType.DBT_DEVTYP_VOLUME)
                        {
                            var description = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_VOLUME));

                            if (description.dbcv_flags == VolumeChangeFlags.DBTF_MEDIA)
                            {
                                OnDeviceChanged(description, DeviceChangeEventType.DeviceInserted);
                            }
                        }
                        break;

                    case DBT_DEVICEREMOVECOMPLETE:

                        head = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_HDR));

                        if (head.dbch_devicetype == DeviceType.DBT_DEVTYP_VOLUME)
                        {
                            var description = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_VOLUME));

                            if (description.dbcv_flags == VolumeChangeFlags.DBTF_MEDIA)
                            {
                                OnDeviceChanged(description, DeviceChangeEventType.DeviceRemoved);
                            }
                        }
                        break;
                }
            }

            return IntPtr.Zero;
        }

        private void OnDeviceChanged(DEV_BROADCAST_VOLUME description, DeviceChangeEventType eventType)
        {
            if (description.Drives.Length > 0)
            {
                var driveLetter = description.Drives.First();

                _cdDrive.SetDevice(driveLetter, eventType);
            }
        }
        private void OnCDTracksLoaded(CDDeviceTracksLoadedEventArgs args)
        {
            _eventAggregator.GetEvent<CDPlayerLoadEvent>().Publish(args);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var viewModel = this.DataContext as MainViewModel;

            viewModel?.Dispose();

            base.OnClosing(e);
        }
    }
}