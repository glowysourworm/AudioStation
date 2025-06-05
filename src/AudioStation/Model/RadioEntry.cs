using System.Collections.ObjectModel;

using SimpleWpf.Extensions;

namespace AudioStation.Model
{
    public class RadioEntry : ViewModelBase
    {
        string _name;
        ObservableCollection<RadioEntryStreamInfo> _radioPlaylist;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public ObservableCollection<RadioEntryStreamInfo> RadioPlaylist
        {
            get { return _radioPlaylist; }
            set { this.RaiseAndSetIfChanged(ref _radioPlaylist, value); }
        }

        public RadioEntry()
        {
            this.RadioPlaylist = new ObservableCollection<RadioEntryStreamInfo>();
        }

        public override string ToString()
        {
            return this.Name + ":  " + this.RadioPlaylist.Count + " entries";
        }
    }
}
