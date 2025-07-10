using System.Windows;
using System.Windows.Forms;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Windows
{
    public partial class DialogWindow : Window
    {
        public event SimpleEventHandler<DialogResult> DialogResultEvent;

        public DialogWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Custom dialog result for non-modal "modal" dialog
            if (this.DialogResultEvent != null)
                this.DialogResultEvent(System.Windows.Forms.DialogResult.OK);
        }
    }
}
