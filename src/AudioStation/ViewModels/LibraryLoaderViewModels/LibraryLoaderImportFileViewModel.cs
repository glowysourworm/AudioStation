using System.Collections.ObjectModel;
using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.Vendor.TagLibViewModel;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    public class LibraryLoaderImportFileViewModel : ViewModelBase
    {
        public event SimpleEventHandler<LibraryLoaderImportFileViewModel> ImportBasicEvent;

        bool _isSelected;
        bool _isExpanded;

        string _fileName;
        string _fileFullPath;
        string _fileMigrationName;
        string _fileMigrationFullPath;
        string _destinationDirectory;

        // Data available for the import (either cached here or in the database)
        bool _tagMinimumForImport;

        LibraryEntryType _importAsType;

        TagFileViewModel _tagFile;
        LibraryLoaderImportOutputViewModel _importOutput;

        SimpleCommand _importBasicCommand;

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
        public bool TagMinimumForImport
        {
            get { return _tagMinimumForImport; }
            set { this.RaiseAndSetIfChanged(ref _tagMinimumForImport, value); }
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
        public string DestinationDirectory
        {
            get { return _destinationDirectory; }
            set { this.RaiseAndSetIfChanged(ref _destinationDirectory, value); }
        }
        public SimpleCommand ImportBasicCommand
        {
            get { return _importBasicCommand; }
            set { this.RaiseAndSetIfChanged(ref _importBasicCommand, value); }
        }

        public LibraryLoaderImportFileViewModel(string fileName, string destinationDirectory, LibraryEntryType importAsType)
        {
            try
            {
                var tagCacheController = IocContainer.Get<ITagCacheController>();

                this.FileFullPath = fileName;
                this.FileName = Path.GetFileName(fileName);
                this.ImportAsType = importAsType;
                this.DestinationDirectory = destinationDirectory;
                this.ImportOutput = new LibraryLoaderImportOutputViewModel();

                this.ImportBasicCommand = new SimpleCommand(() =>
                {
                    if (this.ImportBasicEvent != null)
                        this.ImportBasicEvent(this);

                }, () => this.TagMinimumForImport);

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
                    ApplicationHelpers.Log("Error loading tag file:  {0}", LogMessageType.LibraryLoader, LogLevel.Error, fileName);
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading tag file:  {0}, {1}", LogMessageType.LibraryLoader, LogLevel.Error, fileName, ex.Message);
            }
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
                    var format = "{0:##} {1}.mp3";
                    var formattedTitle = string.Format(format, track, trackTitle);
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
