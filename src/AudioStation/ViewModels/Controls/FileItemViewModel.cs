using System.Collections.ObjectModel;
using System.IO;

using SimpleWpf.Extensions;
using SimpleWpf.NativeIO.FastDirectory;

namespace AudioStation.ViewModels.Controls
{
    public class FileItemViewModel : ViewModelBase
    {
        string _fullPath;
        string _fileNameOrDirectoryName;
        bool _isDirectory;
        bool _valid;

        public string FullPath
        {
            get { return _fullPath; }
            set { this.RaiseAndSetIfChanged(ref _fullPath, value); }
        }
        public string FileNameOrDirectoryName
        {
            get { return _fileNameOrDirectoryName; }
            set { this.RaiseAndSetIfChanged(ref _fileNameOrDirectoryName, value); }
        }
        public bool IsDirectory
        {
            get { return _isDirectory; }
            set { this.RaiseAndSetIfChanged(ref _isDirectory, value); }
        }
        public bool Valid
        {
            get { return _valid; }
            set { this.RaiseAndSetIfChanged(ref _valid, value); }
        }

        public ObservableCollection<FileItemViewModel> DirectoryFiles { get; private set; }

        public FileItemViewModel(string fullPath, bool isDirectory, string searchPattern)
        {
            Load(fullPath, fullPath, searchPattern, isDirectory);
        }

        private void Load(string fullPathBase, string fullPath, string searchPattern, bool isDirectory)
        {
            if (isDirectory)
            {
                if (Directory.Exists(fullPath))
                {
                    LoadImpl(fullPathBase, fullPath, searchPattern, isDirectory);
                }
                else
                {
                    this.DirectoryFiles = new ObservableCollection<FileItemViewModel>();
                    this.IsDirectory = isDirectory;
                    this.FullPath = "";
                    this.FileNameOrDirectoryName = "";
                    this.Valid = false;
                }
            }
            else
            {
                if (Path.Exists(fullPath))
                {
                    LoadImpl(fullPathBase, fullPath, searchPattern, isDirectory);
                }
                else
                {
                    this.DirectoryFiles = new ObservableCollection<FileItemViewModel>();
                    this.IsDirectory = isDirectory;
                    this.FullPath = "";
                    this.FileNameOrDirectoryName = "";
                    this.Valid = false;
                }
            }
        }

        private void LoadImpl(string fullPathBase, string fullPath, string searchPattern, bool isDirectory)
        {
            this.DirectoryFiles = new ObservableCollection<FileItemViewModel>();
            this.IsDirectory = isDirectory;
            this.FullPath = fullPath;

            if (isDirectory)
            {
                // Short Name
                var directoryName = Path.GetDirectoryName(fullPath) ?? "";
                this.FileNameOrDirectoryName = Path.GetRelativePath(directoryName, fullPathBase);

                // Recurse
                foreach (var path in FastDirectoryEnumerator.GetFiles(fullPath, searchPattern, SearchOption.AllDirectories))
                {
                    this.DirectoryFiles.Add(new FileItemViewModel(path.Path, path.Attributes.HasFlag(FileAttributes.Directory), searchPattern));
                }
            }
            else
            {
                this.FileNameOrDirectoryName = Path.GetFileName(fullPath);
            }
        }
    }
}
