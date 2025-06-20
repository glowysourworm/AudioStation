using System.Windows.Controls;

using AudioStation.Core.Model;
using AudioStation.ViewModels.Vendor;
using AudioStation.ViewModels.Vendor.MusicBrainzViewModel;

namespace AudioStation.Views.VendorEntryViews
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
                var record = e.AddedItems[0] as MusicBrainzRecordingViewModel;

                // Set selected record in the primary LibraryEntry
                //entry.MusicBrainzRecord = record;
            }
        }
    }
}
