using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;

using Microsoft.Win32;

using SimpleWpf.Extensions.Command;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.MainViewModels
{
    public class LibraryDirectoryViewModel : ViewModelBase, ILibraryDirectory
    {
        string _directory;
        string _directoryLabel;
        TrackType _trackType;
        TrackGroupingType _trackGroupingType;
        TrackNamingType _trackNamingType;

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
        public TrackType TrackType
        {
            get { return _trackType; }
            set { this.RaiseAndSetIfChanged(ref _trackType, value); }
        }
        public TrackGroupingType GroupingType
        {
            get { return _trackGroupingType; }
            set { this.RaiseAndSetIfChanged(ref _trackGroupingType, value); }
        }
        public TrackNamingType NamingType
        {
            get { return _trackNamingType; }
            set { this.RaiseAndSetIfChanged(ref _trackNamingType, value); }
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
            this.TrackType = TrackType.Any;
            this.GroupingType = TrackGroupingType.None;
            this.NamingType = TrackNamingType.None;

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
