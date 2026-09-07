using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;

using Microsoft.Win32;

using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.MainViewModels
{
    public class LibraryDirectoryViewModel : ViewModelBase, ILibraryDirectory
    {
        string _directory;
        string _directoryLabel;
        bool _isPrimary;
        bool _isReadOnly;
        bool _deleteUnusedFolders;
        TrackCategory _trackCategory;
        TrackGroupingType _groupingType;
        TrackNamingType _namingType;
        LibraryImportType _importType;
        AudioEncoderViewModel _formatPreference;

        SimpleCommand _openFolderCommand;

        public string Directory
        {
            get { return _directory; }
            set { this.RaiseAndSetIfChanged(ref _directory, value); }
        }
        public string DirectoryLabel
        {
            get { return _directoryLabel; }
            set { this.RaiseAndSetIfChanged(ref _directoryLabel, value); }
        }
        public bool IsPrimary
        {
            get { return _isPrimary; }
            set { this.RaiseAndSetIfChanged(ref _isPrimary, value); }
        }
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { this.RaiseAndSetIfChanged(ref _isReadOnly, value); }
        }
        public bool DeleteUnusedFolders
        {
            get { return _deleteUnusedFolders; }
            set { this.RaiseAndSetIfChanged(ref _deleteUnusedFolders, value); }
        }
        public TrackCategory TrackCategory
        {
            get { return _trackCategory; }
            set { this.RaiseAndSetIfChanged(ref _trackCategory, value); }
        }
        public TrackGroupingType GroupingType
        {
            get { return _groupingType; }
            set { this.RaiseAndSetIfChanged(ref _groupingType, value); }
        }
        public TrackNamingType NamingType
        {
            get { return _namingType; }
            set { this.RaiseAndSetIfChanged(ref _namingType, value); }
        }
        public LibraryImportType ImportType
        {
            get { return _importType; }
            set { this.RaiseAndSetIfChanged(ref _importType, value); }
        }
        public AudioEncoderViewModel FormatPreference
        {
            get { return _formatPreference; }
            set { this.RaiseAndSetIfChanged(ref _formatPreference, value); }
        }


        public SimpleCommand OpenFolderCommand
        {
            get { return _openFolderCommand; }
            set { this.RaiseAndSetIfChanged(ref _openFolderCommand, value); }
        }

        public LibraryDirectoryViewModel()
        {
            this.Directory = string.Empty;
            this.DirectoryLabel = string.Empty;
            this.TrackCategory = TrackCategory.Any;
            this.GroupingType = TrackGroupingType.None;
            this.NamingType = TrackNamingType.None;
            this.IsReadOnly = true;
            this.DeleteUnusedFolders = false;
            this.FormatPreference = new AudioEncoderViewModel();

            this.OpenFolderCommand = new SimpleCommand(() =>
            {
                var dialog = new OpenFolderDialog();
                dialog.Multiselect = false;

                if (!string.IsNullOrWhiteSpace(this.Directory))
                    dialog.InitialDirectory = this.Directory;

                if (dialog.ShowDialog() == true)
                {
                    this.Directory = dialog.FolderName;
                }
            });
        }
    }
}
