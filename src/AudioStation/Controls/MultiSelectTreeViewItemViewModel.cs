using System.Collections;
using System.Collections.ObjectModel;

using SimpleWpf.ViewModel;

namespace AudioStation.Controls
{
    public class MultiSelectTreeItemViewModel : ViewModelBase
    {
        object _item;
        object _itemDisplay;
        bool _isSelected;
        bool _isExpanded;
        bool _canHaveChildren;
        IEnumerable _children;
        MultiSelectTreeItemViewModel _parent;

        public object Item
        {
            get { return _item; }
            set { this.RaiseAndSetIfChanged(ref _item, value); }
        }
        public object ItemDisplay
        {
            get { return _itemDisplay; }
            set { this.RaiseAndSetIfChanged(ref _itemDisplay, value); }
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
        public MultiSelectTreeItemViewModel Parent
        {
            get { return _parent; }
            set { this.RaiseAndSetIfChanged(ref _parent, value); }
        }


        public MultiSelectTreeItemViewModel()
        {
            this.Item = null;
            this.ItemDisplay = null;
            this.IsExpanded = false;
            this.IsSelected = false;
            this.CanHaveChildren = false;
            this.Children = new ObservableCollection<MultiSelectTreeItemViewModel>();
            this.Parent = null;
        }
        public MultiSelectTreeItemViewModel(MultiSelectTreeItemViewModel parent, object item, object itemDisplay, bool canHaveChildren)
        {
            this.Item = item;
            this.ItemDisplay = itemDisplay;
            this.IsExpanded = false;
            this.IsSelected = false;
            this.CanHaveChildren = canHaveChildren;
            this.Children = new ObservableCollection<MultiSelectTreeItemViewModel>();
            this.Parent = parent;
        }
    }
}
