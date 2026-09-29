using AudioStation.Controller.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;

using Newtonsoft.Json;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.DataComponent.MainViewModels
{
    public class LibraryDirectoryViewModel : ViewModelBase, ILibraryDirectory
    {
        string _directory;
        string _directoryLabel;
        string _customGroupingFormat;
        string _customNamingFormat;
        bool _isPrimary;
        bool _isReadOnly;
        bool _deleteUnusedFolders;
        TrackGroupingType _groupingType;
        TrackNamingType _namingType;

        SimpleCommand _openFolderCommand;
        SimpleCommand _editCustomNamingFormatCommand;
        SimpleCommand _editCustomGroupingFormatCommand;

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
        public string CustomGroupingFormat
        {
            get { return _customGroupingFormat; }
            set { this.RaiseAndSetIfChanged(ref _customGroupingFormat, value); }
        }
        public string CustomNamingFormat
        {
            get { return _customNamingFormat; }
            set { this.RaiseAndSetIfChanged(ref _customNamingFormat, value); }
        }

        [JsonIgnore]
        public SimpleCommand OpenFolderCommand
        {
            get { return _openFolderCommand; }
            set { this.RaiseAndSetIfChanged(ref _openFolderCommand, value); }
        }

        [JsonIgnore]
        public SimpleCommand EditCustomNamingFormatCommand
        {
            get { return _editCustomNamingFormatCommand; }
            set { this.RaiseAndSetIfChanged(ref _editCustomNamingFormatCommand, value); }
        }

        [JsonIgnore]
        public SimpleCommand EditCustomGroupingFormatCommand
        {
            get { return _editCustomGroupingFormatCommand; }
            set { this.RaiseAndSetIfChanged(ref _editCustomGroupingFormatCommand, value); }
        }

        public LibraryDirectoryViewModel()
        {
            var dialogController = IocContainer.Get<IDialogController>();

            this.Directory = string.Empty;
            this.DirectoryLabel = string.Empty;
            this.GroupingType = TrackGroupingType.None;
            this.NamingType = TrackNamingType.None;
            this.IsReadOnly = true;
            this.DeleteUnusedFolders = false;
            this.CustomGroupingFormat = string.Empty;
            this.CustomNamingFormat = string.Empty;

            this.OpenFolderCommand = new SimpleCommand(() =>
            {
                this.Directory = dialogController.ShowSelectFolder();
            });

            this.EditCustomGroupingFormatCommand = new SimpleCommand(() =>
            {

            });
            this.EditCustomNamingFormatCommand = new SimpleCommand(() =>
            {

            });
        }
    }
}
