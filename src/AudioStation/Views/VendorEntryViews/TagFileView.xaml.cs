using System.Windows.Controls;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Views.VendorEntryViews
{
    public partial class TagFileView : UserControl
    {
        public event SimpleEventHandler FindOnMusicBrainzEvent;

        public TagFileView()
        {
            InitializeComponent();
        }

        private void MusicBrainzButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.FindOnMusicBrainzEvent != null)
                this.FindOnMusicBrainzEvent();
        }
    }
}
