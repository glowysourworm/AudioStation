﻿using System.Collections.ObjectModel;
using System.IO;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility;

using SimpleWpf.Extensions.Command;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.ViewModels.LibraryLoaderViewModels.Import
{
    public class LibraryLoaderImportRadioViewModel : PrimaryViewModelBase
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

        public LibraryLoaderImportRadioViewModel(IConfigurationManager configurationManager, IDialogController dialogController)
        {
            var configuration = configurationManager.GetConfiguration();

            if (!string.IsNullOrEmpty(configuration.DirectoryBase))
            {
                var files = ApplicationHelpers.FastGetFiles(configuration.DirectoryBase, "*.m3u", SearchOption.AllDirectories);

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
                                                      "Your radio file(s) will not be otherwise disturbed.",
                                                      "Are you sure you want to do this?"))
                {
                    //libraryLoader.LoadLibraryAsync(this.Configuration.DirectoryBase);
                    //libraryLoader.Start();
                }
            });
        }

        public override Task Initialize(DialogProgressHandler progressHandler)
        {
            return Task.CompletedTask;
        }

        public override void Dispose()
        {

        }
    }
}
