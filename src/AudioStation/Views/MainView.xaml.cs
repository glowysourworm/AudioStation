using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.Core.Model;
using AudioStation.Event;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class MainView : UserControl
    {
        private readonly IDialogController _dialogController;

        [IocImportingConstructor]
        public MainView(IIocEventAggregator eventAggregator, IDialogController dialogController)
        {
            _dialogController = dialogController;

            InitializeComponent();

            eventAggregator.GetEvent<MainLoadingChangedEvent>().Subscribe(OnMainLoadingChanged, IocEventPriority.Low);
        }

        private void OnMainLoadingChanged(MainLoadingChangedEventData data)
        {
            switch (data.RequestedView)
            {
                case NavigationView.None:
                    break;
                case NavigationView.Configuration:
                    this.ConfigurationTab.IsSelected = true;
                    break;
                case NavigationView.LibraryLoader:
                    this.LibraryLoaderTab.IsSelected = true;
                    break;
                case NavigationView.LibraryManager:
                    this.LibraryManagerTab.IsSelected = true;
                    break;
                case NavigationView.ArtistSearch:
                    this.ArtistSearchTab.IsSelected = true;
                    break;
                case NavigationView.NowPlaying:
                    this.NowPlayingTab.IsSelected = true;
                    break;
                case NavigationView.Radio:
                    this.RadioTab.IsSelected = true;
                    break;
                case NavigationView.RadioBrowser:
                    this.RadioBrowserTab.IsSelected = true;
                    break;
                case NavigationView.Bandcamp:
                    this.BandcampTab.IsSelected = true;
                    break;
                case NavigationView.Log:
                {
                    var viewModel = this.DataContext as MainViewModel;
                    if (viewModel != null)
                    {
                        _dialogController.ShowLogWindow(viewModel.Log);
                    }
                }                
                break;
                default:
                    throw new Exception("Unhandled navigation request view:  MainView.cs");
            }
        }

        private void OnShowOutputMessagesClick(object sender, System.Windows.RoutedEventArgs e)
        {
            
        }

        private void RadioBrowserView_StartStationEvent(ViewModels.RadioViewModels.RadioStationViewModel sender)
        {
            //var nowPlaying = new NowPlayingViewModel()
            //{
            //    Bitrate = sender.Bitrate,
            //    Codec = sender.Codec,
            //    Title = sender.Name,
            //    Source = sender.Endpoint,
            //    SourceType = StreamSourceType.Network
            //};

            //MainViewModel.AudioController.Play(nowPlaying);
        }
    }
}
