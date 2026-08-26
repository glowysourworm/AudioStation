using System.Collections.ObjectModel;

using AudioStation.Core;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Input;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output;
using AudioStation.ViewModels.ComponentViewModels.LoadViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.ComponentViewModels
{
    [IocExportDefault]
    public class LibraryLoaderMusicBrainzViewModel : LibraryLoaderComponentViewModelBase
    {
        private readonly IAudioStationDbClient _audioStationDbClient;
        public override NoViewModel? Load { get; }

        [IocImportingConstructor]
        public LibraryLoaderMusicBrainzViewModel(
                IIocEventAggregator eventAggregator,
                ILibraryLoaderService libraryLoaderService,
                IAudioStationDbClient audioStationDbClient)
            : base(eventAggregator, libraryLoaderService)
        {
            _audioStationDbClient = audioStationDbClient;
        }

        protected override void InitializeComponent(Configuration configuration, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            try
            {
                var results = _audioStationDbClient.GetEntities<AcoustIDLookupResult>();

                foreach (var result in results.GroupBy(x => x.MusicBrainzRecordingId))
                {
                    this.WorkItems.Add(new LibraryWorkItemViewModel()
                    {
                        HasErrors = false,
                        InProgress = false,
                        IsCompleted = false,
                        Load = new LibraryLoaderEntitySetLoadViewModel<AcoustIDLookupResult>()
                        {
                            DisplayName = result.First().FileName,
                            EntitySet = new ObservableCollection<AcoustIDLookupResult>(result)
                        },
                        LoadType = LibraryLoadType.MusicBrainzTagSmall,
                        Output = new LibraryLoaderEntitySetOutputViewModel<TagSmall>(),
                        Progress = 0
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing Library Loader component:  " + ex.Message);
            }
        }

        public override void Dispose()
        {

        }


    }
}
