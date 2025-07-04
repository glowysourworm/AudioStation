﻿using System.Windows.Controls;

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
        [IocImportingConstructor]
        public MainView(IIocEventAggregator eventAggregator)
        {
            InitializeComponent();
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
