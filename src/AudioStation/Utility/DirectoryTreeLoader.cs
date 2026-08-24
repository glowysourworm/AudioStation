using System.IO;

using AudioStation.ViewModels.OtherViewModels;

using SimpleWpf.Utilities;
using SimpleWpf.ViewModel;

namespace AudioStation.Utility
{
    public static class DirectoryTreeLoader
    {
        /// <summary>
        /// Loads recursive directory tree for view model purposes
        /// </summary>
        /// <typeparam name="TTree">Type of tree (must inherit from RecursiveDispatcherViewModel)</typeparam>
        /// <typeparam name="TDirectory">Type of directory node</typeparam>
        /// <typeparam name="TFile">Type of file node</typeparam>
        /// <param name="path">Root directory</param>
        /// <param name="fileSearchPattern">File search pattern to filter file lookup</param>
        public static DirectoryTreeViewModel Load(string path, string fileSearchPattern)
        {
            return Load<DirectoryTreeViewModel, PathViewModel, PathViewModel>(path, fileSearchPattern, directory =>
            {
                return new DirectoryTreeViewModel(directory);

            }, directory =>
            {
                return new PathViewModel(path, directory);

            }, file =>
            {
                return new PathViewModel(path, file);
            });
        }

        /// <summary>
        /// Loads recursive directory tree for view model purposes
        /// </summary>
        /// <typeparam name="TTree">Type of tree (must inherit from RecursiveDispatcherViewModel)</typeparam>
        /// <typeparam name="TDirectory">Type of directory node</typeparam>
        /// <typeparam name="TFile">Type of file node</typeparam>
        /// <param name="path">Root directory</param>
        /// <param name="fileSearchPattern">File search pattern to filter file lookup</param>
        /// <param name="treeConstructor">Constructor to create tree node</param>
        /// <param name="directoryConstructor">Constructor to create directory node value</param>
        /// <param name="fileConstructor">Constructor to create file node value</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static TTree Load<TTree, TDirectory, TFile>(
            string path,
            string fileSearchPattern,
            Func<TDirectory, TTree> treeConstructor,
            Func<string, TDirectory> directoryConstructor,
            Func<string, TFile> fileConstructor) where TDirectory : PathViewModel
                                                 where TFile : PathViewModel
                                                 where TTree : RecursiveDispatcherViewModel<PathViewModel>
        {
            try
            {
                // Directory (Root -> NodeValue)
                var rootValue = directoryConstructor(path);

                // File Tree (Recursive Node Container)
                var root = treeConstructor(rootValue);

                // Recurse through files using a while loop
                var directories = new Stack<TTree>();

                // Start (stack)
                directories.Push(root);

                while (directories.Count > 0)
                {
                    var currentDirectory = directories.Pop();

                    // Current Directory
                    var fileData = BasicHelpers.FastGetFileData(currentDirectory.NodeValue.FullPath, fileSearchPattern, true, SearchOption.TopDirectoryOnly);
                    var fileCount = fileData.Count();
                    var fileIndex = 0;

                    foreach (var file in fileData)
                    {
                        //progressHandler(fileCount, fileIndex++, 0, "Loading Import Files");

                        // Directory (stack)
                        if (file.IsDirectory)
                        {
                            // Next Directory
                            var nodeValue = directoryConstructor(file.Path);

                            // Current -> Next (adds parent)
                            var nextDirectory = currentDirectory.Add(nodeValue) as TTree;

                            // Push (NodeValue, Parent)
                            directories.Push(nextDirectory);
                        }

                        else
                            currentDirectory.Add(fileConstructor(file.Path));
                    }
                }

                return root;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading files:  " + ex.Message);
            }
        }
    }
}
