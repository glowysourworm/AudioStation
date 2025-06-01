using AudioStation.Controller.Interface;
using AudioStation.Model.Command;
using AudioStation.ViewModels;

using Avalonia.Platform.Storage;

using SimpleWpf.RecursiveSerializer.Component.Interface;
using SimpleWpf.RecursiveSerializer.Interface;

namespace AudioStation.Model
{
    public class LibraryConfiguration : ViewModelBase, IRecursiveSerializable
    {
        string _directoryBase;
        ModelCommand _openLibraryFolderCommand;

        public string DirectoryBase
        {
            get { return _directoryBase; }
            set { this.SetProperty(ref _directoryBase, value); }
        }
        public ModelCommand OpenLibraryFolderCommand
        {
            get { return _openLibraryFolderCommand; }
            set { this.SetProperty(ref _openLibraryFolderCommand, value); }
        }

        public LibraryConfiguration()
        {
            this.DirectoryBase = string.Empty;
            this.OpenLibraryFolderCommand = new ModelCommand(async () =>
            {
                this.DirectoryBase = await MainViewModel.DialogController.ShowSelectFolder(new FolderPickerOpenOptions()
                {
                    AllowMultiple = false,
                    Title = "Select Library Folder"
                });
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
