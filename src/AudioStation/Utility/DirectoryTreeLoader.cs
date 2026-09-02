using System.IO;

using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.Utilities;

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
        /// <param name="stopDepth">Recursion can be halted to simulate lazy loading. The stop depth of -1 will indicate no stop depth. Anything less will cause an argument exception.</param>
        /// <param name="path">Root directory</param>
        /// <param name="fileSearchPattern">File search pattern to filter file lookup</param>
        public static FileTreeViewModel Load(string path, string fileSearchPattern, int stopDepth)
        {
            return Load(path, fileSearchPattern, stopDepth, directory =>
            {
                return new FileTreeViewModel(fileSearchPattern, directory);

            }, (directory, fileCount) =>
            {
                return new FileTreeNodeViewModel(path, directory, fileCount);

            }, file =>
            {
                return new FileTreeNodeViewModel(path, file, 0);
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
        /// <param name="stopDepth">Recursion can be halted to simulate lazy loading. The stop depth of -1 will indicate no stop depth. Anything less will cause an argument exception.</param>
        /// <param name="directoryConstructor">Constructor to create directory node value</param>
        /// <param name="fileConstructor">Constructor to create file node value</param>
        public static TTree Load<TTree, TDirectory, TFile>(
               string path,
               string fileSearchPattern,
               int stopDepth,
               Func<TDirectory, TTree> treeConstructor,
               Func<string, int, TDirectory> directoryConstructor,
               Func<string, TFile> fileConstructor) where TDirectory : FileTreeNodeViewModel
                                                    where TFile : FileTreeNodeViewModel
                                                    where TTree : FileTreeViewModel
        {
            if (stopDepth < -1)
                throw new ArgumentException("Must have a stop depth of -1 or greater. Please set stop depth properly.");

            // Current Directory
            var fileData = BasicHelpers.FastGetFileData(path, fileSearchPattern, true, SearchOption.TopDirectoryOnly);
            var directoryFileCount = fileData.Count(x => !x.IsDirectory);

            // Directory (Root -> NodeValue)
            var rootValue = directoryConstructor(path, directoryFileCount);

            // File Tree (Recursive Node Container)
            var root = treeConstructor(rootValue);

            // Load to depth
            LoadToDepth(root, fileSearchPattern, stopDepth, treeConstructor, directoryConstructor, fileConstructor);

            return root;
        }

        /// <summary>
        /// Loads recursive directory tree for view model purposes
        /// </summary>
        /// <typeparam name="TTree">Type of tree (must inherit from RecursiveDispatcherViewModel)</typeparam>
        /// <typeparam name="TDirectory">Type of directory node</typeparam>
        /// <typeparam name="TFile">Type of file node</typeparam>
        /// <param name="directoryTree">Current or root directory</param>
        /// <param name="treeConstructor">Constructor to create tree node</param>
        /// <param name="stopDepth">Recursion can be halted to simulate lazy loading. The stop depth of -1 will indicate no stop depth. Must otherwise have a stop depth greater or equal to the directory tree</param>
        /// <param name="directoryConstructor">Constructor to create directory node value</param>
        /// <param name="fileConstructor">Constructor to create file node value</param>
        public static void LoadToDepth<TTree, TDirectory, TFile>(
               TTree directoryTree,
               string fileSearchPattern,
               int stopDepth,
               Func<TDirectory, TTree> treeConstructor,
               Func<string, int, TDirectory> directoryConstructor,
               Func<string, TFile> fileConstructor) where TDirectory : FileTreeNodeViewModel
                                                    where TFile : FileTreeNodeViewModel
                                                    where TTree : FileTreeViewModel
        {
            // Stop Depth
            if (stopDepth < -1)
                throw new ArgumentException("Must have a stop depth of -1 or greater. Please set stop depth properly.");

            else if (stopDepth < directoryTree.NodeValue.RecursionDepth && stopDepth != -1)
                throw new ArgumentException("Must have a stop depth of greater than or equal to the current directory. Please set stop depth properly.");

            try
            {
                // Recurse through files using a while loop
                var directories = new Stack<TTree>();

                // Start (stack)
                directories.Push(directoryTree);

                while (directories.Count > 0)
                {
                    var currentDirectory = directories.Pop();

                    // Recursion Stop Depth (Lazy Loading)
                    //
                    if (currentDirectory.NodeValue.RecursionDepth >= stopDepth)
                        break;

                    // Previously Loaded 
                    //
                    if (currentDirectory.NodeValue.IsLoaded)
                    {
                        // Load next directories to continue
                        foreach (var item in currentDirectory.Children)
                        {
                            if (item.NodeValue.IsDirectory)
                                directories.Push(item as TTree);
                        }

                        continue;
                    }


                    // Current Directory
                    var fileData = BasicHelpers.FastGetFileData(currentDirectory.NodeValue.FullPath, fileSearchPattern, true, SearchOption.TopDirectoryOnly);

                    foreach (var file in fileData)
                    {
                        //progressHandler(fileCount, fileIndex++, 0, "Loading Import Files");

                        // Directory (stack)
                        if (file.IsDirectory)
                        {
                            // Need file count for directory
                            var directoryData = BasicHelpers.FastGetFileData(file.Path, fileSearchPattern, true, SearchOption.TopDirectoryOnly);

                            // Next Directory
                            var nodeValue = directoryConstructor(file.Path, directoryData.Count(x => !x.IsDirectory));

                            // Current -> Next (adds parent)
                            var nextDirectory = currentDirectory.Add(nodeValue) as TTree;

                            // Push (NodeValue, Parent)
                            directories.Push(nextDirectory);
                        }

                        else
                            currentDirectory.Add(fileConstructor(file.Path));
                    }

                    // Current Directory: IsLoaded = true
                    currentDirectory.NodeValue.IsLoaded = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading files:  " + ex.Message);
            }
        }
    }
}
