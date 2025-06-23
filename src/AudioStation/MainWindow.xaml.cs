using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

using AudioStation.ViewModels;

using Microsoft.Win32;
using Microsoft.Windows.Shell;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation
{
    /// <summary>
    /// IDisposable pattern implemented by Application object OnExit(..)
    /// </summary>
    [IocExportDefault]
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
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
        protected Point HeaderMouseDownPosition { get;private set; }

        private readonly MainViewModel _mainViewModel;
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
        }

        [IocImportingConstructor]
        public MainWindow(IIocEventAggregator eventAggregator, MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _eventAggregator = eventAggregator;

            InitializeComponent();

            this.DataContext = mainViewModel;
            
            // Track position / size of "Normal" window state
            _positionNormal = new Point(this.Left, this.Top);
            _sizeNormal = new Size(this.RenderSize.Width, this.RenderSize.Height);
            _stateChangingOverride = false;
            _windowStateOverride = this.WindowState;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            // WindowState = Requested State
            //
            _stateChanging = true;              // Event Blocker

            // Prevent Window State (base)
            _windowStateRequest = this.WindowState;

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _windowStateOverride = _windowStateRequest;

                WindowStateChangeOverride();

                _stateChanging = false;
            });
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

                    _mainViewModel.ShowOutputMessages = !_mainViewModel.ShowOutputMessages;
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