using System.IO;

using AudioStation.ViewModels;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.RecursiveSerializer.Component.Interface;
using SimpleWpf.RecursiveSerializer.Interface;

namespace AudioStation.Model
{
    public class Configuration : ViewModelBase, IRecursiveSerializable
    {
        public const string LIBRARY_DATABASE_FILE = ".AudioPlayerLibrary";

        string _libraryDatabaseFile;
        LibraryConfiguration _libraryConfiguration;

        SimpleCommand _selectLibraryDatabaseFileCommand;

        public LibraryConfiguration LibraryConfiguration
        {
            get { return _libraryConfiguration; }
            set { this.RaiseAndSetIfChanged(ref _libraryConfiguration, value); }
        }
        public string LibraryDatabaseFile
        {
            get { return _libraryDatabaseFile; }
            set { this.RaiseAndSetIfChanged(ref _libraryDatabaseFile, value); }
        }
        public SimpleCommand SelectLibraryDatabaseFileCommand
        {
            get { return _selectLibraryDatabaseFileCommand; }
            set { this.RaiseAndSetIfChanged(ref _selectLibraryDatabaseFileCommand, value); }
        }


        public Configuration()
        {
            this.LibraryConfiguration = new LibraryConfiguration();
            this.LibraryDatabaseFile = LIBRARY_DATABASE_FILE;
            this.SelectLibraryDatabaseFileCommand = new SimpleCommand(() =>
            {
                this.LibraryDatabaseFile = MainViewModel.DialogController.ShowSaveFile();
            });

            this.LibraryConfiguration.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DirectoryBase")
                {
                    this.LibraryDatabaseFile = Path.Combine(this.LibraryConfiguration.DirectoryBase, LIBRARY_DATABASE_FILE);
                }
            };
        }
        public Configuration(IPropertyReader reader)
        {
            this.LibraryConfiguration = reader.Read<LibraryConfiguration>("LibraryConfiguration");
            this.LibraryDatabaseFile = reader.Read<string>("LibraryDatabaseFile");
        }

        public void GetProperties(IPropertyWriter writer)
        {
            writer.Write("LibraryConfiguration", this.LibraryConfiguration);
            writer.Write("LibraryDatabaseFile", this.LibraryDatabaseFile);
        }
    }
}
