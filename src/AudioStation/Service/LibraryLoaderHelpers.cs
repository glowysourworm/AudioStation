using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LogViewModels;

namespace AudioStation.Service
{
    public static class LibraryLoaderHelpers
    {
        //public static ILibraryLoaderLoad CreateLoad(LibraryLoadType loadType)
        //{
        //    switch (loadType)
        //    {
        //        case LibraryLoadType.Import:
        //            return new LibraryLoaderImportLoad();

        //        case LibraryLoadType.AcoustID:
        //        {
        //            var workLoad = workItem.Load.Data as LibraryLoaderFileLoadViewModel;

        //            if (workLoad == null)
        //                throw new ArgumentException("Invalid work load for Library Loader AcoustID Lookup");

        //            return new LibraryLoaderFileLoad(workLoad.FullPath);
        //        }
        //        case LibraryLoadType.MusicBrainzBasic:
        //        {
        //            var workLoad = workItem.Load.Data as LibraryLoaderEntitySetLoadViewModel<AcoustIDLookupResult>;

        //            if (workLoad == null)
        //                throw new ArgumentException("Invalid work load for Library Loader Music Brainz Import");

        //            return new LibraryLoaderEntitySetLoad<AcoustIDLookupResult>(workLoad.EntitySet);
        //        }
        //        case LibraryLoadType.MusicBrainzAlbumArt:
        //        {
        //            var workLoad = workItem.Load.Data as LibraryLoaderEntityLoadViewModel<TagSmallVendorMap>;

        //            if (workLoad == null)
        //                throw new ArgumentException("Invalid work load for Library Loader Music Brainz Album Art");

        //            return new LibraryLoaderEntityLoad<TagSmallVendorMap>(workLoad.Entity);
        //        }
        //        case LibraryLoadType.FileChecker:
        //        {
        //            var workLoad = workItem.Load.Data as LibraryLoaderEntityLoadViewModel<FileReference>;

        //            if (workLoad == null)
        //                throw new ArgumentException("Invalid work load for Library Loader File Checker");

        //            return new LibraryLoaderEntityLoad<FileReference>(workLoad.Entity);
        //        }
        //        case LibraryLoadType.FileConverter:
        //        {
        //            var workLoad = workItem.Load.Data as LibraryLoaderFileConverterLoadViewModel;

        //            if (workLoad == null)
        //                throw new ArgumentException("Invalid work load for Library Loader File Converter");

        //            return new LibraryLoaderFileConverterLoad()
        //            {
        //                EncoderInfo = workLoad.EncoderInfo,
        //                FileIn = workLoad.FileIn,
        //                FileOut = workLoad.FileOut
        //            };
        //        }
        //        case LibraryLoadType.ImportRadio:
        //        default:
        //            throw new Exception("Unhandled Libary Loader load type");
        //    }
        //}

        //public static object CreateLoad(LibraryWorkItemViewModel workItem)
        //{
        //    var audioStationMapper = IocContainer.Get<IAudioStationMapper>();

        //    switch (workItem.LoadType)
        //    {
        //        case LibraryLoadType.Import:
        //        {
        //            var workLoad = workItem.Load.Data as LibraryLoaderImportLoadViewModel;

        //            if (workLoad == null)
        //                throw new ArgumentException("Invalid work load for Library Loader Import");

        //            return audioStationMapper.Map<LibraryLoaderImportLoadViewModel, LibraryLoaderImportLoad>(workLoad);
        //        }
        //        case LibraryLoadType.AcoustID:
        //        {
        //            var workLoad = workItem.Load.Data as LibraryLoaderFileLoadViewModel;

        //            if (workLoad == null)
        //                throw new ArgumentException("Invalid work load for Library Loader AcoustID Lookup");

        //            return new LibraryLoaderFileLoad(workLoad.FullPath);
        //        }
        //        case LibraryLoadType.MusicBrainzBasic:
        //        {
        //            var workLoad = workItem.Load.Data as LibraryLoaderEntitySetLoadViewModel<AcoustIDLookupResult>;

        //            if (workLoad == null)
        //                throw new ArgumentException("Invalid work load for Library Loader Music Brainz Import");

