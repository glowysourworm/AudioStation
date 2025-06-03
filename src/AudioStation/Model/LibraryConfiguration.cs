using AudioStation.ViewModels;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.RecursiveSerializer.Component.Interface;
using SimpleWpf.RecursiveSerializer.Interface;

namespace AudioStation.Model
{
    public class LibraryConfiguration : ViewModelBase, IRecursiveSerializable
    {
        string _directoryBase;
        SimpleCommand _openLibraryFolderCommand;

        public string DirectoryBase
        {
            get { return _directoryBase; }
            set { this.RaiseAndSetIfChanged(ref _directoryBase, value); }
        }
        public SimpleCommand OpenLibraryFolderCommand
        {
            get { return _openLibraryFolderCommand; }
            set { this.RaiseAndSetIfChanged(ref _openLibraryFolderCommand, value); }
        }

        public LibraryConfiguration()
        {
            this.DirectoryBase = string.Empty;
            this.OpenLibraryFolderCommand = new SimpleCommand(() =>
            {
                this.DirectoryBase = MainViewModel.DialogController.ShowSelectFolder();
            });
        }

        public LibraryConfiguration(IPropertyReader reader)
        {
            this.DirectoryBase = reader.Read<string>("DirectoryBase");
        }
        public void GetProperties(IPropertyWriter writer)
        {
            writer.Write("DirectoryBase", this.DirectoryBase);
        }
    }
}
