using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.EventHandler;

namespace AudioStation.ViewModels.ComponentViewModels
{
    public class LibraryLoaderViewModel : ComponentViewModelBase
    {
        public LibraryLoaderViewModel(string displayName) : base(displayName)
        {
        }

        protected override void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationViewModelController viewModelController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }

        protected override void LoadImpl(IAudioStationConfiguration configuration, IComponentViewModelLoader viewModelLoader, DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }
    }
}