        //            return new LibraryLoaderEntitySetLoad<AcoustIDLookupResult>(workLoad.EntitySet);
        //        }
        //        case LibraryLoadType.MusicBrainzAlbumArt:
        //        {
        //            var workLoad = workItem.Load.Data as LibraryLoaderEntityLoadViewModel<TagSmallVendorMap>;

        //            if (workLoad == null)
        //                throw new ArgumentException("Invalid work load for Library Loader Music Brainz Album Art");

        //            return new LibraryLoaderEntityLoad<TagSmallVendorMap>(workLoad.Entity);
        //        }
        //        case LibraryLoadType.FileChecker:
        //        {
        //            var workLoad = workItem.Load.Data as LibraryLoaderEntityLoadViewModel<FileReference>;

        //            if (workLoad == null)
        //                throw new ArgumentException("Invalid work load for Library Loader File Checker");

        //            return new LibraryLoaderEntityLoad<FileReference>(workLoad.Entity);
        //        }
        //        case LibraryLoadType.FileConverter:
        //        {
        //            var workLoad = workItem.Load.Data as LibraryLoaderFileConverterLoadViewModel;

        //            if (workLoad == null)
        //                throw new ArgumentException("Invalid work load for Library Loader File Converter");

        //            return new LibraryLoaderFileConverterLoad()
        //            {
        //                EncoderInfo = workLoad.EncoderInfo,
        //                FileIn = workLoad.FileIn,
        //                FileOut = workLoad.FileOut
        //            };
        //        }
        //        case LibraryLoadType.ImportRadio:
        //        default:
        //            throw new Exception("Unhandled Libary Loader load type");
        //    }
        //}
        public static void ApplyLibraryLoaderWorkItem(LibraryLoaderWorkItemUpdate sender, ref LibraryWorkItemViewModel viewModel)
        {
            // Log Messages
            foreach (var message in sender.Log)
            {
                if (!viewModel.LogMessages.Any(x => x.Timestamp.Equals(message.Timestamp)))
                {
                    viewModel.LogMessages.Add(new LogMessageViewModel()
                    {
                        Level = message.Level,
                        Message = message.Message,
                        Timestamp = message.Timestamp,
                        Type = message.Type
                    });
                }
            }

            // Work Steps
            foreach (var workStep in sender.ResultStepsCompleted)
            {
                if (!viewModel.WorkSteps.Any(x => x.StepNumber == workStep.StepNumber))
                {
                    viewModel.WorkSteps.Add(new LibraryLoaderWorkStepViewModel()
                    {
                        Complete = workStep.Completed,
                        Message = workStep.Message,
                        StepNumber = workStep.StepNumber,
                        Result = workStep.Result
                    });
                }
            }

            viewModel.State = sender.State;
            viewModel.Progress = !sender.ResultStepsCompleted.Any() ? 0 : (sender.ResultStepsCompleted.Count() / (double)sender.ResultStepCount);
        }

        public static void ApplyLibraryLoaderWorkItem(LibraryLoaderWorkItem sender, ref LibraryWorkItemViewModel viewModel)
        {
            // Log Messages
            foreach (var message in sender.GetOutputItem().CurrentLog)
            {
                if (!viewModel.LogMessages.Any(x => x.Timestamp.Equals(message.Timestamp)))
                {
                    viewModel.LogMessages.Add(new LogMessageViewModel()
                    {
                        Level = message.Level,
                        Message = message.Message,
                        Timestamp = message.Timestamp,
                        Type = message.Type
                    });
                }
            }

            // Work Steps
            foreach (var workStep in sender.GetOutputItem().ResultSteps)
            {
                if (!viewModel.WorkSteps.Any(x => x.StepNumber == workStep.StepNumber))
                {
                    viewModel.WorkSteps.Add(new LibraryLoaderWorkStepViewModel()
                    {
                        Complete = workStep.Completed,
                        Message = workStep.Message,
                        StepNumber = workStep.StepNumber,
                        Result = workStep.Result
                    });
                }
            }

            viewModel.State = sender.GetLoadState();
            viewModel.Progress = !sender.GetOutputItem().ResultSteps.Any(x => x.Completed) ? 0
                                    : (sender.GetOutputItem().ResultSteps.Count(x => x.Completed) / (double)sender.GetOutputItem().ResultSteps.Count());
        }
    }
}
