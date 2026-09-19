using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility.FileUtility;
using AudioStation.Event;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.SimpleCollections.Collection;
using SimpleWpf.UI.ViewModel.FileTreeView;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderFileConverterViewModel : LibraryLoaderWorkerViewModelBase<FileTreeNodeViewModel>
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

        protected override ILibraryLoaderLoad CreateWorkLoad(FileTreeNodeViewModel loadItem, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            return new LibraryLoaderLoad<LibraryLoaderFileConverterPayload>(this.Id, LibraryLoadType.FileConverter,
                                           new LibraryLoaderFileConverterPayload()
                                           {
                                               EncoderInfo = _destinationFormat,
                                               FileIn = loadItem.FullPath,
                                               FileOut = FileHelpers.ReplaceExtension(loadItem.FullPath, _destinationFormat.Extension),
                                           });
        }

        protected override IEnumerable<ILibraryLoaderLoad> CreateWorkLoads(IEnumerable<FileTreeNodeViewModel> loadItems, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                var audioConverter = IocContainer.Get<IAudioConverter>();

                _workItemDict.Clear();

                var result = new List<ILibraryLoaderLoad>();
                var counter = 0;

                //// Search for files that aren't the destination format (non-converted files)
                //var searchPatterns = audioConverter.GetSupportedFormats()
                //                                   .Where(x => x.Encoding != _destinationFormat.Encoding)
                //                                   .Select(x => "*" + x.Extension)
                //                                   .Distinct()
                //                                   .ToArray();

                foreach (var fileNode in loadItems)
                {
                    progressHandler(loadItems.Count(), counter++, 0, 0, "Loading: " + fileNode.FullPath);

                    // CORRUPT FILES! (This will go to file maintainence)
                    //if (fileNode.Size <= 0)
                    //    continue;

                    //// Already Added
                    //if (_workItemDict.ContainsKey(fileNode.FullPath))
                    //    continue;

                    result.Add(new LibraryLoaderLoad<LibraryLoaderFileConverterPayload>(this.Id, LibraryLoadType.FileConverter,
                               new LibraryLoaderFileConverterPayload()
                               {
                                   EncoderInfo = _destinationFormat,
                                   FileIn = fileNode.FullPath,
                                   FileOut = FileHelpers.ReplaceExtension(fileNode.FullPath, _destinationFormat.Extension),
                               }));

                    _workItemDict.Add(fileNode.FullPath, fileNode.FullPath);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing Library Loader component:  " + ex.Message);
            }
        }

        protected override LibraryLoaderLoadViewModel MapWorkLoad(ILibraryLoaderLoad workLoad)
        {
            throw new NotImplementedException();
        }

        protected override LibraryLoaderOutputViewModel MapWorkOutput(ILibraryLoaderOutput workOutput)
        {
            throw new NotImplementedException();
        }

        protected override ILibraryLoaderLoad ResetWorkLoad(LibraryWorkItemViewModel workItem)
        {
            throw new NotImplementedException();
        }
    }
}
