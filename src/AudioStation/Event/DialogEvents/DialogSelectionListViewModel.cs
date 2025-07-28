using System.ComponentModel;
using System.Windows.Controls;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.ViewModel;

namespace AudioStation.Event.DialogEvents
{
    public class DialogSelectionListViewModel : ViewModelBase
    {
        SelectionMode _selectionMode;
        NotifyingObservableCollection<SelectionViewModel> _selectionList;

        /// <summary>
        /// List of items for the Selection List View option
        /// </summary>
        public NotifyingObservableCollection<SelectionViewModel> SelectionList
        {
            get { return _selectionList; }
            set { this.RaiseAndSetIfChanged(ref _selectionList, value); }
        }

        public SelectionMode SelectionMode
        {
            get { return _selectionMode; }
            set { this.RaiseAndSetIfChanged(ref _selectionMode, value); }
        }

        public DialogSelectionListViewModel()
        {
            this.SelectionList = new NotifyingObservableCollection<SelectionViewModel>();
            this.SelectionMode = SelectionMode.Single;
            this.SelectionList.ItemPropertyChanged += SelectionList_ItemPropertyChanged;
        }

        private void SelectionList_ItemPropertyChanged(SelectionViewModel item2,
                                                       PropertyChangedEventArgs item3)
        {
            OnPropertyChanged(nameof(SelectionList));
        }
    }
}
