using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.ViewModels.DataComponent;
using AudioStation.ViewModels.LibraryViewModels;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.UI.Command;

namespace AudioStation.ViewModels.ServiceComponent
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

    public class LibraryManagerViewModel : ServiceComponentViewModelBase
    {
        LibraryViewModel _library;

        ObservableCollection<string> _nonConvertedFiles;
        ObservableCollection<TrackViewModel> _trackTabItems;

        SimpleCommand _convertCommand;
        SimpleCommand<TrackViewModel> _addTrackTabCommand;
        SimpleCommand<TrackViewModel> _removeTrackTabCommand;

        public LibraryViewModel Library
        {
            get { return _library; }
            private set { this.RaiseAndSetIfChanged(ref _library, value); }
        }
        public ObservableCollection<string> NonConvertedFiles
        {
            get { return _nonConvertedFiles; }
            private set { this.RaiseAndSetIfChanged(ref _nonConvertedFiles, value); }
        }
        public ObservableCollection<TrackViewModel> TrackTabItems
        {
            get { return _trackTabItems; }
            private set { this.RaiseAndSetIfChanged(ref _trackTabItems, value); }
        }

        public SimpleCommand ConvertCommand
        {
            get { return _convertCommand; }
            set { this.RaiseAndSetIfChanged(ref _convertCommand, value); }
        }
        public SimpleCommand<TrackViewModel> AddTrackTabCommand
        {
            get { return _addTrackTabCommand; }
            set { RaiseAndSetIfChanged(ref _addTrackTabCommand, value); }
        }
        public SimpleCommand<TrackViewModel> RemoveTrackTabCommand
        {
            get { return _removeTrackTabCommand; }
            set { RaiseAndSetIfChanged(ref _removeTrackTabCommand, value); }
        }

        public LibraryManagerViewModel(IIocEventAggregator eventAggregator) : base("Library Manager")
        {
            this.Library = null;
            this.NonConvertedFiles = new ObservableCollection<string>();

            // Library Entry Tabs (closeable / ManagerView)
            this.AddTrackTabCommand = new SimpleCommand<TrackViewModel>(viewModel =>
            {
                this.TrackTabItems.Add(viewModel);
            });
            this.RemoveTrackTabCommand = new SimpleCommand<TrackViewModel>(viewModel =>
            {
                this.TrackTabItems.Remove(viewModel);
            });

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
        public override bool CanExecute()
        {
            return this.Initialized && this.Loaded && !this.Loading;
        }
        public override bool CanReset()
        {
            return this.Initialized && this.Loaded && !this.Loading;
        }
        public override bool CanLoad()
        {
            return this.Initialized && !this.Loaded && !this.Loading;
        }

        public override void Initialize(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // We're going to also load the library on startup
            this.Initialized = true;

            try
            {
                // Audio Station Library!
                //
                this.Library = audioStationController.LibraryLoaderService.LoadLibrary(progressHandler);

                //var allFiles = BasicHelpers.FastGetFileData(configuration.DirectoryBase, "*.*", false, System.IO.SearchOption.AllDirectories);

                //var convertibleFiles = allFiles.Where(x => CONVERTIBLE_FILE_EXT.Any(z => x.Path.EndsWith(z)))
                //                               .Select(x => x.Path)
                //                               .ToList();

                //this.NonConvertedFiles.AddRange(convertibleFiles);

                // Load Artists / Albums / Genres
                //await this.Library.Initialize(progressHandler);

                this.Loaded = true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading non-converted files:  {0}", LogLevel.Error, ex, ex.Message);
                //this.NonConvertedFiles.Clear();
            }
        }

        public override void Load(Guid? componentPartId, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // TODO: Component Parts:  Library Maintainence Tasks:  Convert Files; Check Files; ...

            //try
            //{
            //    // Audio Station Library!
            //    //
            //    this.Library = audioStationController.LibraryLoaderService.LoadLibrary(progressHandler);

            //    //var allFiles = BasicHelpers.FastGetFileData(configuration.DirectoryBase, "*.*", false, System.IO.SearchOption.AllDirectories);

            //    //var convertibleFiles = allFiles.Where(x => CONVERTIBLE_FILE_EXT.Any(z => x.Path.EndsWith(z)))
            //    //                               .Select(x => x.Path)
            //    //                               .ToList();

            //    //this.NonConvertedFiles.AddRange(convertibleFiles);

            //    // Load Artists / Albums / Genres
            //    //await this.Library.Initialize(progressHandler);

            //    this.Loaded = true;
            //}
            //catch (Exception ex)
            //{
            //    ApplicationHelpers.Log("Error loading non-converted files:  {0}", LogLevel.Error, ex, ex.Message);
            //    //this.NonConvertedFiles.Clear();
            //}
        }

        public override void Execute(Guid? componentPartId, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // TODO: Component Parts
        }
        public override void Reset(Guid? componentPartId, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.Library.Dispose();

            this.Loaded = false;
        }
        public override void Dispose()
        {

        }
    }
}
