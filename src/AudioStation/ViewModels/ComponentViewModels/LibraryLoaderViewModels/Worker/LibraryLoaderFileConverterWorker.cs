using System.IO;

using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility.FileUtility;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load;
using AudioStation.ViewModels.ComponentViewModels.LoadViewModels;

using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.Native.IO;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderFileConverterWorker : LibraryLoaderWorkerViewModelBase
    {
        private readonly IAudioConverter _audioConverter;

        public LibraryLoaderFileConverterWorker(
                IAudioConverter audioConverter,
                IIocEventAggregator eventAggregator,
                ILibraryLoaderWorkerService libraryLoaderService)
            : base("File Converter", "Verifies integrity of files related to Audio Station's library", eventAggregator, libraryLoaderService)
        {
            _audioConverter = audioConverter;
        }

        protected override void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationViewModelController viewModelController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                foreach (var libraryDirectory in configuration.LibraryDirectories.Union(new LibraryDirectory[]
                {
                    configuration.StagingFolder,
                    configuration.DownloadFolder
                }))
                {
                    using (var nativeIO = new FastDirectoryIO(libraryDirectory.Directory, "*.wma", SearchOption.AllDirectories))
                    {
                        var wmaFiles = nativeIO.GetFiles();

                        foreach (var file in wmaFiles)
                        {
                            // CORRUPT FILES! (This will go to file maintainence)
                            if (file.Size <= 0)
                                continue;

                            // Get supported output types (this could be done without reading files.. just a first try)
                            var mediaTypes = _audioConverter.GetSupportedOutputFormats(file.Path);

                            if (!mediaTypes.Any())
                                continue;

                            this.WorkItems.Add(new LibraryWorkItemViewModel()
                            {
                                HasErrors = false,
                                InProgress = false,
                                IsCompleted = false,
                                Load = new LibraryLoaderLoadViewModel()
                                {
                                    DisplayText = file.FileName,
                                    Data = new LibraryLoaderFileConverterLoadViewModel()
                                    {
                                        FileIn = file.FileName,
                                        FileOut = FileHelpers.ReplaceExtension(file.FileName, "*.mp3"),
                                        EncoderInfo = mediaTypes.First()
                                    }
                                },
                                LoadType = LibraryLoadType.FileChecker,
                                Output = new LibraryLoaderOutputViewModel()
                                {
                                    Output = new NoViewModel()
                                },
                                Progress = 0
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing Library Loader component:  " + ex.Message);
            }
        }

        protected override void LoadImpl(IAudioStationConfiguration configuration, IComponentViewModelLoader viewModelLoader, DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }
    }
}
