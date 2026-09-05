using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Interface;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryViewModels;

namespace AudioStation.Component.Interface
{
    public interface IComponentViewModelLoader : IAudioStationPrimaryInitializer
    {
        /// <summary>
        /// Returns a component from the application's view model tree
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        T GetComponent<T>() where T : ComponentViewModelBase;

        /// <summary>
        /// Loads or re-initializes a component to prepare for work load
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        void LoadComponent<T>() where T : ComponentViewModelBase;

        /// <summary>
        /// Asynchronously loads or re-initializes a component to prepare for work load
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        Task LoadComponentAsync<T>() where T : ComponentViewModelBase;

        /// <summary>
        /// Loads a collection of LibraryEntryViewModel ordered by ID; and translates the PageResult into 
        /// one for the ViewModel namespace.
        /// </summary>
        PageResult<TrackViewModel> LoadEntryPage(PageRequest<Track, int> request);

        /// <summary>
        /// Converts any non-mp3 files to mp3 files and puts them in a special staging folder to be imported.
        /// </summary>
        Task ConvertFiles(IEnumerable<string> convertibleFiles, Action<double, string> progressCallback);
    }
}
