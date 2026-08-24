using System.Collections.ObjectModel;

using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.EventHandler;
using AudioStation.ViewModels.ComponentViewModels.LoadViewModels;

using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels.ComponentViewModels
{
    [IocExportDefault]
    public class LibraryLoaderMusicBrainzViewModel : ComponentViewModelBase<NoViewModel>
    {
        ObservableCollection<Track> _entitiesStaged;

        SimpleCommand _runImportCommand;

        public ObservableCollection<Track> EntitiesStaged
        {
            get { return _entitiesStaged; }
            set { this.RaiseAndSetIfChanged(ref _entitiesStaged, value); }
        }
        public SimpleCommand RunImportCommand
        {
            get { return _runImportCommand; }
            set { this.RaiseAndSetIfChanged(ref _runImportCommand, value); }
        }

        public override NoViewModel? Load { get; }

        [IocImportingConstructor]
        public LibraryLoaderMusicBrainzViewModel(IModelController modelController,
                                                         IConfigurationManager configurationManager,
                                                         IDialogController dialogController)
        {
            var musicBrainzDBName = configurationManager.GetConfiguration().MusicBrainzDatabaseName;
            var entities = modelController.GetAudioStationEntities<Track>();

            this.EntitiesStaged = new ObservableCollection<Track>(entities);

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

        public override void Initialize(Configuration configuration, NoViewModel load, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
