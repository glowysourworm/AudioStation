using System.Collections;
using System.Collections.ObjectModel;

using SimpleWpf.ViewModel;

namespace AudioStation.Controls
{
    public class MultiSelectTreeItemViewModel : ViewModelBase
    {
        object _item;
        bool _isSelected;
        bool _isExpanded;
        bool _canHaveChildren;
        IEnumerable _children;

        public object Item
        {
            get { return _item; }
            set { this.RaiseAndSetIfChanged(ref _item, value); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { this.RaiseAndSetIfChanged(ref _isExpanded, value); }
        }
        public bool CanHaveChildren
        {
            get { return _canHaveChildren; }
            set { this.RaiseAndSetIfChanged(ref _canHaveChildren, value); }
        }
        public IEnumerable Children
        {
            get { return _children; }
            set { this.RaiseAndSetIfChanged(ref _children, value); }
        }


        public MultiSelectTreeItemViewModel()
        {
            this.Item = null;
            this.IsExpanded = false;
            this.IsSelected = false;
            this.CanHaveChildren = false;
            this.Children = new ObservableCollection<object>();
        }
        public MultiSelectTreeItemViewModel(object item, bool canHaveChildren)
        {
            this.Item = item;
            this.IsExpanded = false;
            this.IsSelected = false;
            this.CanHaveChildren = canHaveChildren;
            this.Children = new ObservableCollection<object>();
        }
    }
}
