using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model.Vendor.ATLExtension;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.Vendor.AcoustIDViewModel;
using AudioStation.ViewModels.Vendor.MusicBrainzViewModel;

using IF.Lastfm.Core.Api.Enums;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application;

using IRelease = MetaBrainz.MusicBrainz.Interfaces.Entities.IRelease;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    public class LibraryLoaderImportFileViewModel : ViewModelBase
    {
        private readonly IModelValidationService _modelValidationService;
        private readonly IModelFileService _modelFileService;
        private readonly ILibraryImporter _libraryImporter;
        private readonly ITagCacheController _tagCacheController;

        public event SimpleEventHandler<LibraryLoaderImportFileViewModel> SelectMusicBrainzEvent;
        public event SimpleEventHandler<LibraryLoaderImportFileViewModel> SelectAcoustIDEvent;
        public event SimpleEventHandler<LibraryLoaderImportFileViewModel> PlayAudioEvent;

        bool _isSelected;
        bool _isExpanded;
        bool _inError;
        bool _isTagDirty;

        string _fileName;
        string _fileFullPath;
        string _fileMigrationName;
        string _fileMigrationFullPath;

        // Data available for the import (either cached here or in the database)
        bool _minimumImportValid;
        string _tagIssues;

        // UI-only property (set in the constructor)
        string _importOptions;

        AudioStationTag _tagClean;
        AudioStationTag _tagDirty;

        LibraryLoaderImportOutputViewModel _importOutput;
        LibraryLoaderImportLoadViewModel _importLoad;

        LookupResultViewModel _selectedAcoustIDResult;
        MusicBrainzRecordingViewModel _selectedMusicBrainzRecordingMatch;

        SimpleCommand _selectMusicBrainzCommand;
        SimpleCommand _selectAcoustIDCommand;
        SimpleCommand _playAudioCommand;
        SimpleCommand _saveTagCommand;
        SimpleCommand _copyMusicBrainzToTagCommand;
        SimpleCommand _refreshCommand;

        #region (public) Calculated UI Properties
        public string FinalImportDetail
        {
            get 
            {
                var format = "Artist ({0}) Album ({1}) Title ({2}) Genre ({3}) Track ({4} of {5}) Disc ({6} of {7})";
                var result = string.Format(format, _tagDirty.AlbumArtist, _tagDirty.Album,
                                                   _tagDirty.Title, _tagDirty.Genre,
                                                   _tagDirty.Track, _tagDirty.TrackTotal,
                                                   _tagDirty.DiscNumber, _tagDirty.DiscTotal);

                return result;
            }
        }
        public string TagDetail
        {
            get
            {
                var format = "Artist ({0}) Album ({1}) Title ({2}) Genre ({3}) Track ({4} of {5}) Disc ({6} of {7})";
                var result = string.Format(format, _tagClean.AlbumArtist, _tagClean.Album,
                                                   _tagClean.Title, _tagClean.Genre,
                                                   _tagClean.Track, _tagClean.TrackTotal,
                                                   _tagClean.DiscNumber, _tagClean.DiscTotal);

                return result;
            }
        }
        public string ImportOptions
        {
            get { return _importOptions; }
            set { this.RaiseAndSetIfChanged(ref _importOptions, value); }
        }
        #endregion

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
        public string FileName
        {
            get { return _fileName; }
            set { this.RaiseAndSetIfChanged(ref _fileName, value); }
        }
        public string FileFullPath
        {
            get { return _fileFullPath; }
            set { this.RaiseAndSetIfChanged(ref _fileFullPath, value); }
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
        public string TagIssues
        {
            get { return _tagIssues; }
            set { this.RaiseAndSetIfChanged(ref _tagIssues, value); }
        }
        public LibraryLoaderImportOutputViewModel ImportOutput
        {
            get { return _importOutput; }
            set { this.RaiseAndSetIfChanged(ref _importOutput, value); }
        }
        public LibraryLoaderImportLoadViewModel ImportLoad
        {
            get { return _importLoad; }
            set { this.RaiseAndSetIfChanged(ref _importLoad, value); }
        }

        public LookupResultViewModel SelectedAcoustIDResult
        {
            get { return _selectedAcoustIDResult; }
            set { this.RaiseAndSetIfChanged(ref _selectedAcoustIDResult, value); Update(); }
        }
        public MusicBrainzRecordingViewModel SelectedMusicBrainzRecordingMatch
        {
            get { return _selectedMusicBrainzRecordingMatch; }
            set { this.RaiseAndSetIfChanged(ref _selectedMusicBrainzRecordingMatch, value); Update(); }
        }

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

        public LibraryLoaderImportFileViewModel(string fileName,
                                                string destinationDirectory,
                                                LibraryLoaderImportOptionsViewModel options)
        {
            try
            {
                _modelValidationService = IocContainer.Get<IModelValidationService>();
                _modelFileService = IocContainer.Get<IModelFileService>();
                _libraryImporter = IocContainer.Get<ILibraryImporter>();
                _tagCacheController = IocContainer.Get<ITagCacheController>();

                _updating = false;

                this.FileFullPath = fileName;
                this.FileName = Path.GetFileName(fileName);
                this.ImportLoad = new LibraryLoaderImportLoadViewModel()
                {
                    DestinationFolder = destinationDirectory,
                    GroupingType = options.GroupingType,
                    IdentifyUsingAcoustID = options.IdentifyUsingAcoustID,
                    ImportFileMigration = options.ImportFileMigration,
                    IncludeMusicBrainzDetail = options.IncludeMusicBrainzDetail,
                    MigrationDeleteSourceFiles = options.MigrationDeleteSourceFiles,
                    MigrationDeleteSourceFolders = options.MigrationDeleteSourceFolders,
                    MigrationOverwriteDestinationFiles = options.MigrationOverwriteDestinationFiles,
                    NamingType = options.NamingType,
                    SourceFolder = options.SourceFolder,
                    SourceFile = fileName
                };
                this.ImportOptions = CreateOptionsUI(options);

                this.SelectAcoustIDCommand = new SimpleCommand(() =>
                {
                    if (this.SelectAcoustIDEvent != null)
                        this.SelectAcoustIDEvent(this);

                }, () => this.ImportOutput.AcoustIDSuccess);

                this.SelectMusicBrainzCommand = new SimpleCommand(() =>
                {
                    if (this.SelectMusicBrainzEvent != null)
                        this.SelectMusicBrainzEvent(this);

                }, () => this.ImportOutput.MusicBrainzRecordingMatchSuccess);

                this.PlayAudioCommand = new SimpleCommand(() =>
                {
                    if (this.PlayAudioEvent != null)
                        this.PlayAudioEvent(this);
                });

                this.CopyMusicBrainzToTagCommand = new SimpleCommand(() =>
                {
                    CopyMusicBrainzToTag();

                }, () => this.ImportOutput.MusicBrainzRecordingMatchSuccess);

                this.SaveTagCommand = new SimpleCommand(() =>
                {
                    Save();

                });

                this.RefreshCommand = new SimpleCommand(() =>
                {
                    Reload();
                });

                // Initializes the import output
                Reload();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading tag file:  {0}, {1}", LogMessageType.LibraryLoader, LogLevel.Error, ex, fileName, ex.Message);
                this.InError = true;
            }
        }

        private string CreateOptionsUI(LibraryLoaderImportOptionsViewModel options)
        {
            var result = string.Empty;

            result += options.ImportAsType.ToString();
            result += " / " + options.NamingType.ToString();
            result += " / " + options.GroupingType.ToString();

            if (options.IdentifyUsingAcoustID && options.IncludeMusicBrainzDetail)
                result += " / Use Acoustic Fingerprinting";

            if (options.ImportFileMigration)
            {
                if (options.MigrationDeleteSourceFiles)
                    result += " / Delete Source File(s)";

                if (options.MigrationDeleteSourceFolders)
                    result += " / Delete Source Folder(s)";

                if (options.MigrationOverwriteDestinationFiles)
                    result += " / Overwrite Destination";
            }

            return result;
        }

        /// <summary>
        /// Updates properties associated with the migration:  Tag Issues, Minimum Import Status
        /// </summary>
        public void Update()
        {
            if (_updating)
                return;

            _updating = true;

            var validationMessage = string.Empty;

            // Validate Tag (also gives validation message)
            _modelValidationService.ValidateTagImport(_tagDirty, out validationMessage);

            this.TagIssues = validationMessage == string.Empty ? "(None - Save Tag (Disk))" : validationMessage;

            this.MinimumImportValid = !this.InError && _libraryImporter.CanImportEntity(this.ImportLoad, this.ImportOutput);

            if (this.MinimumImportValid)
            {
                var fileMigrationName = _modelFileService.CalculateFileName(_tagDirty, this.ImportLoad.NamingType);
                var fileMigrationFolder = _modelFileService.CalculateFolderPath(_tagDirty, this.ImportLoad.DestinationFolder, this.ImportLoad.GroupingType);

                this.FileMigrationName = fileMigrationName;
                this.FileMigrationFullPath = Path.Combine(fileMigrationFolder, fileMigrationName);

                this.TagIssues = "(None)";
            }

            // Commands update
            this.SelectAcoustIDCommand.RaiseCanExecuteChanged();
            this.SelectMusicBrainzCommand.RaiseCanExecuteChanged();
            this.CopyMusicBrainzToTagCommand.RaiseCanExecuteChanged();
            this.PlayAudioCommand.RaiseCanExecuteChanged();
            this.RefreshCommand.RaiseCanExecuteChanged();
            this.SaveTagCommand.RaiseCanExecuteChanged();

            // Tag Dirty Flag
            this.IsTagDirty = !ApplicationHelpers.Compare(_tagClean, _tagDirty);

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
                _tagCacheController.SetData(this.FileFullPath, _tagDirty, true);

                // Update Clean Tag
                _tagClean = _tagCacheController.GetCopy(this.FileFullPath);
                _tagDirty = _tagCacheController.GetCopy(this.FileFullPath);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving import tag:  {0}", LogMessageType.General, LogLevel.Error, ex, this.FileFullPath);
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
                _tagClean = _tagCacheController.GetCopy(this.FileFullPath);
                _tagDirty = _tagCacheController.GetCopy(this.FileFullPath);

                // Unhook Events
                if (this.ImportOutput != null)
                    this.ImportOutput.PropertyChanged -= this.ImportOutput_PropertyChanged;

                // Reload Working Data
                this.ImportOutput = new LibraryLoaderImportOutputViewModel();

                // Unload Selected Data
                this.SelectedMusicBrainzRecordingMatch = null;
                this.SelectedAcoustIDResult = null;

                // Hook Events
                this.ImportOutput.PropertyChanged += ImportOutput_PropertyChanged;

                // Reset Error Flag
                this.InError = false;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error refreshing import tag:  {0}", LogMessageType.General, LogLevel.Error, ex, this.FileFullPath);
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
            return ApplicationHelpers.Map<AudioStationTag, AudioStationTag>(_tagDirty);
        }

        /// <summary>
        /// Saves new tag data to the current dirty tag (in memory only)
        /// </summary>
        public void SaveTagEdit(IAudioStationTag tagEdit)
        {
            // Sets calculated fields for the tag
            tagEdit.ToATL();

            ApplicationHelpers.MapOnto(tagEdit, _tagDirty);

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
            _tagDirty.Album = GetAlbum();
            _tagDirty.AlbumArtist = GetAlbumArtist();
            _tagDirty.Title = GetTrackTitle();
            _tagDirty.Genre = GetGenre();
            _tagDirty.Track = (uint)GetTrackNumber();
            _tagDirty.TrackTotal = (ushort)GetTrackCount();
            _tagDirty.DiscNumber = (ushort)GetDisc();
            _tagDirty.DiscTotal = (ushort)GetDiscCount();

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

        #region (private) Tag Data (resolved from AcoustID + Music Brainz + Tag)
        public string GetAlbumArtist()
        {
            var result = string.Empty;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                result = this.SelectedMusicBrainzRecordingMatch
                             .ArtistCredit?
                             .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                             .FirstOrDefault()?.Name ?? string.Empty;
            }

            return result;
        }
        public string GetAlbum()
        {
            var result = string.Empty;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                var release = GetRelease();

                if (release != null)
                {
                    result = release.Title ?? string.Empty;
                }
            }

            return result;
        }
        public string GetTrackTitle()
        {
            var result = string.Empty;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                result = this.SelectedMusicBrainzRecordingMatch.Title ?? string.Empty;
            }

            return result;
        }
        public string GetGenre()
        {
            var result = string.Empty;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                result = this.SelectedMusicBrainzRecordingMatch
                             .Genres?
                             .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                             .FirstOrDefault()?.Name ?? string.Empty;
            }

            return result;
        }
        public int GetDisc()
        {
            int result = _tagDirty.DiscNumber;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                var release = GetRelease();
                var medium = GetReleaseMedium();

                if (medium != null && release != null)
                {
                    result = (int)(release.Media?.IndexOf(medium) + 1);
                }
            }

            return result;
        }
        public int GetDiscCount()
        {
            int result = _tagDirty.DiscTotal;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                var release = GetRelease();

                if (release != null && release.Media != null)
                {
                    result = (int)release.Media.Count;
                }
            }
            
            return result;
        }
        public uint GetTrackNumber()
        {
            uint result = _tagDirty.Track;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                var medium = GetReleaseMedium();

                if (medium != null)
                {
                    result = (uint)medium.Tracks.First(x => x.Title == GetTrackTitle()).Position;
                }
            }

            return result;
        }
        public uint GetTrackCount()
        {
            uint result = _tagDirty.TrackTotal;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                var medium = GetReleaseMedium();

                if (medium != null)
                {
                    result = (uint)medium.TrackCount;
                }
            }

            return result;
        }
        private IRelease? GetRelease()
        {
            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                // ISSUES! Can't match release here against the tag! Since, we're trying to pull from music brainz.
                return this.SelectedMusicBrainzRecordingMatch?.Releases?.FirstOrDefault();
            }

            return null;
        }
        private IMedium? GetReleaseMedium()
        {
            var release = GetRelease();
            var trackTitle = GetTrackTitle();

            if (release != null)
            {
                return release.Media?.FirstOrDefault(x => x.Tracks != null && x.Tracks.Any(z => z.Title == trackTitle));
            }

            return null;
        }
        #endregion
    }
}
