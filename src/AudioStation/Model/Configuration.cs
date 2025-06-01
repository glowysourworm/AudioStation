using System;
using System.IO;
using System.Runtime.Serialization;

using AudioStation.Controller.Interface;
using AudioStation.Model.Command;
using AudioStation.ViewModels;
using AudioStation.Views;

using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AudioStation.Model
{
    public class Configuration : ViewModelBase, ISerializable
    {
        public const string LIBRARY_DATABASE_FILE = ".AudioPlayerLibrary";

        string _libraryDatabaseFile;
        LibraryConfiguration _libraryConfiguration;

        ModelCommand _selectLibraryDatabaseFileCommand;

        public LibraryConfiguration LibraryConfiguration
        {
            get { return _libraryConfiguration; }
            set { this.SetProperty(ref _libraryConfiguration, value); }
        }
        public string LibraryDatabaseFile
        {
            get { return _libraryDatabaseFile; }
            set { this.SetProperty(ref _libraryDatabaseFile, value); }
        }
        public ModelCommand SelectLibraryDatabaseFileCommand
        {
            get { return _selectLibraryDatabaseFileCommand; }
            set { this.SetProperty(ref _selectLibraryDatabaseFileCommand, value); }
        }


        public Configuration(IDialogController dialogController)
        {
            this.LibraryConfiguration = new LibraryConfiguration(dialogController);
            this.LibraryDatabaseFile = LIBRARY_DATABASE_FILE;
            this.SelectLibraryDatabaseFileCommand = new ModelCommand(async () =>
            {
                this.LibraryDatabaseFile = await dialogController.ShowSaveFile(new FilePickerSaveOptions()
                {
                    Title = "Save Library Database",
                    SuggestedFileName = LIBRARY_DATABASE_FILE                    
                });
            });

            this.LibraryConfiguration.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DirectoryBase")
                {
                    this.LibraryDatabaseFile = Path.Combine(this.LibraryConfiguration.DirectoryBase, LIBRARY_DATABASE_FILE);
                }
            };
        }
        public Configuration(SerializationInfo info, StreamingContext context)
        {
            this.LibraryConfiguration = (LibraryConfiguration)info.GetValue("LibraryConfiguration", typeof(LibraryConfiguration));
            this.LibraryDatabaseFile = (string)info.GetValue("LibraryDatabaseFile", typeof(string));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("LibraryConfiguration", this.LibraryConfiguration);
            info.AddValue("LibraryDatabaseFile", this.LibraryDatabaseFile);
        }
    }
}
