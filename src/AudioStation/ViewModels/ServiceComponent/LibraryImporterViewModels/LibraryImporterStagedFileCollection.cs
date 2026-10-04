using System.Collections;
using System.ComponentModel;

using AudioStation.Core.Component.LibraryLoaderComponent;

using SimpleWpf.UI.ViewModel;
using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels
{
    /// <summary>
    /// Collection container for the staged files. Maintains separate collections for: IsSelected; 
    /// IsValid; (not) IsValid; Has Music Brianz Result(s); has (special) Music Brainz Tag Result; 
    /// has AcoustID Result; etc...
    /// </summary>
    public class LibraryImporterStagedFileCollection : ViewModelBase, IList<LibraryImporterFileTreeNodeViewModel>
    {
        private List<LibraryImporterFileTreeNodeViewModel> _fileList;
        private LibraryImporterFileTreeViewModel _tree;

        private LibraryImporterFileTreeViewModel _selectedFiles;
        private LibraryImporterFileTreeViewModel _validFiles;
        private LibraryImporterFileTreeViewModel _invalidFiles;
        private LibraryImporterFileTreeViewModel _acoustIDFiles;
        private LibraryImporterFileTreeViewModel _musicBrainzFiles;
        private LibraryImporterFileTreeViewModel _musicBrainzSpecialTagFiles;
        private LibraryImporterFileTreeViewModel _libraryConflictFiles;
        private LibraryImporterFileTreeViewModel _importReadyFiles;
        private LibraryImporterFileTreeViewModel _successfulFiles;
        private LibraryImporterFileTreeViewModel _failureFiles;

        public LibraryImporterStagedFileCollection()
        {
            _tree = new LibraryImporterFileTreeViewModel();
            _fileList = new List<LibraryImporterFileTreeNodeViewModel>();
            _selectedFiles = new LibraryImporterFileTreeViewModel();
            _validFiles = new LibraryImporterFileTreeViewModel();
            _invalidFiles = new LibraryImporterFileTreeViewModel();
            _acoustIDFiles = new LibraryImporterFileTreeViewModel();
            _musicBrainzFiles = new LibraryImporterFileTreeViewModel();
            _musicBrainzSpecialTagFiles = new LibraryImporterFileTreeViewModel();
            _libraryConflictFiles = new LibraryImporterFileTreeViewModel();
            _importReadyFiles = new LibraryImporterFileTreeViewModel();
            _successfulFiles = new LibraryImporterFileTreeViewModel();
            _failureFiles = new LibraryImporterFileTreeViewModel();

            _tree.ItemPropertyChangedEvent += File_PropertyChanged;
        }

        public LibraryImporterStagedFileCollection(IEnumerable<LibraryImporterFileTreeNodeViewModel> stagedFiles)
            : this()
        {
            BeginUpdate();

            foreach (var file in stagedFiles)
            {
                AddUpdate(file);
            }

            EndUpdate(true);
        }

        public LibraryImporterFileTreeNodeViewModel this[int index]
        {
            get { return _fileList[index]; }
            set { throw new NotSupportedException(); }
        }

        public LibraryImporterFileTreeViewModel Tree
        {
            get { return _tree; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeNodeViewModel> Files
        {
            get { return _fileList; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeNodeViewModel> SelectedFiles
        {
            get { return _selectedFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeNodeViewModel> ValidFiles
        {
            get { return _validFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeNodeViewModel> InvalidFiles
        {
            get { return _invalidFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeNodeViewModel> AcoustIDFiles
        {
            get { return _acoustIDFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeNodeViewModel> MusicBrainzFiles
        {
            get { return _musicBrainzFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeNodeViewModel> MusicBrainzSpecialTagFiles
        {
            get { return _musicBrainzSpecialTagFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeNodeViewModel> LibraryConflictFiles
        {
            get { return _libraryConflictFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeNodeViewModel> ImportReadyFiles
        {
            get { return _importReadyFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeNodeViewModel> SuccessfulFiles
        {
            get { return _successfulFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeNodeViewModel> FailureFiles
        {
            get { return _failureFiles; }
        }

        public void BeginUpdate()
        {
            _tree.BeginUpdate();
            _selectedFiles.BeginUpdate();
            _validFiles.BeginUpdate();
            _invalidFiles.BeginUpdate();
            _acoustIDFiles.BeginUpdate();
            _musicBrainzFiles.BeginUpdate();
            _musicBrainzSpecialTagFiles.BeginUpdate();
            _libraryConflictFiles.BeginUpdate();
            _importReadyFiles.BeginUpdate();
            _successfulFiles.BeginUpdate();
            _failureFiles.BeginUpdate();
        }
        public void EndUpdate(bool notifyObservers = false)
        {
            _tree.EndUpdate();
            _selectedFiles.EndUpdate();
            _validFiles.EndUpdate();
            _invalidFiles.EndUpdate();
            _acoustIDFiles.EndUpdate();
            _musicBrainzFiles.EndUpdate();
            _musicBrainzSpecialTagFiles.EndUpdate();
            _libraryConflictFiles.EndUpdate();
            _importReadyFiles.EndUpdate();
            _successfulFiles.EndUpdate();
            _failureFiles.EndUpdate();
        }

        private void AddUpdate(LibraryImporterFileTreeNodeViewModel file)
        {
            // All Files ~ O(1)
            if (!_tree.ContainsKey(file.FullPath))
            {
                _tree.Add(file.FullPath, file);
                _fileList.Add(file);
            }

            // Add/Remove from the rest of the child collections
            MaintainCollections(file);

            OnPropertyChanged("Count");
        }

        private void File_PropertyChanged(TreeViewNodeModelBase treeSender, object item, PropertyChangedEventArgs eventArgs)
        {
            // The tree sender should be the root. We want the item itself.
            var file = item as LibraryImporterFileTreeNodeViewModel;

            if (file != null)
            {
                var updating = _tree.IsUpdating();

                if (!updating)
                    BeginUpdate();

                MaintainCollections(file);

                if (!updating)
                    EndUpdate();
            }
        }

        private void MaintainCollections(LibraryImporterFileTreeNodeViewModel file)
        {
            // Maintain Collections Procedure:  We want to avoid performance degradataion.
            // So, this should be the only place we need to watch item properties - which
            // is why this collection was built.
            //
            // Just check property names here; and update the collections.
            //

            // IsSelected
            if (file.IsSelected && !_selectedFiles.ContainsKey(file.FullPath))
            {
                _selectedFiles.Add(file.FullPath, file);
            }
            else if (!file.IsSelected && _selectedFiles.ContainsKey(file.FullPath))
            {
                _selectedFiles.Remove(file.FullPath);
            }

            // IsValid (Bubble Up Event)
            if (file.TagRecordDirty.IsValid && !_validFiles.ContainsKey(file.FullPath))
            {
                _validFiles.Add(file.FullPath, file);
            }
            else if (!file.TagRecordDirty.IsValid && _validFiles.ContainsKey(file.FullPath))
            {
                _validFiles.Remove(file.FullPath);
            }

            // (not) IsValid (Bubble Up Event)
            if (!file.TagRecordDirty.IsValid && !_invalidFiles.ContainsKey(file.FullPath))
            {
                _invalidFiles.Add(file.FullPath, file);
            }
            else if (file.TagRecordDirty.IsValid && _invalidFiles.ContainsKey(file.FullPath))
            {
                _invalidFiles.Remove(file.FullPath);
            }

            // AcoustID
            if (file.ImportOutput.AcoustIDResults.Any() && !_acoustIDFiles.ContainsKey(file.FullPath))
            {
                _acoustIDFiles.Add(file.FullPath, file);
            }
            else if (!file.ImportOutput.AcoustIDResults.Any() && _acoustIDFiles.ContainsKey(file.FullPath))
            {
                _acoustIDFiles.Remove(file.FullPath);
            }

            // Music Brainz (basic) (Bubble Up Event)
            if (file.ImportOutput.MusicBrainzAcoustIDResults.Any() && !_musicBrainzFiles.ContainsKey(file.FullPath))
            {
                _musicBrainzFiles.Add(file.FullPath, file);
            }
            else if (!file.ImportOutput.MusicBrainzAcoustIDResults.Any() && _musicBrainzFiles.ContainsKey(file.FullPath))
            {
                _musicBrainzFiles.Remove(file.FullPath);
            }

            // Music Brainz (special tag)
            if (file.MusicBrainzReleaseTrackQuerySuccess && !_musicBrainzSpecialTagFiles.ContainsKey(file.FullPath))
            {
                _musicBrainzSpecialTagFiles.Add(file.FullPath, file);
            }
            else if (!file.MusicBrainzReleaseTrackQuerySuccess && _musicBrainzSpecialTagFiles.ContainsKey(file.FullPath))
            {
                _musicBrainzSpecialTagFiles.Remove(file.FullPath);
            }

            // Library Conflict
            if (file.LibraryConflict && !_libraryConflictFiles.ContainsKey(file.FullPath))
            {
                _libraryConflictFiles.Add(file.FullPath, file);
            }
            else if (!file.LibraryConflict && _libraryConflictFiles.ContainsKey(file.FullPath))
            {
                _libraryConflictFiles.Remove(file.FullPath);
            }

            // Successful Import
            if (file.ImportOutput.ImportResult == LibraryWorkerResultLevel.Success && !_successfulFiles.ContainsKey(file.FullPath))
            {
                _successfulFiles.Add(file.FullPath, file);
            }
            else if (file.ImportOutput.ImportResult != LibraryWorkerResultLevel.Success && _successfulFiles.ContainsKey(file.FullPath))
            {
                _successfulFiles.Remove(file.FullPath);
            }

            // Failed Import
            if (file.ImportOutput.ImportResult != LibraryWorkerResultLevel.Success &&
                file.ImportOutput.ImportResult != LibraryWorkerResultLevel.None &&
                !_failureFiles.ContainsKey(file.FullPath))
            {
                _failureFiles.Add(file.FullPath, file);
            }
            else if ((file.ImportOutput.ImportResult == LibraryWorkerResultLevel.Success ||
                      file.ImportOutput.ImportResult == LibraryWorkerResultLevel.None) &&
                      _failureFiles.ContainsKey(file.FullPath))
            {
                _failureFiles.Remove(file.FullPath);
            }

            // Import Ready
            if (_validFiles.ContainsKey(file.FullPath))
            {
                if (file.ImportOutput.ImportResult == LibraryWorkerResultLevel.None && !_importReadyFiles.ContainsKey(file.FullPath))
                {
                    _importReadyFiles.Add(file.FullPath, file);
                }
                else if (file.ImportOutput.ImportResult != LibraryWorkerResultLevel.None && _importReadyFiles.ContainsKey(file.FullPath))
                {
                    _importReadyFiles.Remove(file.FullPath);
                }
            }
        }

        #region (public) IList
        public int Count { get { return _tree.Count; } }
        public bool IsReadOnly { get { return false; } }

        public void Add(LibraryImporterFileTreeNodeViewModel item)
        {
            BeginUpdate();
            AddUpdate(item);
            EndUpdate(true);
        }

        public void Clear()
        {
            _tree.Clear();
            _fileList.Clear();
            _selectedFiles.Clear();
            _validFiles.Clear();
            _invalidFiles.Clear();
            _acoustIDFiles.Clear();
            _musicBrainzFiles.Clear();
            _musicBrainzSpecialTagFiles.Clear();
            _libraryConflictFiles.Clear();
            _importReadyFiles.Clear();
            _successfulFiles.Clear();
            _failureFiles.Clear();

            OnPropertyChanged("Count");
        }

        public bool Contains(LibraryImporterFileTreeNodeViewModel item)
        {
            return _tree.ContainsKey(item.FullPath);
        }

        public void CopyTo(LibraryImporterFileTreeNodeViewModel[] array, int arrayIndex)
        {
            _fileList.CopyTo(array, arrayIndex);
        }
        public int IndexOf(LibraryImporterFileTreeNodeViewModel item)
        {
            return _fileList.IndexOf(item);
        }

        public void Insert(int index, LibraryImporterFileTreeNodeViewModel item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(LibraryImporterFileTreeNodeViewModel file)
        {
            _tree.Remove(file.FullPath);
            _fileList.Remove(file);

            // IsSelected
            if (_selectedFiles.ContainsKey(file.FullPath))
            {
                _selectedFiles.Remove(file.FullPath);
            }

            // IsValid
            if (_validFiles.ContainsKey(file.FullPath))
                _validFiles.Remove(file.FullPath);

            // (not) IsValid
            if (_invalidFiles.ContainsKey(file.FullPath))
                _invalidFiles.Remove(file.FullPath);

            // AcoustID
            if (_acoustIDFiles.ContainsKey(file.FullPath))
                _acoustIDFiles.Remove(file.FullPath);

            // Music Brainz (basic)
            if (_musicBrainzFiles.ContainsKey(file.FullPath))
                _musicBrainzFiles.Remove(file.FullPath);

            // Music Brainz (special tag)
            if (_musicBrainzSpecialTagFiles.ContainsKey(file.FullPath))
                _musicBrainzSpecialTagFiles.Remove(file.FullPath);

            // Library Conflict
            if (_libraryConflictFiles.ContainsKey(file.FullPath))
                _libraryConflictFiles.Remove(file.FullPath);

            // Import Ready
            if (_importReadyFiles.ContainsKey(file.FullPath))
                _importReadyFiles.Remove(file.FullPath);

            // Successful Files
            if (_successfulFiles.ContainsKey(file.FullPath))
                _successfulFiles.Remove(file.FullPath);

            // Failed Files
            if (_failureFiles.ContainsKey(file.FullPath))
                _failureFiles.Remove(file.FullPath);

            OnPropertyChanged("Count");

            return true;
        }

        public void RemoveAt(int index)
        {
            var item = _fileList[index];

            Remove(item);
        }
        IEnumerator<LibraryImporterFileTreeNodeViewModel> IEnumerable<LibraryImporterFileTreeNodeViewModel>.GetEnumerator()
        {
            return _fileList.GetEnumerator();
        }
        public IEnumerator GetEnumerator()
        {
            return _tree.GetEnumerator();
        }
        #endregion
    }
}
