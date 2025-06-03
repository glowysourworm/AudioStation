using AudioStation.ViewModels;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.ObservableCollection;

namespace AudioStation.Model
{
    public class Album : ViewModelBase
    {
        string _name;
        uint _year;
        SortedObservableCollection<Artist> _artists;
    }
}
