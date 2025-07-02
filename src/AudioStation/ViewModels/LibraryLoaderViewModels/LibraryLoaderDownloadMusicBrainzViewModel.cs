using System.Collections.ObjectModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    [IocExportDefault]
    public class LibraryLoaderDownloadMusicBrainzViewModel : ViewModelBase
    {
        ObservableCollection<Mp3FileReference> _entitiesStaged;

        SimpleCommand _runImportCommand;

        public ObservableCollection<Mp3FileReference> EntitiesStaged
        {
            get { return _entitiesStaged; }
            set { this.RaiseAndSetIfChanged(ref _entitiesStaged, value); }
        }
        public SimpleCommand RunImportCommand
        {
            get { return _runImportCommand; }
            set { this.RaiseAndSetIfChanged(ref _runImportCommand, value); }
        }

        [IocImportingConstructor]
        public LibraryLoaderDownloadMusicBrainzViewModel(IModelController modelController,
                                                         IConfigurationManager configurationManager,
                                                         IDialogController dialogController)
        {
            var musicBrainzDBName = configurationManager.GetConfiguration().MusicBrainzDatabaseName;
            var entities = modelController.GetAudioStationEntities<Mp3FileReference>();

            this.EntitiesStaged = new ObservableCollection<Mp3FileReference>(entities);

            this.RunImportCommand = new SimpleCommand(() =>
            {
                if (dialogController.ShowConfirmation("Download Music Brainz (Detail)",
                                                      "This process can take some time. Result data will be stored in database:",
                                                      "", musicBrainzDBName, "",
                                                      "Your audio library data will not be altered or disturbed.",
                                                      "Are you sure you want to do this?"))
                {
                    //libraryLoader.LoadLibraryAsync(this.Configuration.DirectoryBase);
                    //libraryLoader.Start();
                }
            });
        }
    }
}
