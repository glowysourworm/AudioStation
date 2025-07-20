using SimpleWpf.Extensions;

namespace AudioStation.Event.DialogEvents
{
    public class SelectionViewModel : ViewModelBase
    {
        object _item;
        string _itemText;
        bool _selected;

        public object Item
        {
            get { return _item; }
            set { this.RaiseAndSetIfChanged(ref _item, value); }
        }
        public string ItemText
        {
            get { return _itemText; }
            set { this.RaiseAndSetIfChanged(ref _itemText, value); }
        }
        public bool Selected
        {
            get { return _selected; }
            set { this.RaiseAndSetIfChanged(ref _selected, value); }
        }

        public SelectionViewModel(object item, string itemText, bool selected)
        {
            this.Item = item;
            this.ItemText = itemText;
            this.Selected = selected;
        }
    }
}
