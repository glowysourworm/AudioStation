using AudioStation.Controller.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker
{
    public class LibraryLoaderMusicBrainzAlbumArtViewModel : LibraryLoaderWorkerViewModelBase
    {
        public LibraryLoaderMusicBrainzAlbumArtViewModel()
            : base("Music Brainz (album art)", "Downloads album art for any recordings which have a Music Brainz ID in the library", -1, false)
        {
        }
        public LibraryLoaderMusicBrainzAlbumArtViewModel(int workflowId)
            : base("Music Brainz (album art)", "Downloads album art for any recordings which have a Music Brainz ID in the library", workflowId, true)
        {
        }

        public override void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            base.Load(configuration, audioStationController, progressHandler);

            try
            {
                var results = audioStationController.ServiceController
                                                    .GetDataService<IAudioStationDbClient>()
                                                    .GetEntities<TagSmallVendorMap>();
                var counter = 0;

                foreach (var result in results.Where(x => x.MusicBrainzRecordingId != null))
                {
                    progressHandler(results.Count(), counter++, 0, "Loading: Music Brainz Id=" + result.MusicBrainzRecordingId);

                    this.WorkItems.Add(new LibraryWorkItemViewModel()
                    {
                        HasErrors = false,
                        InProgress = false,
                        IsCompleted = false,
                        LoadType = LibraryLoadType.MusicBrainzAlbumArt,
                        Load = new LibraryLoaderLoadViewModel()
                        {
                            // Vendor Lookup
                            Data = new LibraryLoaderEntityLoadViewModel<TagSmallVendorMap>()
                            {
                                Entity = result
                            },
                            DisplayText = "Music Brainz Result:  " + result.TagSmall.Title
                        },
                        Output = new LibraryLoaderOutputViewModel()
                        {
                            // File Reference(s)
                            Output = new LibraryLoaderEntitySetOutputViewModel<FileReference>()
                        },
                        Progress = 0
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing Library Loader component:  " + ex.Message);
            }

            this.Loaded = true;
        }
    }
}
