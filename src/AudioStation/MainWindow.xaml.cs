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

        // Custom control template
        public Grid WindowRoot { get; private set; }
        public Grid LayoutRoot { get; private set; }
        public Button MinimizeButton { get; private set; }
        public Button MaximizeButton { get; private set; }
        public Button RestoreButton { get; private set; }
        public Button CloseButton { get; private set; }
        public Grid HeaderBar { get; private set; }
        public Popup UserMenuPopup { get; private set; }
        public Button PlayerModeButton { get; private set; }
        public Button ManagementModeButton { get; private set; }
        public Button ShowOutputButton { get; private set; }
        public Button ExitButton { get; private set; }

        public WindowState WindowStateOverride { get { return _windowStateOverride; } }

        protected bool IsHeaderMouseDown { get; private set; }
        protected Point HeaderMouseDownPosition { get; private set; }

        private readonly IViewModelController _viewModelController;
        private readonly IDialogController _dialogController;
        private readonly IIocEventAggregator _eventAggregator;

        // This may need to be calculated
        Thickness _DPIMargin = new Thickness(8);

        // Custom Maximize / Minimize (override)
        Point _positionNormal;
        Size _sizeNormal;
        bool _stateChangingOverride;
        bool _stateChanging;
        WindowState _windowStateOverride;
        WindowState _windowStateRequest;            // Handles main request method -> (BeginInvoke) -> State Override

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

            // Set to maximized "mode"
            this.Height = SystemParameters.MaximizedPrimaryScreenHeight - _DPIMargin.Top - _DPIMargin.Bottom;
            this.Width = SystemParameters.MaximizedPrimaryScreenWidth - _DPIMargin.Left - _DPIMargin.Right;

            this.Left = 0;
            this.Top = 0;
            //-----------

            // Track position / size of "Normal" window state
            _positionNormal = new Point(this.Left, this.Top);
            _sizeNormal = new Size(this.Width, this.Height);                // These must be set in the markup, or initialized here!
            _stateChangingOverride = false;
            _windowStateOverride = this.WindowState;

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

        protected override void OnStateChanged(EventArgs e)
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.ApplicationClosing)
                return;

            // WindowState = Requested State
            //
            _stateChanging = true;              // Event Blocker

            // Prevent Window State (base)
            _windowStateRequest = this.WindowState;

            ApplicationHelpers.BeginInvokeDispatcher(() =>
            {
                _windowStateOverride = _windowStateRequest;

                WindowStateChangeOverride();

                _stateChanging = false;

            }, DispatcherPriority.Background);
        }

        private void WindowStateChangeOverride()
        {
            _stateChangingOverride = true;                  // Event Blocker

            if (_windowStateOverride == WindowState.Maximized)
            {
                this.Height = SystemParameters.MaximizedPrimaryScreenHeight - _DPIMargin.Top - _DPIMargin.Bottom;
                this.Width = SystemParameters.MaximizedPrimaryScreenWidth - _DPIMargin.Left - _DPIMargin.Right;

                this.Left = 0;
                this.Top = 0;
            }
            else if (_windowStateOverride == WindowState.Normal)
            {
                this.Height = _sizeNormal.Height;
                this.Width = _sizeNormal.Width;
                this.Left = _positionNormal.X;
                this.Top = _positionNormal.Y;
            }
            else if (_windowStateOverride == WindowState.Minimized)
            {
                // This should be handled by the base control behavior
            }

            _stateChangingOverride = false;

            OnWindowOverridesChanged();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            TrackWindowPositionAndSize();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (this.IsHeaderMouseDown)
            {
                TrackWindowPositionAndSize();
            }
        }

        private void TrackWindowPositionAndSize()
        {
            if (_windowStateOverride == WindowState.Normal &&
                !_stateChangingOverride &&
                !_stateChanging)
            {
                _positionNormal.X = this.Left;
                _positionNormal.Y = this.Top;
                _sizeNormal.Width = this.RenderSize.Width;
                _sizeNormal.Height = this.RenderSize.Height;
            }
        }

        protected T GetRequiredTemplateChild<T>(string childName) where T : DependencyObject
        {
            return (T)base.GetTemplateChild(childName);
        }

        public override void OnApplyTemplate()
        {
            this.WindowRoot = this.GetRequiredTemplateChild<Grid>("WindowRoot");
            this.LayoutRoot = this.GetRequiredTemplateChild<Grid>("LayoutRoot");
            this.MinimizeButton = this.GetRequiredTemplateChild<Button>("MinimizeButton");
            this.MaximizeButton = this.GetRequiredTemplateChild<Button>("MaximizeButton");
            this.RestoreButton = this.GetRequiredTemplateChild<Button>("RestoreButton");
            this.CloseButton = this.GetRequiredTemplateChild<Button>("CloseButton");
            this.HeaderBar = this.GetRequiredTemplateChild<Grid>("PART_HeaderBar");
            this.UserMenuPopup = this.GetRequiredTemplateChild<Popup>("UserMenuPopup");
            this.PlayerModeButton = this.GetRequiredTemplateChild<Button>("PlayerModeButton");
            this.ManagementModeButton = this.GetRequiredTemplateChild<Button>("ManagementModeButton");
            this.ShowOutputButton = this.GetRequiredTemplateChild<Button>("ShowOutputButton");
            this.ExitButton = this.GetRequiredTemplateChild<Button>("ExitButton");

            if (this.CloseButton != null)
            {
                this.CloseButton.Click += CloseButton_Click;
            }

            if (this.MinimizeButton != null)
            {
                this.MinimizeButton.Click += MinimizeButton_Click;
            }

            if (this.RestoreButton != null)
            {
                this.RestoreButton.Click += RestoreButton_Click;
            }

            if (this.MaximizeButton != null)
            {
                this.MaximizeButton.Click += MaximizeButton_Click;
            }

            if (this.HeaderBar != null)
            {
                this.HeaderBar.AddHandler(Grid.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.OnHeaderBarMouseLeftButtonDown));
                this.HeaderBar.AddHandler(Grid.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.OnHeaderBarMouseLeftButtonUp));
                this.HeaderBar.AddHandler(Grid.MouseLeaveEvent, new MouseEventHandler(this.OnHeaderBarMouseLeave));
            }

            // User Menu
            if (this.UserMenuPopup != null)
            {
                this.UserMenuPopup.MouseLeave += (sender, e) =>
                {
                    this.UserMenuPopup.IsOpen = false;
                };
            }
            if (this.PlayerModeButton != null)
            {
                this.PlayerModeButton.Click += (sender, e) =>
                {
                    this.UserMenuPopup.IsOpen = false;
                };
            }
            if (this.ManagementModeButton != null)
            {
                this.ManagementModeButton.Click += (sender, e) =>
                {
                    this.UserMenuPopup.IsOpen = false;
                };
            }
            if (this.ShowOutputButton != null)
            {
                this.ShowOutputButton.Click += (sender, e) =>
                {
                    this.UserMenuPopup.IsOpen = false;

                    _dialogController.ShowLogWindow(_viewModelController.GetMainViewModel().Log);

                    //_mainViewModel.ShowOutputMessages = !_mainViewModel.ShowOutputMessages;
                };
            }
            if (this.ExitButton != null)
            {
                this.ExitButton.Click += (sender, e) =>
                {
                    this.UserMenuPopup.IsOpen = false;
                    this.Close();
                };
            }

            base.OnApplyTemplate();
        }

        // Window Drag Behavior
        protected virtual void OnHeaderBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Double click maximize / minimize
            if (e.ClickCount == 2 && base.ResizeMode == ResizeMode.CanResize)
            {
                // Set Window State (override)
                _windowStateOverride = _windowStateOverride == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                WindowStateChangeOverride();

                return;
            }

            this.IsHeaderMouseDown = true;
            this.HeaderMouseDownPosition = e.GetPosition(this);

            if (_windowStateOverride != WindowState.Maximized)
            {
                try
                {
                    base.DragMove();
                }
                catch (InvalidOperationException ex)
                {
                    this.IsHeaderMouseDown = false;
                    this.HeaderMouseDownPosition = new Point();
                    return;
                }
            }
        }
        private void OnHeaderBarMouseLeave(object sender, MouseEventArgs e)
        {
            this.IsHeaderMouseDown = false;
        }

        private void OnHeaderBarMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.IsHeaderMouseDown = false;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            _windowStateOverride = WindowState.Maximized;

            WindowStateChangeOverride();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            _windowStateOverride = WindowState.Normal;

            WindowStateChangeOverride();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            // -> (BeginInvoke) -> (override)
            this.WindowState = WindowState.Minimized;

            WindowStateChangeOverride();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnWindowOverridesChanged()
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs("WindowStateOverride"));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var viewModel = this.DataContext as MainViewModel;

            viewModel?.Dispose();

            base.OnClosing(e);
        }
    }
}