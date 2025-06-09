using System.Windows;
using System.Windows.Threading;

using AudioStation.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Model;
using AudioStation.ViewModels.LibraryViewModels.Comparer;
using AudioStation.ViewModels.RadioViewModels;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels
{
    [IocExportDefault]
    public class RadioViewModel : ViewModelBase
    {
        private readonly IOutputController _outputController;

        // Our Primary Library Collections
        SortedObservableCollection<RadioEntryViewModel> _radioEntries;
        SortedObservableCollection<RadioStationViewModel> _radioBrowserSearchResults;

        SimpleCommand<string> _searchRadioBrowserCommand;

        public SortedObservableCollection<RadioEntryViewModel> RadioEntries
        {
            get { return _radioEntries; }
            set { this.RaiseAndSetIfChanged(ref _radioEntries, value); }
        }
        public SortedObservableCollection<RadioStationViewModel> RadioBrowserSearchResults
        {
            get { return _radioBrowserSearchResults; }
            set { this.RaiseAndSetIfChanged(ref _radioBrowserSearchResults, value); }
        }
        public SimpleCommand<string> SearchRadioBrowserCommand
        {
            get { return _searchRadioBrowserCommand; }
            set { this.RaiseAndSetIfChanged(ref _searchRadioBrowserCommand, value); }
        }

        [IocImportingConstructor]
        public RadioViewModel(ILibraryLoader libraryLoader, IOutputController outputController)
        {
            _outputController = outputController;

            this.RadioEntries = new SortedObservableCollection<RadioEntryViewModel>(new PropertyComparer<string, RadioEntryViewModel>(x => x.Name));
            this.RadioBrowserSearchResults = new SortedObservableCollection<RadioStationViewModel>(new PropertyComparer<string, RadioStationViewModel>(x => x.Name));

            // Library Loader
            libraryLoader.RadioEntryLoaded += OnRadioEntryLoaded;

            // Radio Browser (service) Search
            this.SearchRadioBrowserCommand = new SimpleCommand<string>(SearchRadioBrowser);
        }

        public async void SearchRadioBrowser(string search)
        {
            try
            {
                var streams = await RadioBrowserSearchComponent.SearchStation(search);
                //var streams = await RadioBrowserSearchComponent.GetTopStations(10);

                if (streams.Count == 0)
                    return;

                this.RadioBrowserSearchResults.Clear();

                foreach (var stream in streams)
                {
                    if (stream == null || stream.Bitrate <= 0 || string.IsNullOrEmpty(stream.Codec))
                        continue;

                    this.RadioBrowserSearchResults.Add(new RadioStationViewModel()
                    {
                        Bitrate = stream.Bitrate,
                        Codec = stream.Codec,
                        Name = stream.Name,
                        Homepage = stream.Homepage.ToString(),
                        Endpoint = stream.Url.ToString(),
                        LogoEndpoint = stream.Favicon.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error querying Radio Browser:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }
        }

        #region ILibraryLoader Events (some of these events are from the worker thread)
        private void OnRadioEntryLoaded(RadioEntry radioEntry)
        {
            if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(OnRadioEntryLoaded, DispatcherPriority.ApplicationIdle, radioEntry);
            else
            {
                if (!this.RadioEntries.Any(item => item.Name == radioEntry.Name))
                {
                    var entry = new RadioEntryViewModel()
                    {
                        Name = radioEntry.Name,
                    };

                    entry.Stations.AddRange(radioEntry.Streams.Select(x => new RadioStationViewModel()
                    {
                        Bitrate = x.Bitrate,
                        Codec = x.Codec,
                        Name = x.Name,
                        Endpoint = x.Endpoint,
                        Homepage = x.Homepage,
                        LogoEndpoint = x.LogoEndpoint

                    }));

                    this.RadioEntries.Add(entry);
                }
                else
                {
                    _outputController.AddLog("Radio Station already exists! {0}", LogMessageType.General, LogLevel.Error, radioEntry.Name);
                }
            }
        }
        #endregion
    }
}
