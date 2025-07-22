using System.Windows;

namespace AudioStation.Windows
{
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
