using System.Windows;
using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
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

        SimpleCommand _importM3UCommand;
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
        public SimpleCommand ImportM3UCommand
        {
            get { return _importM3UCommand; }
            set { this.RaiseAndSetIfChanged(ref _importM3UCommand, value); }
        }

        [IocImportingConstructor]
        public RadioViewModel(ILibraryLoader libraryLoader,
                              IOutputController outputController,
                              IDialogController dialogController)
        {
            _outputController = outputController;

            this.RadioEntries = new SortedObservableCollection<RadioEntryViewModel>(new PropertyComparer<string, RadioEntryViewModel>(x => x.Name));
            this.RadioBrowserSearchResults = new SortedObservableCollection<RadioStationViewModel>(new PropertyComparer<string, RadioStationViewModel>(x => x.Name));

            // Radio Browser (service) Search
            this.SearchRadioBrowserCommand = new SimpleCommand<string>(SearchRadioBrowser);
            this.ImportM3UCommand = new SimpleCommand(() =>
            {
                var directory = dialogController.ShowSelectFolder();

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    //libraryLoader.LoadRadioAsync(directory);
                    //libraryLoader.Start();
                }
            });
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
                        LogoEndpoint = stream.Favicon
                    });
                }
            }
            catch (Exception ex)
            {
                _outputController.Log("Error querying Radio Browser:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }
        }

        #region ILibraryLoader Events (some of these events are from the worker thread)
        private void OnRadioEntryLoaded(RadioEntry radioEntry)
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(OnRadioEntryLoaded, DispatcherPriority.ApplicationIdle, radioEntry);

            else if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
            {
                var entry = this.RadioEntries.FirstOrDefault(item => item.Name == radioEntry.Name);

                // Add
                if (entry == null)
                {
                    entry = new RadioEntryViewModel()
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

                // Update
                else
                {
                    foreach (var station in radioEntry.Streams)
                    {
                        var radioStation = entry.Stations.FirstOrDefault(x => x.Name == station.Name);

                        if (radioStation == null)
                        {
                            radioStation = new RadioStationViewModel()
                            {
                                Bitrate = station.Bitrate,
                                Codec = station.Codec,
                                Name = station.Name,
                                Endpoint = station.Endpoint,
                                Homepage = station.Homepage,
                                LogoEndpoint = station.LogoEndpoint
                            };

                            entry.Stations.Add(radioStation);
                        }
                        else
                        {
                            radioStation.Homepage = string.IsNullOrEmpty(radioStation.Homepage) ? station.Homepage : radioStation.Homepage;
                            radioStation.LogoEndpoint = string.IsNullOrEmpty(radioStation.LogoEndpoint) ? station.LogoEndpoint : radioStation.LogoEndpoint;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
