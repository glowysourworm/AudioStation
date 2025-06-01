using System.Runtime.Serialization;

using AudioStation.Controller.Interface;
using AudioStation.Model.Command;
using AudioStation.ViewModels;

using Avalonia.Platform.Storage;

namespace AudioStation.Model
{
    public class LibraryConfiguration : ViewModelBase, ISerializable
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

        public LibraryConfiguration(IDialogController dialogController)
        {
            this.DirectoryBase = string.Empty;
            this.OpenLibraryFolderCommand = new ModelCommand(async () =>
            {
                this.DirectoryBase = await dialogController.ShowSelectFolder(new FolderPickerOpenOptions()
                {
                    AllowMultiple = false,
                    Title = "Select Library Folder"
                });
            });
        }

        public LibraryConfiguration(SerializationInfo info, StreamingContext context)
        {
            this.DirectoryBase = (string)info.GetValue("DirectoryBase", typeof(string));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("DirectoryBase", this.DirectoryBase);
        }
    }
}
