using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

using SimpleWpf.Extensions.Event;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels
{
    /// <summary>
    /// Collection container for the staged files. Maintains separate collections for: IsSelected; 
    /// IsValid; (not) IsValid; Has Music Brianz Result(s); has (special) Music Brainz Tag Result; 
    /// has AcoustID Result; etc...
    /// </summary>
    public class LibraryImporterStagedFileCollection : ViewModelBase, IEnumerable, IList<LibraryImporterFileViewModel>, INotifyCollectionChanged
    {
        private List<LibraryImporterFileViewModel> _fileList;
        private KeyedObservableCollection<string, LibraryImporterFileViewModel> _files;
        private KeyedObservableCollection<string, LibraryImporterFileViewModel> _selectedFiles;
        private KeyedObservableCollection<string, LibraryImporterFileViewModel> _validFiles;
        private KeyedObservableCollection<string, LibraryImporterFileViewModel> _invalidFiles;
        private KeyedObservableCollection<string, LibraryImporterFileViewModel> _acoustIDFiles;
        private KeyedObservableCollection<string, LibraryImporterFileViewModel> _musicBrainzFiles;
        private KeyedObservableCollection<string, LibraryImporterFileViewModel> _musicBrainzSpecialTagFiles;
        private KeyedObservableCollection<string, LibraryImporterFileViewModel> _libraryConflictFiles;

        public event CollectionItemChangedHandler<LibraryImporterFileViewModel> ItemPropertyChanged
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
            _files = new KeyedObservableCollection<string, LibraryImporterFileViewModel>();
            _fileList = new List<LibraryImporterFileViewModel>();
            _selectedFiles = new KeyedObservableCollection<string, LibraryImporterFileViewModel>();
            _validFiles = new KeyedObservableCollection<string, LibraryImporterFileViewModel>();
            _invalidFiles = new KeyedObservableCollection<string, LibraryImporterFileViewModel>();
            _acoustIDFiles = new KeyedObservableCollection<string, LibraryImporterFileViewModel>();
            _musicBrainzFiles = new KeyedObservableCollection<string, LibraryImporterFileViewModel>();
            _musicBrainzSpecialTagFiles = new KeyedObservableCollection<string, LibraryImporterFileViewModel>();
            _libraryConflictFiles = new KeyedObservableCollection<string, LibraryImporterFileViewModel>();
        }
        public LibraryImporterStagedFileCollection(IEnumerable<LibraryImporterFileViewModel> stagedFiles)
            : this()
        {
            foreach (var file in stagedFiles)
            {
                AddUpdate(file);
            }
        }

        public LibraryImporterFileViewModel this[int index]
        {
            get { return _fileList[index]; }
            set { throw new NotSupportedException(); }
        }

        public IReadOnlyCollection<LibraryImporterFileViewModel> Files
        {
            get { return _files; }
        }
        public IReadOnlyCollection<LibraryImporterFileViewModel> SelectedFiles
        {
            get { return _selectedFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileViewModel> ValidFiles
        {
            get { return _validFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileViewModel> InvalidFiles
        {
            get { return _invalidFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileViewModel> AcoustIDFiles
        {
            get { return _acoustIDFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileViewModel> MusicBrainzFiles
        {
            get { return _musicBrainzFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileViewModel> MusicBrainzSpecialTagFiles
        {
            get { return _musicBrainzSpecialTagFiles; }
        }
        public IReadOnlyCollection<LibraryImporterFileViewModel> LibraryConflictFiles
        {
            get { return _libraryConflictFiles; }
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
        }

        private void AddUpdate(LibraryImporterFileViewModel file)
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
            var file = sender as LibraryImporterFileViewModel;

            if (file != null)
                MaintainCollections(file);
        }

        private void MaintainCollections(LibraryImporterFileViewModel file)
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
        }

        #region (public) IList
        public int Count { get { return _files.Count; } }
        public bool IsReadOnly { get { return false; } }

        public void Add(LibraryImporterFileViewModel item)
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

            if (this.SelectionChanged != null)
                this.SelectionChanged();

            OnPropertyChanged("Count");
        }

        public bool Contains(LibraryImporterFileViewModel item)
        {
            return _files.ContainsKey(item.FullPath);
        }

        public void CopyTo(LibraryImporterFileViewModel[] array, int arrayIndex)
        {
            _fileList.CopyTo(array, arrayIndex);
        }
        public int IndexOf(LibraryImporterFileViewModel item)
        {
            return _fileList.IndexOf(item);
        }

        public void Insert(int index, LibraryImporterFileViewModel item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(LibraryImporterFileViewModel file)
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

            OnPropertyChanged("Count");

            return true;
        }

        public void RemoveAt(int index)
        {
            var item = _fileList[index];

            Remove(item);
        }
        IEnumerator<LibraryImporterFileViewModel> IEnumerable<LibraryImporterFileViewModel>.GetEnumerator()
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
