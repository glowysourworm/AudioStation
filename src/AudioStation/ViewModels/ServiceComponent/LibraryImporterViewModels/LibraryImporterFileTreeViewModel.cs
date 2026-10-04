using System.Collections;
using System.Diagnostics.CodeAnalysis;

using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels
{
    public class LibraryImporterFileTreeViewModel : IDictionary<string, LibraryImporterFileTreeNodeViewModel>, IList<LibraryImporterFileTreeNodeViewModel>, IReadOnlyCollection<LibraryImporterFileTreeNodeViewModel>
    {
        // This will be used for viewing the importer tree view
        SimpleTreeViewModel _tree;

        // This will be used for O(1) access
        Dictionary<string, LibraryImporterFileTreeNodeViewModel> _dictionary;

        // Begin / End Update pattern
        bool _updating;

        /// <summary>
        /// (Bubble Up Event) Tree wide property changed event. This should be fired once by the root node.
        /// </summary>
        public event TreeViewDelegates.ItemPropertyChangedTreeEventHandler ItemPropertyChangedEvent
        {
            add { _tree.ItemPropertyChangedEvent += value; }
            remove { _tree.ItemPropertyChangedEvent -= value; }
        }

        public LibraryImporterFileTreeViewModel()
        {
            _tree = new SimpleTreeViewModel();
            _dictionary = new Dictionary<string, LibraryImporterFileTreeNodeViewModel>();
        }

        #region (public) Begin / End Update:  This includes all modifiers

        public void BeginUpdate()
        {
            if (_updating)
                throw new Exception("Currently running an update! Please check that the other has finished");

            _updating = true;

            _tree.BeginUpdate();
        }
        public void EndUpdate()
        {
            if (!_updating)
                throw new Exception("No update has been started! Please check begin / end update ordering");

            _updating = false;

            _tree.EndUpdate();
        }
        public bool IsUpdating()
        {
            return _updating;
        }
        public void Add(LibraryImporterFileTreeNodeViewModel item)
        {
            _tree.Add(item);
            _dictionary.Add(item.FullPath, item);       // NOTE:  The FullPath property has been used for the key!
        }

        public void Add(string key, LibraryImporterFileTreeNodeViewModel value)
        {
            this.Add(value);
        }

        public void Add(KeyValuePair<string, LibraryImporterFileTreeNodeViewModel> item)
        {
            this.Add(item.Value);
        }

        public void Clear()
        {
            _dictionary.Clear();
            _tree.Clear();
        }
        public void Insert(int index, LibraryImporterFileTreeNodeViewModel item)
        {
            throw new NotSupportedException("This method is not supported for the LibraryImporterFileTreeViewModel");
        }

        public bool Remove(LibraryImporterFileTreeNodeViewModel item)
        {
            _tree.Remove(item);

            // NOTE:  The FullPath property has been used for the key!
            return _dictionary.Remove(item.FullPath);
        }

        public bool Remove(string key)
        {
            return this.Remove(_dictionary[key]);
        }

        public bool Remove(KeyValuePair<string, LibraryImporterFileTreeNodeViewModel> item)
        {
            throw new NotSupportedException("This method is not supported for the LibraryImporterFileTreeViewModel");
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException("This method is not supported for the LibraryImporterFileTreeViewModel");
        }
        #endregion

        #region (public) Collection Access
        public LibraryImporterFileTreeNodeViewModel this[int index]
        {
            get { throw new NotSupportedException("Cannot use indexing on SimpleTreeViewModel"); }
            set { throw new NotSupportedException("Cannot use indexing on SimpleTreeViewModel"); }
        }

        public LibraryImporterFileTreeNodeViewModel this[string key]
        {
            get { return _dictionary[key]; }
            set { throw new NotSupportedException("Cannot use set-indexing on SimpleTreeViewModel"); }
        }
        public int Count { get { return _dictionary.Count; } }
        public bool IsReadOnly { get { return true; } }
        public ICollection<string> Keys
        {
            get { return _dictionary.Keys; }
        }
        public ICollection<LibraryImporterFileTreeNodeViewModel> Values
        {
            get { return _dictionary.Values; }
        }

        public bool Contains(LibraryImporterFileTreeNodeViewModel item)
        {
            // This could be turned into ~O(Log(N)) by using the tree's numbering scheme
            return _dictionary.Values.Contains(item);
        }

        public bool Contains(KeyValuePair<string, LibraryImporterFileTreeNodeViewModel> item)
        {
            return _dictionary.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void CopyTo(LibraryImporterFileTreeNodeViewModel[] array, int arrayIndex)
        {
            throw new NotSupportedException("This method is not supported for the LibraryImporterFileTreeViewModel");
        }

        public void CopyTo(KeyValuePair<string, LibraryImporterFileTreeNodeViewModel>[] array, int arrayIndex)
        {
            throw new NotSupportedException("This method is not supported for the LibraryImporterFileTreeViewModel");
        }

        public int IndexOf(LibraryImporterFileTreeNodeViewModel item)
        {
            throw new NotSupportedException("This method is not supported for the LibraryImporterFileTreeViewModel");
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out LibraryImporterFileTreeNodeViewModel value)
        {
            throw new NotSupportedException("This method is not supported for the LibraryImporterFileTreeViewModel");
        }

        IEnumerator<LibraryImporterFileTreeNodeViewModel> IEnumerable<LibraryImporterFileTreeNodeViewModel>.GetEnumerator()
        {
            throw new NotSupportedException("This method is not supported for the LibraryImporterFileTreeViewModel");
        }

        IEnumerator<KeyValuePair<string, LibraryImporterFileTreeNodeViewModel>> IEnumerable<KeyValuePair<string, LibraryImporterFileTreeNodeViewModel>>.GetEnumerator()
        {
            throw new NotSupportedException("This method is not supported for the LibraryImporterFileTreeViewModel");
        }
        #endregion

        #region (public) UI SUPPORT

        // UI SUPPORT: This must be the tree view
        public IEnumerator GetEnumerator()
        {
            return _tree.GetEnumerator();
        }

        #endregion
    }
}
