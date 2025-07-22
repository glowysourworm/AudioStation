using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Threading;

using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.Vendor.AcoustIDViewModel;
using AudioStation.ViewModels.Vendor.MusicBrainzViewModel;
using AudioStation.ViewModels.Vendor.TagLibViewModel;

using MetaBrainz.MusicBrainz.CoverArt.Interfaces;
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
    public class LibraryLoaderImportFileViewModel : ViewModelBase, ISimpleTag
    {
        private readonly IModelValidationService _modelValidationService;
        private readonly IModelFileService _modelFileService;
        private readonly ILibraryImporter _libraryImporter;

        public event SimpleEventHandler<LibraryLoaderImportFileViewModel> SelectMusicBrainzEvent;
        public event SimpleEventHandler<LibraryLoaderImportFileViewModel> SelectAcoustIDEvent;

        bool _isSelected;
        bool _isExpanded;

        string _fileName;
        string _fileFullPath;
        string _fileMigrationName;
        string _fileMigrationFullPath;

        // Data available for the import (either cached here or in the database)
        bool _minimumImportValid;
        string _tagIssues;

        LibraryLoaderImportOutputViewModel _importOutput;
        LibraryLoaderImportLoadViewModel _importLoad;

        LookupResultViewModel _selectedAcoustIDResult;
        MusicBrainzRecordingViewModel _selectedMusicBrainzRecordingMatch;

        SimpleCommand _selectMusicBrainzCommand;
        SimpleCommand _selectAcoustIDCommand;

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
            set { this.RaiseAndSetIfChanged(ref _selectedAcoustIDResult, value); }
        }
        public MusicBrainzRecordingViewModel SelectedMusicBrainzRecordingMatch
        {
            get { return _selectedMusicBrainzRecordingMatch; }
            set { this.RaiseAndSetIfChanged(ref _selectedMusicBrainzRecordingMatch, value); }
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

        #region ISimpleTag
        public string Album
        {
            get { return GetAlbum(); }
            set { }
        }
        public string FirstAlbumArtist
        {
            get { return GetPrimaryAlbumArtist(); }
            set { }
        }
        public string Title 
        { 
            get { return GetTrackTitle(); }
            set { } 
        }
        public string FirstGenre 
        { 
            get { return GetGenre(); }
            set { }
        }
        public uint Track
        {
            get { return GetTrackNumber(); }
            set { }
        }
        public uint TrackCount
        {
            get { return GetTrackCount(); }
            set { }
        }
        public uint Disc
        {
            get { return GetDisc(); }
            set { }
        }
        public uint DiscCount
        {
            get { return GetDiscCount(); }
            set { }
        }
        #endregion

        public LibraryLoaderImportFileViewModel(string fileName, 
                                                string destinationDirectory, 
                                                LibraryLoaderImportOptionsViewModel options)
        {
            try
            {
                _modelValidationService = IocContainer.Get<IModelValidationService>();
                _modelFileService = IocContainer.Get<IModelFileService>();
                _libraryImporter = IocContainer.Get<ILibraryImporter>();

                var tagCacheController = IocContainer.Get<ITagCacheController>();

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
                this.ImportOutput = new LibraryLoaderImportOutputViewModel()
                {
                    ImportedTagFile = tagCacheController.Get(fileName)
                };
                this.ImportOutput.PropertyChanged += ImportOutput_PropertyChanged;

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
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading tag file:  {0}, {1}", LogMessageType.LibraryLoader, LogLevel.Error, ex, fileName, ex.Message);
            }
        }

        public void UpdateMigrationDetails()
        {
            var validationMessage = string.Empty;

            // Validate Tag (also gives validation message)
            _modelValidationService.ValidateTagImport(this, out validationMessage);

            this.TagIssues = validationMessage;
            this.MinimumImportValid = _libraryImporter.CanImportEntity(this.ImportLoad, this.ImportOutput);

            if (this.MinimumImportValid)
            {
                var fileMigrationName = _modelFileService.CalculateFileName(this, this.ImportLoad.NamingType);
                var fileMigrationFolder = _modelFileService.CalculateFolderPath(this, this.ImportLoad.DestinationFolder, this.ImportLoad.GroupingType);

                this.FileMigrationName = fileMigrationName;
                this.FileMigrationFullPath = Path.Combine(fileMigrationName, fileMigrationFolder);

                this.TagIssues = "(None)";
            }
        }

        private void ImportOutput_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateMigrationDetails();

            this.SelectAcoustIDCommand.RaiseCanExecuteChanged();
            this.SelectMusicBrainzCommand.RaiseCanExecuteChanged();
        }

        #region (private) Tag Data (resolved from AcoustID + Music Brainz + Tag
        public string GetPrimaryAlbumArtist()
        {
            var result = string.Empty;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                result = this.SelectedMusicBrainzRecordingMatch.ArtistCredit?.FirstOrDefault()?.Name ?? string.Empty;
            }

            if (result == string.Empty)
            {
                result = this.ImportOutput.ImportedTagFile.Tag.FirstAlbumArtist ?? string.Empty;
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
                result = this.ImportOutput.ImportedTagFile.Tag.Album ?? string.Empty;
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
                result = this.ImportOutput.ImportedTagFile.Tag.Title ?? string.Empty;
            }

            return result;
        }
        public string GetGenre()
        {
            var result = string.Empty;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                result = this.SelectedMusicBrainzRecordingMatch.Genres?.FirstOrDefault()?.Name ?? string.Empty;
            }

            if (result == string.Empty)
            {
                result = this.ImportOutput.ImportedTagFile.Tag.FirstGenre ?? string.Empty;
            }

            return result;
        }
        public uint GetDisc()
        {
            uint result = 0;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                var release = GetRelease();
                var medium = GetReleaseMedium();

                if (medium != null && release != null)
                {
                    result = (uint)(release.Media?.IndexOf(medium) + 1);
                }
            }

            if (result == 0)
            {
                result = this.ImportOutput.ImportedTagFile.Tag.Disc;
            }

            return result;
        }
        public uint GetDiscCount()
        {
            uint result = 0;

            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                var release = GetRelease();

                if (release != null && release.Media != null)
                {
                    result = (uint)release.Media.Count;
                }
            }

            if (result == 0)
            {
                result = this.ImportOutput.ImportedTagFile.Tag.DiscCount;
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
                result = this.ImportOutput.ImportedTagFile.Tag.Track;
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
                result = this.ImportOutput.ImportedTagFile.Tag.TrackCount;
            }

            return result;
        }
        private IRelease? GetRelease()
        {
            if (this.SelectedMusicBrainzRecordingMatch != null)
            {
                var album = this.ImportOutput.ImportedTagFile.Tag.Album ?? string.Empty;

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
