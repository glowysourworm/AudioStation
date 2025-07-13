using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad
{
    public class LibraryLoaderEntityLoad : LibraryLoaderLoadBase
    {
        public AudioStationEntityBase Entity { get; private set; }

        public LibraryLoaderEntityLoad(AudioStationEntityBase entity)
        {
            this.Entity = entity;
        }
    }
}
