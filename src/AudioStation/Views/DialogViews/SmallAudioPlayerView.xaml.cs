using System.Windows;
using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.Controls;
using AudioStation.Event.DialogEvents;

using SimpleWpf.IocFramework.Application;

namespace AudioStation.Views.DialogViews
{
    public partial class SmallAudioPlayerView : UserControl
    {
        private readonly IAudioController _audioController;

        bool _sourceValid;
        bool _disposed;

        public SmallAudioPlayerView()
        {
            _audioController = IocContainer.Get<IAudioController>();

            InitializeComponent();

            this.Loaded += SmallAudioPlayerView_Loaded;
            this.Unloaded += SmallAudioPlayerView_Unloaded;

            _audioController.CurrentTimeUpdated += OnCurrentTimeUpdated;
        }

        private void SmallAudioPlayerView_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as DialogSmallAudioPlayerViewModel;

            if (viewModel != null)
            {
                _sourceValid = true;

                _audioController.Stop();
                _audioController.Load(viewModel.FileName, viewModel.SourceType);
                _audioController.Play();
            }
            else
            {
                _sourceValid = false;
                Dispose();
            }
        }

        private void SmallAudioPlayerView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_sourceValid)
            {
                Dispose();
            }
        }

        private void ScrubberControl_ScrubbedRatioChanged(ScrubberControl sender, float currentTimeRatio)
        {
            var viewModel = this.DataContext as DialogSmallAudioPlayerViewModel;

            if (_sourceValid && viewModel != null)
            {
                var positionMilliSeconds = viewModel.Duration.TotalMilliseconds * currentTimeRatio;

                _audioController.SetCurrentTime(TimeSpan.FromMilliseconds(positionMilliSeconds));
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_sourceValid)
            {
                _audioController.Pause();
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_sourceValid)
            {
                _audioController.Play();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_sourceValid)
            {
                _audioController.Stop();

                Dispose();
            }
        }

        private void OnCurrentTimeUpdated(TimeSpan currentTime)
        {
            var viewModel = this.DataContext as DialogSmallAudioPlayerViewModel;

            if (viewModel != null)
            {
                viewModel.CurrentTime = currentTime;
            }
        }

        private void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _audioController.Stop();
                _audioController.CurrentTimeUpdated -= OnCurrentTimeUpdated;
            }
        }
    }
}
