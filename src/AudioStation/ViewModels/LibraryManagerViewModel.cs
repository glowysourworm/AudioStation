using System.Collections.ObjectModel;
using System.Windows;

using AudioStation.Component;
using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.EventHandler;
using AudioStation.Model;
using AudioStation.ViewModels.LibraryManagerViewModels;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels
{
    public class LibraryManagerViewModel : PrimaryViewModelBase
    {
        private readonly IViewModelLoader _viewModelLoader;

        LibraryViewModel _library;

        ObservableCollection<string> _nonConvertedFiles;

        SimpleCommand _convertCommand;

        public LibraryViewModel Library
        {
            get { return _library; }
            set { this.RaiseAndSetIfChanged(ref _library, value); }
        }

        public ObservableCollection<string> NonConvertedFiles
        {
            get { return _nonConvertedFiles; }
            set { this.RaiseAndSetIfChanged(ref _nonConvertedFiles, value); }
        }

        public SimpleCommand ConvertCommand
        {
            get { return _convertCommand; }
            set { this.RaiseAndSetIfChanged(ref _convertCommand, value); }
        }

        public LibraryManagerViewModel(IViewModelLoader viewModelLoader, IIocEventAggregator eventAggregator)
        {
            _viewModelLoader = viewModelLoader;

            this.Library = new LibraryViewModel(viewModelLoader);
            this.NonConvertedFiles = new ObservableCollection<string>();

            this.ConvertCommand = new SimpleCommand(async () =>
            {
                var dialogViewModel = new DialogLoadingViewModel()
                {
                    Title = "Converting Files",
                    Progress = 0,
                    ShowProgressBar = true
                };

                // Dialog Show
                eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData(dialogViewModel));

                // Convert...
                await viewModelLoader.ConvertFiles(this.NonConvertedFiles, (progress, fileName) =>
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        dialogViewModel.Progress = progress;
                        dialogViewModel.Message = fileName;
                    });
                });

                // Dialog Hide
                eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());

                this.NonConvertedFiles.Clear();
                this.NonConvertedFiles.AddRange(viewModelLoader.LoadNonConvertedFiles());
            });
        }

        public override void Initialize(DialogProgressHandler progressHandler)
        {
            try
            {
                this.NonConvertedFiles.AddRange(_viewModelLoader.LoadNonConvertedFiles());

                // Load Artists / Albums / Genres
                this.Library.Initialize(progressHandler);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading non-converted files:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                this.NonConvertedFiles.Clear();
            }
        }

        public override void Dispose()
        {
            
        }
    }
}
