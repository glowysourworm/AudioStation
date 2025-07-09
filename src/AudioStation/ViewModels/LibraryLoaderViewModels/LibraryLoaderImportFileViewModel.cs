using System.Collections.ObjectModel;
using System.Formats.Tar;
using System.IO;
using System.Windows;

using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.Vendor.TagLibViewModel;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    public class LibraryLoaderImportFileViewModel : ViewModelBase
    {
        bool _isSelected;
        bool _isExpanded;

        string _fileName;
        string _fileFullPath;
        string _fileMigrationName;
        string _fileMigrationFullPath;
        string _destinationDirectory;
        ObservableCollection<string> _acoustIDTestResults;
        ObservableCollection<string> _musicBrainzTestResults;

        // Data available for the import (either cached here or in the database)
        bool _tagMinimumForImport;
        bool _musicBrainzDataLocal;
        bool _acoustIDLocal;

        LibraryEntryType _importAsType;

        TagFileViewModel _tagFile;

        SimpleCommand _copyTagCommand;

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
        public ObservableCollection<string> AcoustIDTestResults
        {
            get { return _acoustIDTestResults; }
            set { this.RaiseAndSetIfChanged(ref _acoustIDTestResults, value); }
        }
        public ObservableCollection<string> MusicBrainzTestResults
        {
            get { return _musicBrainzTestResults; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzTestResults, value); }
        }
        public bool TagMinimumForImport
        {
            get { return _tagMinimumForImport; }
            set { this.RaiseAndSetIfChanged(ref _tagMinimumForImport, value); }
        }
        public bool MusicBrainzDataLocal
        {
            get { return _musicBrainzDataLocal; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzDataLocal, value); }
        }
        public bool AcoustIDLocal
        {
            get { return _acoustIDLocal; }
            set { this.RaiseAndSetIfChanged(ref _acoustIDLocal, value); }
        }
        public TagFileViewModel TagFile
        {
            get { return _tagFile; }
            set { this.RaiseAndSetIfChanged(ref _tagFile, value); }
        }
        public LibraryEntryType ImportAsType
        {
            get { return _importAsType; }
            set { this.RaiseAndSetIfChanged(ref _importAsType, value); }
        }
        public string DestinationDirectory
        {
            get { return _destinationDirectory; }
            set { this.RaiseAndSetIfChanged(ref _destinationDirectory, value); }
        }
        public SimpleCommand CopyTagCommand
        {
            get { return _copyTagCommand; }
            set { this.RaiseAndSetIfChanged(ref _copyTagCommand, value); }
        }

        public LibraryLoaderImportFileViewModel(string fileName, string destinationDirectory, LibraryEntryType importAsType)
        {
            try
            {
                this.FileFullPath = fileName;
                this.FileName = Path.GetFileName(fileName);
                this.ImportAsType = importAsType;
                this.DestinationDirectory = destinationDirectory;
                this.AcoustIDTestResults = new ObservableCollection<string>();
                this.MusicBrainzTestResults = new ObservableCollection<string>();

                var file = TagLib.File.Create(fileName);

                this.TagFile = new TagFileViewModel(file);
                this.TagFile.PropertyChanged += (sender, e) =>
                {
                    UpdateMigrationDetails();
                };
                
                UpdateMigrationDetails();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading tag file:  {0}, {1}", LogMessageType.LibraryLoader, LogLevel.Error, fileName, ex.Message);
            }

            this.CopyTagCommand = new SimpleCommand(() =>
            {
                if (this.TagFile != null)
                {
                    try
                    {
                        Clipboard.SetDataObject(this.TagFile.Tag as TagLib.Tag);

                        ApplicationHelpers.Log("Tag data copied to clipboard:  {0}", LogMessageType.LibraryLoader, LogLevel.Information, this.TagFile.Name);
                    }
                    catch (Exception ex)
                    {
                        ApplicationHelpers.Log("Tag data copy to clipboard failed:  {0}", LogMessageType.LibraryLoader, LogLevel.Error, ex.Message);
                    }
                }
            });
        }

        private void UpdateMigrationDetails()
        {
            this.TagMinimumForImport = !string.IsNullOrWhiteSpace(this.TagFile.Tag.FirstAlbumArtist) &&
                                       !string.IsNullOrWhiteSpace(this.TagFile.Tag.Album) &&
                                       !string.IsNullOrWhiteSpace(this.TagFile.Tag.Title) &&
                                       this.TagFile.Tag.Track > 0;

            if (this.TagMinimumForImport)
            {
                this.FileMigrationName = CalculateFileName(this.TagFile.Tag.Track, this.TagFile.Tag.FirstAlbumArtist, this.TagFile.Tag.Title, LibraryEntryNamingType.Standard);
                this.FileMigrationFullPath = Path.Combine(this.DestinationDirectory, this.TagFile.Tag.FirstAlbumArtist, this.TagFile.Tag.Album, this.FileMigrationName);
            }
        }

        private string CalculateFileName(uint track, string firstAlbumArtist, string trackTitle, LibraryEntryNamingType namingType)
        {
            switch (namingType)
            {
                case LibraryEntryNamingType.Standard:
                {
                    var format = "{0:##} {1}-{2}.mp3";
                    var formattedTitle = string.Format(format, track, firstAlbumArtist, trackTitle);
                    return StringHelpers.MakeFriendlyFileName(formattedTitle);
                }
                case LibraryEntryNamingType.None:
                case LibraryEntryNamingType.Descriptive:
                default:
                    throw new Exception("Unhandled naming type:  LibraryLoaderFileViewModel.cs");
            }
        }
    }
}
