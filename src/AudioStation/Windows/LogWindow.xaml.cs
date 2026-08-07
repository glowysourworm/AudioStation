using System.Windows;

namespace AudioStation.Windows
{
    public partial class LogWindow : Window
    {
        public LogWindow()
        {
            InitializeComponent();

            this.DataContextChanged += LogWindow_DataContextChanged;
            this.OutputTabControl.SelectionChanged += OutputTabControl_SelectionChanged;
        }

        private void LogWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.LogLevelCB.SelectedIndex = 0;
            this.SubLogCB.SelectedIndex = 0;
        }

        private void OutputTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.LogLevelCB.SelectedIndex = 0;
            this.SubLogCB.SelectedIndex = 0;
        }

        private void SubLogCB_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.LogLevelCB.SelectedIndex = 0;
            this.SubLogCB.SelectedIndex = 0;
        }

        private void LogLevelCB_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.LogLevelCB.SelectedIndex = 0;
            this.SubLogCB.SelectedIndex = 0;
        }
    }
}
