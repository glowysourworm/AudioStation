using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using AudioStation.ViewModels;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation
{
    /// <summary>
    /// IDisposable pattern implemented by Application object OnExit(..)
    /// </summary>
    [IocExportDefault]
    public partial class MainWindow : Window
    {
        // Custom control template
        public Grid WindowRoot { get; private set; }
        public Grid LayoutRoot { get; private set; }
        public Button MinimizeButton { get; private set; }
        public Button MaximizeButton { get; private set; }
        public Button RestoreButton { get; private set; }
        public Button CloseButton { get; private set; }
        public Grid HeaderBar { get; private set; }

        protected bool IsHeaderMouseDown { get; private set; }
        protected Point HeaderMouseDownPosition { get;private set; }

        // Needed by the framework
        public MainWindow()
        {
            InitializeComponent();
        }

        [IocImportingConstructor]
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();

            this.DataContext = mainViewModel;
        }

        public T GetRequiredTemplateChild<T>(string childName) where T : DependencyObject
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
            }

            base.OnApplyTemplate();
        }

        // Window Drag Behavior
        protected virtual void OnHeaderBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Double click maximize / minimize
            if (e.ClickCount == 2 && base.ResizeMode == ResizeMode.CanResize)
            {
                this.ToggleWindowState();
                return;
            }

            this.IsHeaderMouseDown = true;
            this.HeaderMouseDownPosition = e.GetPosition(this);

            if (this.WindowState != WindowState.Maximized)
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

        protected void ToggleWindowState()
        {
            if (base.WindowState != WindowState.Maximized)
            {
                base.WindowState = WindowState.Maximized;
            }
            else
            {
                base.WindowState = WindowState.Normal;
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleWindowState();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleWindowState();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var viewModel = this.DataContext as MainViewModel;

            viewModel?.Dispose();

            base.OnClosing(e);
        }
    }
}