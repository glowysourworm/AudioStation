using System.Collections.ObjectModel;
using System.Windows;

using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.ViewModels.LibraryManagerViewModels;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels
{
    [IocExportDefault]
    public class LibraryManagerViewModel : ViewModelBase
    {
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

        [IocImportingConstructor]
        public LibraryManagerViewModel(IViewModelLoader viewModelLoader, IIocEventAggregator eventAggregator)
        {
            this.Library = new LibraryViewModel(viewModelLoader);
            this.NonConvertedFiles = new ObservableCollection<string>(viewModelLoader.LoadNonConvertedFiles());     // TODO: Initialize Pattern!

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
    }
}
