using System.Collections.ObjectModel;
using System.IO;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    [IocExportDefault]
    public class LibraryLoaderImportBasicViewModel : ViewModelBase
    {
        LibraryEntryType _selectedImportType;
        ObservableCollection<string> _importFilesStaged;
        string _importDirectory;

        SimpleCommand _runImportCommand;

        public LibraryEntryType SelectedImportType
        {
            get { return _selectedImportType; }
            set { this.RaiseAndSetIfChanged(ref _selectedImportType, value); }
        }
        public ObservableCollection<string> ImportFilesStaged
        {
            get { return _importFilesStaged; }
            set { this.RaiseAndSetIfChanged(ref _importFilesStaged, value); }
        }
        public string ImportDirectory
        {
            get { return _importDirectory; }
            set { this.RaiseAndSetIfChanged(ref _importDirectory, value); }
        }
        public SimpleCommand RunImportCommand
        {
            get { return _runImportCommand; }
            set { this.RaiseAndSetIfChanged(ref _runImportCommand, value); }
        }

        [IocImportingConstructor]
        public LibraryLoaderImportBasicViewModel(IConfigurationManager configurationManager, IDialogController dialogController)
        {
            var configuration = configurationManager.GetConfiguration();

            if (!string.IsNullOrEmpty(configuration.DirectoryBase))
            {
                var files = ApplicationHelpers.FastGetFiles(configuration.DirectoryBase, "*.mp3", SearchOption.AllDirectories);

                this.ImportFilesStaged = new ObservableCollection<string>(files);
                this.ImportDirectory = configuration.DirectoryBase;
            }
            else
            {
                this.ImportFilesStaged = new ObservableCollection<string>();
            }

            this.RunImportCommand = new SimpleCommand(() =>
            {
                if (dialogController.ShowConfirmation("Library Database Initialization",
                                          "This will delete your existing library data and reload it from:",
                                          "", this.ImportDirectory, "",
                                          "Your audio file(s) will not be otherwise disturbed.",
                                          "Are you sure you want to do this?"))
                {
                    //libraryLoader.LoadLibraryAsync(this.Configuration.DirectoryBase);
                    //libraryLoader.Start();
                }
            });
        }
    }
}
