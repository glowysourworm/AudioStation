using System.ComponentModel;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Vendor.ATLExtension;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;
using AudioStation.Core.Utility;
using AudioStation.ViewModels.TagViewModels;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application;
using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel.FileTreeView;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels
{
    /// <summary>
    /// PathViewModel provides the node VALUE for the recursive directory structure. The "Path" view model is essentially
    /// the container for this value.
    /// </summary>
    public class LibraryImporterFileViewModel : FileTreeNodeViewModel
    {
        private readonly IAudioStationMapper _audioStationMapper;
        private readonly ITagCacheController _tagCacheController;

        public event SimpleEventHandler<LibraryImporterFileViewModel> SelectMusicBrainzEvent;
        public event SimpleEventHandler<LibraryImporterFileViewModel> SelectAcoustIDEvent;
        public event SimpleEventHandler<LibraryImporterFileViewModel> PlayAudioEvent;

        bool _inError;
        bool _isTagDirty;

        string _fileMigrationName;
        string _fileMigrationFullPath;

        // Data available for the import (either cached here or in the database)
        bool _minimumImportValid;
        TagSmallEditViewModel _tag;
        TagSmallViewModel _musicBrainzTag;

        AudioStationTag _tagClean;
        AudioStationTag _tagDirty;

        LibraryImportType _importType;

        //LibraryLoaderImportOutputViewModel _importOutput;
        //LibraryLoaderImportLoadViewModel _importLoad;

        //AcoustIDLookupResultViewModel _selectedAcoustIDResult;
        //TagSmallViewModel _selectedMusicBrainzRecordingMatch;

        SimpleCommand _selectMusicBrainzCommand;
        SimpleCommand _selectAcoustIDCommand;
        SimpleCommand _playAudioCommand;
        SimpleCommand _saveTagCommand;
        SimpleCommand _copyMusicBrainzToTagCommand;
        SimpleCommand _refreshCommand;

        public bool InError
        {
            get { return _inError; }
            set { this.RaiseAndSetIfChanged(ref _inError, value); }
        }
        public bool IsTagDirty
        {
            get { return _isTagDirty; }
            set { this.RaiseAndSetIfChanged(ref _isTagDirty, value); }
        }
        public string FileMigrationName
        {
            get { return _fileMigrationName; }
            set { this.RaiseAndSetIfChanged(ref _fileMigrationName, value); }
        }
        public string FileMigrationFullPath
        {
            get { return _fileMigrationFullPath; }
            set { this.RaiseAndSetIfChanged(ref _fileMigrationFullPath, value); }
        }
        public bool MinimumImportValid
        {
            get { return _minimumImportValid; }
            set { this.RaiseAndSetIfChanged(ref _minimumImportValid, value); }
        }
        public TagSmallEditViewModel Tag
        {
            get { return _tag; }
            set { this.RaiseAndSetIfChanged(ref _tag, value); }
        }
        public TagSmallViewModel MusicBrainzTag
        {
            get { return _musicBrainzTag; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzTag, value); }
        }
        public AudioStationTag TagClean
        {
            get { return _tagClean; }
            set { this.RaiseAndSetIfChanged(ref _tagClean, value); }
        }
        public AudioStationTag TagDirty
        {
            get { return _tagDirty; }
            set { this.RaiseAndSetIfChanged(ref _tagDirty, value); }
        }
        public LibraryImportType ImportType
        {
            get { return _importType; }
            set { this.RaiseAndSetIfChanged(ref _importType, value); }
        }
        //public LibraryLoaderImportOutputViewModel ImportOutput
        //{
        //    get { return _importOutput; }
        //    set { this.RaiseAndSetIfChanged(ref _importOutput, value); }
        //}
        //public LibraryLoaderImportLoadViewModel ImportLoad
        //{
        //    get { return _importLoad; }
        //    set { this.RaiseAndSetIfChanged(ref _importLoad, value); }
        //}
        //public AcoustIDLookupResultViewModel SelectedAcoustIDResult
        //{
        //    get { return _selectedAcoustIDResult; }
        //    set { this.RaiseAndSetIfChanged(ref _selectedAcoustIDResult, value); }
        //}
        //public TagSmallViewModel SelectedMusicBrainzRecordingMatch
        //{
        //    get { return _selectedMusicBrainzRecordingMatch; }
        //    set { this.RaiseAndSetIfChanged(ref _selectedMusicBrainzRecordingMatch, value); }
        //}
        public SimpleCommand SelectMusicBrainzCommand
        {
            get { return _selectMusicBrainzCommand; }
            set { this.RaiseAndSetIfChanged(ref _selectMusicBrainzCommand, value); }
        }
        public SimpleCommand SelectAcoustIDCommand
        {
            get { return _selectAcoustIDCommand; }
            set { this.RaiseAndSetIfChanged(ref _selectAcoustIDCommand, value); }
        }
        public SimpleCommand PlayAudioCommand
        {
            get { return _playAudioCommand; }
            set { this.RaiseAndSetIfChanged(ref _playAudioCommand, value); }
        }
        public SimpleCommand SaveTagCommand
        {
            get { return _saveTagCommand; }
            set { this.RaiseAndSetIfChanged(ref _saveTagCommand, value); }
        }
        public SimpleCommand CopyMusicBrainzToTagCommand
        {
            get { return _copyMusicBrainzToTagCommand; }
            set { this.RaiseAndSetIfChanged(ref _copyMusicBrainzToTagCommand, value); }
        }
        public SimpleCommand RefreshCommand
        {
            get { return _refreshCommand; }
            set { this.RaiseAndSetIfChanged(ref _refreshCommand, value); }
        }

        bool _updating;

        /// <summary>
        /// Constructor for an import file view model. This may represent either a file or a directory.
        /// </summary>
        public LibraryImporterFileViewModel(string fileFullPath,
                                            string fileBaseDirectory,                                       // This is the base path for the tree
                                            LibraryImportType importType)
            : base(fileBaseDirectory, fileFullPath, 0)
        {
            _tagCacheController = IocContainer.Get<ITagCacheController>();

            _updating = false;

            //this.ImportLoad = new LibraryLoaderImportLoadViewModel()
            //{
            //    DestinationFolder = destinationDirectory.Directory,
            //    GroupingType = destinationDirectory.GroupingType,
            //    IdentifyUsingAcoustID = options.IdentifyUsingAcoustID,
            //    ImportFileMigration = options.ImportType == Core.Model.LibraryImportType.Migration,
            //    IncludeMusicBrainzDetail = options.IdentifyUsingMusicBrainz,
            //    MigrationDeleteSourceFiles = options.MigrationDeleteSourceFiles,
            //    MigrationDeleteSourceFolders = options.MigrationDeleteSourceFolders,
            //    MigrationOverwriteDestinationFiles = options.MigrationOverwriteDestinationFiles,
            //    NamingType = destinationDirectory.NamingType,
            //    SourceFolder = sourceDirectory.Directory,
            //    SourceFile = fullPath
            //};
            this.Tag = new TagSmallEditViewModel();
            this.MusicBrainzTag = new TagSmallViewModel();
            this.ImportType = importType;

            this.SelectAcoustIDCommand = new SimpleCommand(() =>
            {
                if (this.SelectAcoustIDEvent != null)
                    this.SelectAcoustIDEvent(this);

            }/*,  () => this.ImportOutput.AcoustIDSuccess */);

            this.SelectMusicBrainzCommand = new SimpleCommand(() =>
            {
                if (this.SelectMusicBrainzEvent != null)
                    this.SelectMusicBrainzEvent(this);

            }/*, () => this.ImportOutput.MusicBrainzRecordingMatchSuccess*/);

            this.PlayAudioCommand = new SimpleCommand(() =>
            {
                if (this.PlayAudioEvent != null)
                    this.PlayAudioEvent(this);
            });

            this.CopyMusicBrainzToTagCommand = new SimpleCommand(() =>
            {
                CopyMusicBrainzToTag();

            }, () => this.ImportType == LibraryImportType.Migration);  /*, () => this.ImportOutput.MusicBrainzRecordingMatchSuccess*/

            this.SaveTagCommand = new SimpleCommand(() =>
            {
                Save();

            }, () => this.ImportType == LibraryImportType.Migration);

            this.RefreshCommand = new SimpleCommand(() =>
            {
                Reload();

            }, () => this.Tag.IsModified);

            // Initializes the import output
            Reload();
        }

        /// <summary>
        /// Updates properties associated with the migration:  Tag Issues, Minimum Import Status
        /// </summary>
        public void Update()
        {
            if (_updating)
                return;

            _updating = true;

            // Validate Tag (also gives validation message)
            //var validation = TagValidator.ValidateTagImport(_tagDirty);

            // Update (validation)
            //this.Tag.Update(_tagClean, _tagDirty, validation);

            //// Update (Music Brainz)
            //if (this.SelectedMusicBrainzRecordingMatch != null)
            //    _audioStationMapper.MapOnto(this.SelectedMusicBrainzRecordingMatch, this.MusicBrainzTag);

            //this.MinimumImportValid = !this.InError && _libraryImporter.CanImportEntity(this.ImportLoad, this.ImportOutput);

            if (this.MinimumImportValid)
            {
                //var fileMigrationName = _modelFileService.CalculateFileName(_tagDirty, this.ImportLoad.NamingType);
                //var fileMigrationFolder = _modelFileService.CalculateFolderPath(_tagDirty, this.ImportLoad.DestinationFolder, this.ImportLoad.GroupingType);

                //this.FileMigrationName = fileMigrationName;
                //this.FileMigrationFullPath = System.IO.Path.Combine(fileMigrationFolder, fileMigrationName);

                //this.TagIssues = "(None)";
            }

            // Commands update
            this.SelectAcoustIDCommand.RaiseCanExecuteChanged();
            this.SelectMusicBrainzCommand.RaiseCanExecuteChanged();
            this.CopyMusicBrainzToTagCommand.RaiseCanExecuteChanged();
            this.PlayAudioCommand.RaiseCanExecuteChanged();
            this.RefreshCommand.RaiseCanExecuteChanged();
            this.SaveTagCommand.RaiseCanExecuteChanged();

            // Tag Dirty Flag
            //this.IsTagDirty = !ApplicationHelpers.Compare(_tagClean, _tagDirty);

            // Update some UI properties
            OnPropertyChanged("FinalImportDetail");
            OnPropertyChanged("TagDetail");

            _updating = false;
        }

        /// <summary>
        /// Saves data to the (physical) import tag file, and refreshes migration detail
        /// </summary>
        /// <exception cref="Exception">Minimum import requirements have not been met</exception>
        public void Save()
        {
            try
            {
                // Save tag data to (source) file
                _tagCacheController.SetData(this.FullPath, _tagDirty, true);

                // Update Clean Tag
                _tagClean = _tagCacheController.GetCopy(this.FullPath);
                _tagDirty = _tagCacheController.GetCopy(this.FullPath);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving import tag:  {0}", LogLevel.Error, ex, this.FullPath);
                this.InError = true;
            }

            Update();
        }

        /// <summary>
        /// Reloads tag data from import source file
        /// </summary>
        public void Reload()
        {
            _updating = true;

            // Piggy-backing code in the constructor (watch for nulls)
            //
            try
            {
                // Get clean copy of the tag
                //_tagClean = _tagCacheController.GetCopy(this.FullPath);
                //_tagDirty = _tagCacheController.GetCopy(this.FullPath);

                //// Unhook Events
                //if (this.ImportOutput != null)
                //    this.ImportOutput.PropertyChanged -= ImportOutput_PropertyChanged;

                //// Reload Working Data
                //this.ImportOutput = new LibraryLoaderImportOutputViewModel();

                //// Unload Selected Data
                //this.SelectedMusicBrainzRecordingMatch = null;
                //this.SelectedAcoustIDResult = null;

                //// Hook Events
                //this.ImportOutput.PropertyChanged += ImportOutput_PropertyChanged;

                // Reset Error Flag
                this.InError = false;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error refreshing import tag:  {0}", LogLevel.Error, ex, this.FullPath);
                this.InError = true;
                _updating = false;
                return;
            }

            _updating = false;

            // Also, sets the updating flag
            //
            Update();
        }

        /// <summary>
        /// Gets current (dirty) tag. This has yet to be completed and saved as part of the migration.
        /// </summary>
        /// <returns></returns>
        public IAudioStationTag GetTagCopy()
        {
            return _audioStationMapper.Map<AudioStationTag, AudioStationTag>(_tagDirty);
        }

        /// <summary>
        /// Saves new tag data to the current dirty tag (in memory only)
        /// </summary>
        public void SaveTagEdit(IAudioStationTag tagEdit)
        {
            // Sets calculated fields for the tag
            tagEdit.ToATL();

            _audioStationMapper.MapOnto(tagEdit, _tagDirty);

            Update();
        }

        public void SaveTagFieldEdit(string fieldName, IAudioStationTag editTag)
        {
            switch (fieldName)
            {
                case "AlbumArtists":
                    _tagDirty.AlbumArtists = editTag.AlbumArtists;
                    break;
                case "Album":
                    if (!string.IsNullOrWhiteSpace(editTag.Album))
                    {
                        _tagDirty.Album = editTag.Album;
                    }
                    break;
                case "Genres":
                    _tagDirty.Genres = editTag.Genres;
                    break;
                case "TrackCount":
                    if (editTag.TrackTotal > 0)
                    {
                        _tagDirty.TrackTotal = editTag.TrackTotal;
                    }
                    break;
                case "DiscCount":
                    if (editTag.DiscTotal > 0)
                    {
                        _tagDirty.DiscTotal = editTag.DiscTotal;
                    }
                    break;
                case "Artwork":
                    _tagDirty.EmbeddedPictures = editTag.EmbeddedPictures;
                    break;
                default:
                    throw new Exception("Unhandled group tag edit field name:  LibraryLoaderImportViewModel.cs");
            }

            // Set ATL Fields
            _tagDirty.ToATL();

            Update();
        }

        private void CopyMusicBrainzToTag()
        {
            // Update our dirty copy of the tag
            //
            //_tagDirty.Album = this.SelectedMusicBrainzRecordingMatch.Album;
            //_tagDirty.AlbumArtist = this.SelectedMusicBrainzRecordingMatch.AlbumArtist;
            //_tagDirty.Title = this.SelectedMusicBrainzRecordingMatch.Title;
            //_tagDirty.Genre = this.SelectedMusicBrainzRecordingMatch.Genre;
            //_tagDirty.Track = (uint)this.SelectedMusicBrainzRecordingMatch.Track;
            //_tagDirty.TrackTotal = (ushort)this.SelectedMusicBrainzRecordingMatch.TrackTotal;
            //_tagDirty.DiscNumber = (ushort)this.SelectedMusicBrainzRecordingMatch.MediaNumber;
            //_tagDirty.DiscTotal = (ushort)this.SelectedMusicBrainzRecordingMatch.MediaTotal;

            // ATL FIELD UPDATES
            _tagDirty.TrackNumber = _tagDirty.Track.ToString();
            _tagDirty.AlbumArtists.Clear();
            _tagDirty.Genres.Clear();

            if (!string.IsNullOrEmpty(_tagDirty.AlbumArtist))
                _tagDirty.AlbumArtists.Add(_tagDirty.AlbumArtist);

            if (!string.IsNullOrEmpty(_tagDirty.Genre))
                _tagDirty.Genres.Add(_tagDirty.Genre);

            Update();
        }

        private void ImportOutput_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Update();
        }

        public override string ToString()
        {
            return this.FullPath;
        }
    }
}
