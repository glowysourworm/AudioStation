using System.Collections.ObjectModel;

using AudioStation.Controller.Interface;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import
{
    [IocExportDefault]
    public class LibraryImporterRadioViewModel : ViewModelBase
    {
        ObservableCollection<string> _importFilesStaged;
        string _importDirectory;

        SimpleCommand _runImportCommand;

        public ObservableCollection<string> ImportFilesStaged
        {
            get { return _importFilesStaged; }
            set { RaiseAndSetIfChanged(ref _importFilesStaged, value); }
        }
        public string ImportDirectory
        {
            get { return _importDirectory; }
            set { RaiseAndSetIfChanged(ref _importDirectory, value); }
        }
        public SimpleCommand RunImportCommand
        {
            get { return _runImportCommand; }
            set { RaiseAndSetIfChanged(ref _runImportCommand, value); }
        }

        [IocImportingConstructor]
        public LibraryImporterRadioViewModel(IDialogController dialogController)
        {
            //if (!string.IsNullOrEmpty(configuration.DirectoryBase))
            //{
            //    var files = ApplicationHelpers.FastGetFiles(configuration.DirectoryBase, "*.m3u", SearchOption.AllDirectories);

            //    this.ImportFilesStaged = new ObservableCollection<string>(files);
            //    this.ImportDirectory = configuration.DirectoryBase;
            //}
            //else
            //{
            //    this.ImportFilesStaged = new ObservableCollection<string>();
            //}

            //this.RunImportCommand = new SimpleCommand(() =>
            //{
            //    if (dialogController.ShowConfirmation("Library Database Initialization",
            //                                          "This will delete your existing library data and reload it from:",
            //                                          "", this.ImportDirectory, "",
            //                                          "Your radio file(s) will not be otherwise disturbed.",
            //                                          "Are you sure you want to do this?"))
            //    {
            //        //libraryLoader.LoadLibraryAsync(this.Configuration.DirectoryBase);
            //        //libraryLoader.Start();
            //    }
            //});
        }
    }
}
