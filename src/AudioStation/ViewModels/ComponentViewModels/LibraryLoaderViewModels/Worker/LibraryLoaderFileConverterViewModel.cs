using System.IO;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility.FileUtility;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load;
using AudioStation.ViewModels.ComponentViewModels.LoadViewModels;

using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.Native.IO;
using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderFileConverterViewModel : LibraryLoaderWorkerViewModelBase
    {
        private readonly IAudioConverter _audioConverter;

        // Use for extra performance
        SimpleDictionary<string, string> _workItemDict;

        public LibraryLoaderFileConverterViewModel(
                IAudioConverter audioConverter,
                IIocEventAggregator eventAggregator,
                ILibraryLoaderWorkerService libraryLoaderService)
            : base("File Converter", "Verifies integrity of files related to Audio Station's library", eventAggregator, libraryLoaderService)
        {
            _audioConverter = audioConverter;

            _workItemDict = new SimpleDictionary<string, string>();
        }

        protected override void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                _workItemDict.Clear();

                foreach (var format in _audioConverter.GetSupportedFormats())
                {
                    foreach (var libraryDirectory in configuration.LibraryDirectories.Union(new LibraryDirectory[]
                    {
                        configuration.StagingFolder,
                        configuration.DownloadFolder
                    }))
                    {
                        // Read-only directories
                        if (libraryDirectory.IsReadOnly)
                            continue;

                        // Only need to look for non-converted files
                        if (format.Encoding == libraryDirectory.FormatPreference.Encoding)
                            continue;

                        using (var nativeIO = new FastDirectoryIO(libraryDirectory.Directory, format.Filter, SearchOption.AllDirectories))
                        {
                            var audioFiles = nativeIO.GetFiles().Where(x => !x.IsDirectory).ToList();

                            foreach (var file in audioFiles)
                            {
                                // CORRUPT FILES! (This will go to file maintainence)
                                if (file.Size <= 0)
                                    continue;

                                // Already Added
                                if (_workItemDict.ContainsKey(file.FullPath))
                                    continue;

                                this.WorkItems.Add(new LibraryWorkItemViewModel()
                                {
                                    HasErrors = false,
                                    InProgress = false,
                                    IsCompleted = false,
                                    Load = new LibraryLoaderLoadViewModel()
                                    {
                                        DisplayText = file.FullPath,
                                        Data = new LibraryLoaderFileConverterLoadViewModel()
                                        {
                                            FileIn = file.FullPath,
                                            FileOut = FileHelpers.ReplaceExtension(file.FullPath, libraryDirectory.FormatPreference.Extension),
                                            EncoderInfo = libraryDirectory.FormatPreference
                                        }
                                    },
                                    LoadType = LibraryLoadType.FileConverter,
                                    Output = new LibraryLoaderOutputViewModel()
                                    {
                                        Output = new NoViewModel()
                                    },
                                    Progress = 0
                                });

                                _workItemDict.Add(file.FullPath, file.FullPath);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing Library Loader component:  " + ex.Message);
            }
        }

        protected override void LoadImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }
    }
}
