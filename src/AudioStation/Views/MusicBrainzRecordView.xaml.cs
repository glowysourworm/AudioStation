using System.Windows.Controls;

using AudioStation.Model;
using AudioStation.Model.Vendor;

namespace AudioStation.Views
{
    public partial class MusicBrainzRecordView : UserControl
    {
        public MusicBrainzRecordView()
        {
            InitializeComponent();
        }

        private void OnMusicBrainzResultsChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var entry = this.DataContext as LibraryEntry;
                var record = e.AddedItems[0] as MusicBrainzRecord;

                // Set selected record in the primary LibraryEntry
                entry.MusicBrainzRecord = record;
            }
        }
    }
}
