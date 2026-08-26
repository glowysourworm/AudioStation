using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

using AudioStation.Core;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.ViewModels.ComponentViewModels.LoadViewModels;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.Utilities;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.ViewModels.ComponentViewModels
{
    public enum LibraryManagerErrorFilterType
    {
        [Display(Name = "None", Description = "Do not apply any extra filtering to library results")]
        None,

        [Display(Name = "File Load Error", Description = "Search only for entries that had errors loading their files")]
        FileLoadError,

        [Display(Name = "File Un-Available", Description = "Search only for entries that have a missing file")]
        FileUnavailable
    }

    [IocExportDefault]
    public class LibraryManagerViewModel : ComponentViewModelBase<LibraryViewModel>
    {
        LibraryViewModel _library;

        ObservableCollection<string> _nonConvertedFiles;

        SimpleCommand _convertCommand;

        private readonly string[] CONVERTIBLE_FILE_EXT;
        private readonly string CONVERT_OUTPUT_FOLDER = "ConvertedFiles";

        public LibraryViewModel Library
        {
            get { return _library; }
            set { this.RaiseAndSetIfChanged(ref _library, value); }
        }

        public ObservableCollection<string> NonConvertedFiles
        {
            get { return _nonConvertedFiles; }
            set { this.RaiseAndSetIfChanged(ref _nonConvertedFiles, value); }
        }

        public SimpleCommand ConvertCommand
        {
            get { return _convertCommand; }
            set { this.RaiseAndSetIfChanged(ref _convertCommand, value); }
        }

        public override LibraryViewModel Load { get { return this.Library; } }

        [IocImportingConstructor]
        public LibraryManagerViewModel(IIocEventAggregator eventAggregator)
        {
            CONVERTIBLE_FILE_EXT = new string[]
            {
                                        ".wma", ".wav", ".m4a"
            };

            this.Library = new LibraryViewModel();
            this.NonConvertedFiles = new ObservableCollection<string>();

            this.ConvertCommand = new SimpleCommand(async () =>
            {
                var dialogViewModel = new DialogLoadingViewModel()
                {
                    Title = "Converting Files",
                    Progress = 0,
                    ShowProgressBar = true
                };

                // Dialog Show
                eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData(dialogViewModel));

                // Convert...
                //await viewModelLoader.ConvertFiles(this.NonConvertedFiles, (progress, fileName) =>
                //{
                //    ApplicationHelpers.BeginInvokeDispatcher(() =>
                //    {
                //        dialogViewModel.Progress = progress;
                //        dialogViewModel.Message = fileName;

                //    }, DispatcherPriority.Background);
                //});

                // Dialog Hide
                eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());

                //this.NonConvertedFiles.Clear();
                //this.NonConvertedFiles.AddRange(viewModelLoader.LoadNonConvertedFiles());
            });
        }

        public override void Initialize(AudioStationConfiguration configuration, LibraryViewModel load, DialogProgressHandler progressHandler)
        {
            try
            {
                // TODO: CHECK CONFIGURATION!
                if (!System.IO.Path.Exists(configuration.DirectoryBase))
                    return;

                var allFiles = BasicHelpers.FastGetFileData(configuration.DirectoryBase, "*.*", false, System.IO.SearchOption.AllDirectories);

                var convertibleFiles = allFiles.Where(x => CONVERTIBLE_FILE_EXT.Any(z => x.Path.EndsWith(z)))
                                               .Select(x => x.Path)
                                               .ToList();

                this.NonConvertedFiles.AddRange(convertibleFiles);

                // Load Artists / Albums / Genres
                //await this.Library.Initialize(progressHandler);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading non-converted files:  {0}", LogLevel.Error, ex, ex.Message);
                this.NonConvertedFiles.Clear();
            }
        }

        public override void Dispose()
        {

        }
    }
}
