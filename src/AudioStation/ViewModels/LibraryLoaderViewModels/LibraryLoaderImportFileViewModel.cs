using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

using AudioStation.Core.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;
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
        private readonly IModelFileService _modelFileService;

        public event SimpleEventHandler<LibraryLoaderImportFileViewModel> ImportBasicEvent;
        public event SimpleEventHandler<LibraryLoaderImportFileViewModel> SaveTagEvent;

        bool _isSelected;
        bool _isExpanded;

        string _fileName;
        string _fileFullPath;
        string _fileMigrationName;
        string _fileMigrationFullPath;
        string _destinationDirectory;

        // Data available for the import (either cached here or in the database)
        bool _minimumImportValid;
        string _tagIssues;

        LibraryEntryType _importAsType;
        LibraryEntryNamingType _namingType;
        LibraryEntryGroupingType _groupingType;

        TagFileViewModel _tagFile;
        LibraryLoaderImportOutputViewModel _importOutput;

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
        public TagFileViewModel TagFile
        {
            get { return _tagFile; }
            set { this.RaiseAndSetIfChanged(ref _tagFile, value); }
        }
        public LibraryLoaderImportOutputViewModel ImportOutput
        {
            get { return _importOutput; }
            set { this.RaiseAndSetIfChanged(ref _importOutput, value); }
        }
        public LibraryEntryType ImportAsType
        {
            get { return _importAsType; }
            set { this.RaiseAndSetIfChanged(ref _importAsType, value); }
        }
        public LibraryEntryNamingType NamingType
        {
            get { return _namingType; }
            set { this.RaiseAndSetIfChanged(ref _namingType, value); }
        }
        public LibraryEntryGroupingType GroupingType
        {
            get { return _groupingType; }
            set { this.RaiseAndSetIfChanged(ref _groupingType, value); }
        }
        public string DestinationDirectory
        {
            get { return _destinationDirectory; }
            set { this.RaiseAndSetIfChanged(ref _destinationDirectory, value); }
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
                                                LibraryEntryType importAsType,
                                                LibraryEntryNamingType namingType, 
                                                LibraryEntryGroupingType groupingType)
        {
            try
            {
                _modelFileService = IocContainer.Get<IModelFileService>();

                var tagCacheController = IocContainer.Get<ITagCacheController>();

                this.FileFullPath = fileName;
                this.FileName = Path.GetFileName(fileName);
                this.ImportAsType = importAsType;
                this.NamingType = namingType;
                this.GroupingType = groupingType;
                this.DestinationDirectory = destinationDirectory;
                this.ImportOutput = new LibraryLoaderImportOutputViewModel();
                this.ImportOutput.PropertyChanged += ImportOutput_PropertyChanged;

                var file = tagCacheController.Get(fileName);

                if (file != null)
                {
                    this.TagFile = new TagFileViewModel(file);
                    this.TagFile.PropertyChanged += (sender, e) =>
                    {
                        UpdateMigrationDetails();
                    };

                    UpdateMigrationDetails();
                }
                else
                {
                    ApplicationHelpers.Log("Error loading tag file:  {0}", LogMessageType.LibraryLoader, LogLevel.Error, null, fileName);
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading tag file:  {0}, {1}", LogMessageType.LibraryLoader, LogLevel.Error, ex, fileName, ex.Message);
            }
        }

        public void UpdateMigrationDetails()
        {
            this.TagIssues = string.Empty;

            if (string.IsNullOrWhiteSpace(GetPrimaryAlbumArtist()))
                this.TagIssues += "(Album Artist)";

            if (string.IsNullOrWhiteSpace(GetAlbum()))
                this.TagIssues += " (Album)";

            if (string.IsNullOrWhiteSpace(GetTrackTitle()))
                this.TagIssues += " (Title)";

            if (string.IsNullOrWhiteSpace(GetGenre()))
                this.TagIssues += " (Genre)";

            if (GetDisc() <= 0)
                this.TagIssues += " (Disc)";

            if (GetDiscCount() <= 0)
                this.TagIssues += " (Disc Count)";

            if (GetTrackNumber() <= 0)
                this.TagIssues += " (Track)";

            this.MinimumImportValid = string.IsNullOrEmpty(this.TagIssues);

            if (this.MinimumImportValid)
            {
                var fileMigrationName = _modelFileService.CalculateFileName(this, this.NamingType);
                var fileMigrationFolder = _modelFileService.CalculateFolderPath(this, this.DestinationDirectory, this.GroupingType);

                this.FileMigrationName = fileMigrationName;
                this.FileMigrationFullPath = Path.Combine(fileMigrationName, fileMigrationFolder);

                this.TagIssues = "(None)";
            }
        }

        private void ImportOutput_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateMigrationDetails();
        }

        #region (private) Tag Data (resolved from AcoustID + Music Brainz + Tag
        public string GetPrimaryAlbumArtist()
        {
            var result = string.Empty;

            if (this.ImportOutput.SelectedMusicBrainzRecordingMatch != null)
            {
                result = this.ImportOutput.SelectedMusicBrainzRecordingMatch.ArtistCredit?.FirstOrDefault()?.Name ?? string.Empty;
            }

            if (result == string.Empty)
            {
                result = this.TagFile.Tag.FirstAlbumArtist ?? string.Empty;
            }

            return result;
        }
        public string GetAlbum()
        {
            var result = string.Empty;

            if (this.ImportOutput.SelectedMusicBrainzRecordingMatch != null)
            {
                var release = GetRelease();

                if (release != null)
                {
                    result = release.Title ?? string.Empty;
                }
            }

            if (result == string.Empty)
            {
                result = this.TagFile.Tag.Album ?? string.Empty;
            }

            return result;
        }        
        public string GetTrackTitle()
        {
            var result = string.Empty;

            if (this.ImportOutput.SelectedMusicBrainzRecordingMatch != null)
            {
                result = this.ImportOutput.SelectedMusicBrainzRecordingMatch.Title ?? string.Empty;
            }

            if (result == string.Empty)
            {
                result = this.TagFile.Tag.Title ?? string.Empty;
            }

            return result;
        }
        public string GetGenre()
        {
            var result = string.Empty;

            if (this.ImportOutput.SelectedMusicBrainzRecordingMatch != null)
            {
                result = this.ImportOutput.SelectedMusicBrainzRecordingMatch.Genres?.FirstOrDefault()?.Name ?? string.Empty;
            }

            if (result == string.Empty)
            {
                result = this.TagFile.Tag.FirstGenre ?? string.Empty;
            }

            return result;
        }
        public uint GetDisc()
        {
            uint result = 0;

            if (this.ImportOutput.SelectedMusicBrainzRecordingMatch != null)
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
                result = this.TagFile.Tag.Disc;
            }

            return result;
        }
        public uint GetDiscCount()
        {
            uint result = 0;

            if (this.ImportOutput.SelectedMusicBrainzRecordingMatch != null)
            {
                var release = GetRelease();

                if (release != null && release.Media != null)
                {
                    result = (uint)release.Media.Count;
                }
            }

            if (result == 0)
            {
                result = this.TagFile.Tag.DiscCount;
            }

            return result;
        }
        public uint GetTrackNumber()
        {
            uint result = 0;

            if (this.ImportOutput.SelectedMusicBrainzRecordingMatch != null)
            {
                var medium = GetReleaseMedium();

                if (medium != null)
                {
                    result = (uint)medium.Tracks.First(x => x.Title == GetTrackTitle()).Position;
                }
            }

            if (result == 0)
            {
                result = this.TagFile.Tag.Track;
            }

            return result;
        }
        public uint GetTrackCount()
        {
            uint result = 0;

            if (this.ImportOutput.SelectedMusicBrainzRecordingMatch != null)
            {
                var medium = GetReleaseMedium();

                if (medium != null)
                {
                    result = (uint)medium.Tracks.Count;
                }
            }

            if (result == 0)
            {
                result = this.TagFile.Tag.TrackCount;
            }

            return result;
        }
        private IRelease? GetRelease()
        {
            if (this.ImportOutput.SelectedMusicBrainzRecordingMatch != null)
            {
                var album = this.TagFile.Tag.Album ?? string.Empty;

                if (album == string.Empty) 
                {
                    return this.ImportOutput.SelectedMusicBrainzRecordingMatch?.Releases?.FirstOrDefault();
                }
                else
                {
                    return this.ImportOutput.SelectedMusicBrainzRecordingMatch?.Releases?.FirstOrDefault(x => x.Title == album);
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
