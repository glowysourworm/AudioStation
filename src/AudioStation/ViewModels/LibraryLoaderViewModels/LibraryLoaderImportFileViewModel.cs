using System.ComponentModel;
using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model.Vendor.ATLExtension;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.Vendor.AcoustIDViewModel;
using AudioStation.ViewModels.Vendor.MusicBrainzViewModel;

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
        SimpleCommand _refreshCommand;

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
        public SimpleCommand RefreshCommand
        {
            get { return _refreshCommand; }
            set { this.RaiseAndSetIfChanged(ref _refreshCommand, value); }
        }

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

                // Initializes the import output
                Reload();

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

                this.SaveTagCommand = new SimpleCommand(() =>
                {
                    Save();
                });

                this.RefreshCommand = new SimpleCommand(() =>
                {
                    Reload();
                });
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading tag file:  {0}, {1}", LogMessageType.LibraryLoader, LogLevel.Error, ex, fileName, ex.Message);
                this.InError = true;
            }
        }

        /// <summary>
        /// Updates properties associated with the migration:  Tag Issues, Minimum Import Status
        /// </summary>
        public void Update()
        {
            var validationMessage = string.Empty;

            // Validate Tag (also gives validation message)
            _modelValidationService.ValidateTagImport(_tagDirty, out validationMessage);

            this.TagIssues = validationMessage;
            this.MinimumImportValid = !this.InError && _libraryImporter.CanImportEntity(this.ImportLoad, this.ImportOutput);

            if (this.MinimumImportValid)
            {
                var fileMigrationName = _modelFileService.CalculateFileName(_tagDirty, this.ImportLoad.NamingType);
                var fileMigrationFolder = _modelFileService.CalculateFolderPath(_tagDirty, this.ImportLoad.DestinationFolder, this.ImportLoad.GroupingType);

                this.FileMigrationName = fileMigrationName;
                this.FileMigrationFullPath = Path.Combine(fileMigrationName, fileMigrationFolder);

                this.TagIssues = "(None)";
            }

            UpdateDirtyTag();
        }

        /// <summary>
        /// Saves data to the (physical) import tag file, and refreshes migration detail
        /// </summary>
        /// <exception cref="Exception">Minimum import requirements have not been met</exception>
        public void Save()
        {
            if (!this.MinimumImportValid)
                throw new Exception("Trying to save migration detail to tag before it has been validated");

            try
            {
                // Save tag data to (source) file
                _tagCacheController.SetData(this.FileFullPath, _tagDirty, true);

                // Update Clean Tag
                _tagClean = _tagCacheController.GetCopy(this.FileFullPath);
                _tagDirty = _tagCacheController.Get(this.FileFullPath);             // Technically, this is the same reference as above

                this.IsTagDirty = false;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving import tag:  {0}", LogMessageType.General, LogLevel.Error, ex, this.FileFullPath);
                this.InError = true;
            }
        }

        /// <summary>
        /// Reloads tag data from import source file
        /// </summary>
        public void Reload()
        {
            // Piggy-backing code in the constructor (watch for nulls)
            //
            try
            {
                // Unhook Events
                if (this.ImportOutput != null)
                    this.ImportOutput.PropertyChanged -= this.ImportOutput_PropertyChanged;

                // Reload Working Data
                this.ImportOutput = new LibraryLoaderImportOutputViewModel();

                // Get clean copy of the tag
                _tagClean = _tagCacheController.GetCopy(this.FileFullPath);
                _tagDirty = _tagCacheController.Get(this.FileFullPath);

                // Hook Events
                this.ImportOutput.PropertyChanged += ImportOutput_PropertyChanged;

                // Reset Error Flag
                this.InError = false;
                this.IsTagDirty = false;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error refreshing import tag:  {0}", LogMessageType.General, LogLevel.Error, ex, this.FileFullPath);
                this.InError = true;
                return;
            }

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
            ApplicationHelpers.MapOnto(tagEdit, _tagDirty);

            this.IsTagDirty = _tagClean.GetHashCode() != _tagDirty.GetHashCode();
        }

        private void UpdateDirtyTag()
        {
            // Update our dirty copy of the tag
            //
            _tagDirty.Album = GetAlbum();
            _tagDirty.AlbumArtist = GetAlbumArtist();
            _tagDirty.Title = GetTrackTitle();
            _tagDirty.Genre = GetGenres();
            _tagDirty.Track = (uint)GetTrackNumber();
            _tagDirty.TrackTotal = (ushort)GetTrackCount();
            _tagDirty.DiscNumber = (ushort)GetDisc();
            _tagDirty.DiscTotal = (ushort)GetDiscCount();

            this.IsTagDirty = _tagClean.GetHashCode() != _tagDirty.GetHashCode();
        }

        private void ImportOutput_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Update();

            this.SelectAcoustIDCommand.RaiseCanExecuteChanged();
            this.SelectMusicBrainzCommand.RaiseCanExecuteChanged();
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

            if (result == string.Empty)
            {
                result = _tagDirty.AlbumArtist ?? string.Empty;
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

            if (result == string.Empty)
            {
                result = _tagDirty.Album ?? string.Empty;
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

            if (result == string.Empty)
            {
                result = _tagDirty.Title ?? string.Empty;
            }

            return result;
        }
        public string GetGenres()
        {
            var result = string.Empty;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                result = this.SelectedMusicBrainzRecordingMatch
                             .Genres?
                             .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                             .FirstOrDefault()?.Name ?? string.Empty;
            }

            if (result == string.Empty)
            {
                result = _tagDirty.Genre ?? string.Empty;
            }

            return result;
        }
        public int GetDisc()
        {
            int result = 0;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                var release = GetRelease();
                var medium = GetReleaseMedium();

                if (medium != null && release != null)
                {
                    result = (int)(release.Media?.IndexOf(medium) + 1);
                }
            }

            if (result == 0)
            {
                result = _tagDirty.DiscNumber;
            }

            return result;
        }
        public int GetDiscCount()
        {
            int result = 0;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                var release = GetRelease();

                if (release != null && release.Media != null)
                {
                    result = (int)release.Media.Count;
                }
            }

            if (result == 0)
            {
                result = _tagDirty.DiscTotal;
            }

            return result;
        }
        public uint GetTrackNumber()
        {
            uint result = 0;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                var medium = GetReleaseMedium();

                if (medium != null)
                {
                    result = (uint)medium.Tracks.First(x => x.Title == GetTrackTitle()).Position;
                }
            }

            if (result == 0)
            {
                result = _tagDirty.Track;
            }

            return result;
        }
        public uint GetTrackCount()
        {
            uint result = 0;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                var medium = GetReleaseMedium();

                if (medium != null)
                {
                    result = (uint)medium.Tracks.Count;
                }
            }

            if (result == 0)
            {
                result = _tagDirty.TrackTotal;
            }

            return result;
        }
        private IRelease? GetRelease()
        {
            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                var album = _tagDirty.Album ?? string.Empty;

                if (album == string.Empty)
                {
                    return this.SelectedMusicBrainzRecordingMatch?.Releases?.FirstOrDefault();
                }
                else
                {
                    return this.SelectedMusicBrainzRecordingMatch?.Releases?.FirstOrDefault(x => x.Title == album);
                }
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
