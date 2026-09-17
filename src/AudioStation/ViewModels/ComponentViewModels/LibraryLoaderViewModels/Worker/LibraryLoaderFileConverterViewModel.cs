using System.IO;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility.FileUtility;
using AudioStation.Event;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.Native.IO;
using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderFileConverterViewModel : LibraryLoaderWorkerViewModelBase
    {
        // Use for extra performance
        SimpleDictionary<string, string> _workItemDict;
        AudioEncoderInfo _destinationFormat;

        public LibraryLoaderFileConverterViewModel(AudioEncoderInfo destinationFormat)
            : base("File Converter", "Verifies integrity of files related to Audio Station's library")
        {
            _destinationFormat = destinationFormat;
            _workItemDict = new SimpleDictionary<string, string>();
        }

        protected override IEnumerable<LibraryLoaderLoad> CreateWorkLoads(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                var audioConverter = IocContainer.Get<IAudioConverter>();

                _workItemDict.Clear();

                var result = new List<LibraryLoaderLoad>();

                // Search for files that aren't the destination format (non-converted files)
                var searchPatterns = audioConverter.GetSupportedFormats()
                                                   .Where(x => x.Encoding != _destinationFormat.Encoding)
                                                   .Select(x => "*" + x.Extension)
                                                   .Distinct()
                                                   .ToArray();

                foreach (var libraryDirectory in configuration.LibraryDirectories.Union(new LibraryDirectory[]
                {
                        configuration.StagingFolder,
                        configuration.DownloadFolder
                }))
                {
                    // Read-only directories
                    if (libraryDirectory.IsReadOnly)
                        continue;

                    using (var nativeIO = new FastDirectoryIO(libraryDirectory.Directory, SearchOption.AllDirectories, searchPatterns))
                    {
                        var audioFiles = nativeIO.GetFiles().Where(x => !x.IsDirectory).ToList();
                        var counter = 0;

                        foreach (var file in audioFiles)
                        {
                            progressHandler(audioFiles.Count, counter++, 0, 0, "Loading: " + file.FullPath);

                            // CORRUPT FILES! (This will go to file maintainence)
                            if (file.Size <= 0)
                                continue;

                            // Already Added
                            if (_workItemDict.ContainsKey(file.FullPath))
                                continue;

                            result.Add(new LibraryLoaderLoad(LibraryLoadType.FileConverter,
                                       new LibraryLoaderFileConverterLoad(LibraryLoadType.FileConverter)
                                       {
                                           EncoderInfo = _destinationFormat,
                                           FileIn = file.FullPath,
                                           FileOut = FileHelpers.ReplaceExtension(file.FullPath, _destinationFormat.Extension),
                                       }));

                            _workItemDict.Add(file.FullPath, file.FullPath);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing Library Loader component:  " + ex.Message);
            }
        }

        protected override LibraryLoaderLoadViewModel MapWorkLoad(LibraryLoaderLoad workLoad)
        {
            throw new NotImplementedException();
        }

        protected override LibraryLoaderOutputViewModel MapWorkOutput(LibraryLoaderOutput workOutput)
        {
            throw new NotImplementedException();
        }

        protected override LibraryLoaderLoad ResetWorkLoad(LibraryWorkItemViewModel workItem)
        {
            throw new NotImplementedException();
        }
    }
}
