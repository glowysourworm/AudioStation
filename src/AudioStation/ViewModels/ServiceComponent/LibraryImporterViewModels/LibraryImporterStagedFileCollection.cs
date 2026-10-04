using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

using AudioStation.Core.Component.LibraryLoaderComponent;

using SimpleWpf.Extensions.Event;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels
{
    /// <summary>
    /// Collection container for the staged files. Maintains separate collections for: IsSelected; 
    /// IsValid; (not) IsValid; Has Music Brianz Result(s); has (special) Music Brainz Tag Result; 
    /// has AcoustID Result; etc...
    /// </summary>
    public class LibraryImporterStagedFileCollection : ViewModelBase, IEnumerable, IList<LibraryImporterFileTreeViewModel>, INotifyCollectionChanged
    {
        private List<LibraryImporterFileTreeViewModel> _fileList;
        private KeyedObservableCollection<string, LibraryImporterFileTreeViewModel> _files;
        private KeyedObservableCollection<string, LibraryImporterFileTreeViewModel> _selectedFiles;
        private KeyedObservableCollection<string, LibraryImporterFileTreeViewModel> _validFiles;
        private KeyedObservableCollection<string, LibraryImporterFileTreeViewModel> _invalidFiles;
        private KeyedObservableCollection<string, LibraryImporterFileTreeViewModel> _acoustIDFiles;
        private KeyedObservableCollection<string, LibraryImporterFileTreeViewModel> _musicBrainzFiles;
        private KeyedObservableCollection<string, LibraryImporterFileTreeViewModel> _musicBrainzSpecialTagFiles;
        private KeyedObservableCollection<string, LibraryImporterFileTreeViewModel> _libraryConflictFiles;
        private KeyedObservableCollection<string, LibraryImporterFileTreeViewModel> _importReadyFiles;
        private KeyedObservableCollection<string, LibraryImporterFileTreeViewModel> _successfulFiles;
        private KeyedObservableCollection<string, LibraryImporterFileTreeViewModel> _failureFiles;

        public event CollectionItemChangedHandler<LibraryImporterFileTreeViewModel> ItemPropertyChanged
        {
            add { _files.ItemPropertyChanged += value; }
            remove { _files.ItemPropertyChanged -= value; }
        }
        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add { _files.CollectionChanged += value; }
            remove { _files.CollectionChanged -= value; }
        }
        public event SimpleEventHandler SelectionChanged;

        public LibraryImporterStagedFileCollection()
        {
            _files = new KeyedObservableCollection<string, LibraryImporterFileTreeViewModel>();
            _fileList = new List<LibraryImporterFileTreeViewModel>();
            _selectedFiles = new KeyedObservableCollection<string, LibraryImporterFileTreeViewModel>();
            _validFiles = new KeyedObservableCollection<string, LibraryImporterFileTreeViewModel>();
            _invalidFiles = new KeyedObservableCollection<string, LibraryImporterFileTreeViewModel>();
            _acoustIDFiles = new KeyedObservableCollection<string, LibraryImporterFileTreeViewModel>();
            _musicBrainzFiles = new KeyedObservableCollection<string, LibraryImporterFileTreeViewModel>();
            _musicBrainzSpecialTagFiles = new KeyedObservableCollection<string, LibraryImporterFileTreeViewModel>();
            _libraryConflictFiles = new KeyedObservableCollection<string, LibraryImporterFileTreeViewModel>();
            _importReadyFiles = new KeyedObservableCollection<string, LibraryImporterFileTreeViewModel>();
            _successfulFiles = new KeyedObservableCollection<string, LibraryImporterFileTreeViewModel>();
            _failureFiles = new KeyedObservableCollection<string, LibraryImporterFileTreeViewModel>();
        }
        public LibraryImporterStagedFileCollection(IEnumerable<LibraryImporterFileTreeViewModel> stagedFiles)
            : this()
        {
            foreach (var file in stagedFiles)
            {
                AddUpdate(file);
            }
        }

        public LibraryImporterFileTreeViewModel this[int index]
        {
            get { return _fileList[index]; }
            set { throw new NotSupportedException(); }
        }

        public IReadOnlyCollection<LibraryImporterFileTreeViewModel> Files
        {
            get { return _files; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeViewModel> SelectedFiles
        {
            get { return _selectedFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeViewModel> ValidFiles
        {
            get { return _validFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeViewModel> InvalidFiles
        {
            get { return _invalidFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeViewModel> AcoustIDFiles
        {
            get { return _acoustIDFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeViewModel> MusicBrainzFiles
        {
            get { return _musicBrainzFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeViewModel> MusicBrainzSpecialTagFiles
        {
            get { return _musicBrainzSpecialTagFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeViewModel> LibraryConflictFiles
        {
            get { return _libraryConflictFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeViewModel> ImportReadyFiles
        {
            get { return _importReadyFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeViewModel> SuccessfulFiles
        {
            get { return _successfulFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileTreeViewModel> FailureFiles
        {
            get { return _failureFiles; }
        }

        public void BeginUpdate()
        {
            _files.BeginUpdate();
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
            _files.EndUpdate(notifyObservers);
            _selectedFiles.EndUpdate(notifyObservers);
            _validFiles.EndUpdate(notifyObservers);
            _invalidFiles.EndUpdate(notifyObservers);
            _acoustIDFiles.EndUpdate(notifyObservers);
            _musicBrainzFiles.EndUpdate(notifyObservers);
            _musicBrainzSpecialTagFiles.EndUpdate(notifyObservers);
            _libraryConflictFiles.EndUpdate(notifyObservers);
            _importReadyFiles.EndUpdate(notifyObservers);
            _successfulFiles.EndUpdate(notifyObservers);
            _failureFiles.EndUpdate(notifyObservers);
        }

        private void AddUpdate(LibraryImporterFileTreeViewModel file)
        {
            // All Files ~ O(1)
            if (!_files.ContainsKey(file.FullPath))
            {
                _files.Add(file.FullPath, file);
                _fileList.Add(file);
            }

            // Add/Remove from the rest of the child collections
            MaintainCollections(file);

            OnPropertyChanged("Count");

            // Bubble Up Events (There are two on the staged file object)
            file.PropertyChanged -= File_PropertyChanged;
            file.PropertyChanged += File_PropertyChanged;
        }

        private void File_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var file = sender as LibraryImporterFileTreeViewModel;

            if (file != null)
                MaintainCollections(file);
        }

        private void MaintainCollections(LibraryImporterFileTreeViewModel file)
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

                if (this.SelectionChanged != null)
                    this.SelectionChanged();
            }
            else if (!file.IsSelected && _selectedFiles.ContainsKey(file.FullPath))
            {
                _selectedFiles.Remove(file.FullPath);

                if (this.SelectionChanged != null)
                    this.SelectionChanged();
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
        public int Count { get { return _files.Count; } }
        public bool IsReadOnly { get { return false; } }

        public void Add(LibraryImporterFileTreeViewModel item)
        {
            AddUpdate(item);
        }

        public void Clear()
        {
            _files.Clear();
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

            if (this.SelectionChanged != null)
                this.SelectionChanged();

            OnPropertyChanged("Count");
        }

        public bool Contains(LibraryImporterFileTreeViewModel item)
        {
            return _files.ContainsKey(item.FullPath);
        }

        public void CopyTo(LibraryImporterFileTreeViewModel[] array, int arrayIndex)
        {
            _fileList.CopyTo(array, arrayIndex);
        }
        public int IndexOf(LibraryImporterFileTreeViewModel item)
        {
            return _fileList.IndexOf(item);
        }

        public void Insert(int index, LibraryImporterFileTreeViewModel item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(LibraryImporterFileTreeViewModel file)
        {
            _files.Remove(file.FullPath);
            _fileList.Remove(file);

            // IsSelected
            if (_selectedFiles.ContainsKey(file.FullPath))
            {
                _selectedFiles.Remove(file.FullPath);

                if (this.SelectionChanged != null)
                    this.SelectionChanged();
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
        IEnumerator<LibraryImporterFileTreeViewModel> IEnumerable<LibraryImporterFileTreeViewModel>.GetEnumerator()
        {
            return _fileList.GetEnumerator();
        }
        public IEnumerator GetEnumerator()
        {
            return _files.GetEnumerator();
        }
        #endregion
    }
}
