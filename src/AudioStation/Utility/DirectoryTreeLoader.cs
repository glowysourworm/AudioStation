using System.IO;

using AudioStation.ViewModels.LibraryViewModels.LibraryTree;

using SimpleWpf.Native.IO;
using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.Utilities;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.Utility
{
    public static class DirectoryTreeLoader
    {
        /// <summary>
        /// Returns a new tree node for the tree based on user code
        /// </summary>
        /// <typeparam name="TTree">The type of node</typeparam>
        /// <param name="fileData">File data for the current file or directory</param>
        /// <param name="recursionDepth">Current depth of the tree for the new node</param>
        /// <param name="parent">Parent node</param>
        /// <returns>Newly constructed node for the tree</returns>
        public delegate TTree TreeNodeConstructor<TTree>(int recursionDepth, FastDirectoryResult fileData, int fileCountRecursive, TTree? parent) where TTree : FileTreeNodeViewModel;

        /// <summary>
        /// Loads recursive directory tree for view model purposes
        /// </summary>
        /// <typeparam name="TTree">Type of tree (must inherit from RecursiveDispatcherViewModel)</typeparam>
        /// <typeparam name="TDirectory">Type of directory node</typeparam>
        /// <typeparam name="TFile">Type of file node</typeparam>
        /// <param name="stopDepth">Recursion can be halted to simulate lazy loading. The stop depth of -1 will indicate no stop depth. Anything less will cause an argument exception.</param>
        /// <param name="path">Root directory</param>
        /// <param name="fileSearchPattern">File search pattern to filter file lookup</param>
        public static FileTreeViewModel Load(string path,
                                             int stopDepth,
                                             DialogProgressHandler? progressHandler = null,
                                             params string[] searchPatterns)
        {
            if (stopDepth < -1)
                throw new ArgumentException("Must have a stop depth of -1 or greater. Please set stop depth properly.");

            var fileTree = new FileTreeViewModel();

            Load<FileTreeViewModel, FileTreeNodeViewModel>(fileTree, path, stopDepth, (recursionDepth, fileData, fileCountRecursive, parent) =>
            {
                if (fileData.IsDirectory)
                    return new FileTreeNodeViewModel(recursionDepth, fileData.FullPath, fileCountRecursive, parent);
                else
                    return new FileTreeNodeViewModel(recursionDepth, fileData.FullPath, 0, parent);

            }, progressHandler, searchPatterns);

            return fileTree;
        }

        /// <summary>
        /// Loads recursive directory tree for view model purposes
        /// </summary>
        /// <typeparam name="TNode">Type of tree (must inherit from RecursiveDispatcherViewModel)</typeparam>
        /// <typeparam name="TDirectory">Type of directory node</typeparam>
        /// <typeparam name="TFile">Type of file node</typeparam>
        /// <param name="path">Root directory</param>
        /// <param name="fileSearchPatterns">File search pattern to filter file lookup</param>
        /// <param name="stopDepth">Recursion can be halted to simulate lazy loading. The stop depth of -1 will indicate no stop depth. Anything less will cause an argument exception.</param>
        /// <param name="treeConstructor">Constructor to create tree node value</param>
        public static void Load<TTree, TNode>(
               TTree fileTree,
               string path,
               int stopDepth,
               TreeNodeConstructor<TNode> treeConstructor,
               DialogProgressHandler? progressHandler = null,
               params string[] fileSearchPatterns) where TNode : FileTreeNodeViewModel
                                                   where TTree : FileTreeViewModel
        {
            if (stopDepth < -1)
                throw new ArgumentException("Must have a stop depth of -1 or greater. Please set stop depth properly.");

            // Begin Update:  Allows the user to add / remove nodes
            //
            fileTree.BeginUpdate();

            var fileData = BasicHelpers.FastGetFileData(path, true, SearchOption.TopDirectoryOnly, fileSearchPatterns);
            var fileCount = fileData.Count();

            foreach (var file in fileData)
            {
                if (file.IsDirectory)
                {
                    var currentFileCount = Directory.GetFiles(file.FullPath, "*", SearchOption.AllDirectories).Count();
                    var directoryNode = treeConstructor(0, file, currentFileCount, null);

                    fileTree.Add(directoryNode);
                }
                else
                {
                    var fileNode = treeConstructor(0, file, 0, null);

                    fileTree.Add(fileNode);
                }
            }

            fileTree.EndUpdate();

            // Load to depth
            LoadToDepth(fileTree, 1, stopDepth, treeConstructor, progressHandler, fileSearchPatterns);
        }

        /// <summary>
        /// Loads recursive directory tree for view model purposes
        /// </summary>
        /// <typeparam name="TNode">Type of tree (must inherit from RecursiveDispatcherViewModel)</typeparam>
        /// <param name="directoryTree">Current or root directory</param>
        /// <param name="stopDepth">Recursion can be halted to simulate lazy loading. The stop depth of -1 will indicate no stop depth. Must otherwise have a stop depth greater or equal to the directory tree</param>
        /// <param name="treeConstructor">Constructor to create node of the tree</param>
        public static void LoadToDepth<TTree, TNode>(
               TTree fileTree,
               int startDepth,
               int stopDepth,
               TreeNodeConstructor<TNode> treeConstructor,
               DialogProgressHandler? progressHandler = null,
               params string[] searchPatterns) where TNode : FileTreeNodeViewModel
                                               where TTree : FileTreeViewModel
        {
            // Start Depth
            if (startDepth < 1)
                throw new ArgumentException("Start depth must be at least 1. To initialize the tree you must first load the root(s).");

            if (startDepth > stopDepth && stopDepth > -1)
                throw new ArgumentException("Start depth must not exceed the stop depth");

            // Stop Depth
            if (stopDepth < -1)
                throw new ArgumentException("Must have a stop depth of -1 or greater. Please set stop depth properly.");

            else if (stopDepth < fileTree.Depth && stopDepth != -1)
                throw new ArgumentException("Must have a stop depth of greater than or equal to the current directory. Please set stop depth properly.");

            try
            {
                fileTree.BeginUpdate();

                // Recurse through files using a while loop
                var directories = new Stack<TNode>();

                // Start (stack) (the level prior to the start depth will be where to pick where the tree was last initialized)
                foreach (var node in fileTree.Collection.SimpleTreeWhere<TNode>(x => x.RecursionDepth == (startDepth - 1)))
                {
                    if (node.IsDirectory)
                        directories.Push(node);
                }


                while (directories.Count > 0)
                {
                    var currentDirectory = directories.Pop();

                    // Recursion Stop Depth (Lazy Loading)
                    //
                    if (currentDirectory.RecursionDepth >= stopDepth &&
                        stopDepth != -1)
                        break;

                    // Previously Loaded 
                    //
                    if (currentDirectory.IsLoaded)
                    {
                        // Load next directories to continue
                        foreach (var item in currentDirectory.Children.Cast<TNode>())
                        {
                            if (item.IsDirectory)
                                directories.Push(item);
                        }

                        continue;
                    }


                    // Current Directory (FILES ONLY)
                    var fileData = BasicHelpers.FastGetFileData(currentDirectory.FullPath, true, SearchOption.TopDirectoryOnly, searchPatterns);
                    var fileCount = fileData.Count();
                    var fileIndex = 0;

                    foreach (var file in fileData)
                    {
                        if (progressHandler != null)
                            progressHandler(1, 1, fileCount, fileIndex++, "Loading Import Files");

                        // Directory (stack)
                        if (file.IsDirectory)
                        {
                            // Need file count for directory
                            var directoryData = BasicHelpers.FastGetFileData(file.FullPath, true, SearchOption.TopDirectoryOnly, searchPatterns);

                            // Next Directory (current directory is parent)
                            var nextDirectory = treeConstructor(currentDirectory.RecursionDepth + 1, file, directoryData.Count(x => !x.IsDirectory), currentDirectory);

                            // Current -> Next (add to child list)
                            currentDirectory.Add(nextDirectory);

                            // Push (NodeValue, Parent)
                            directories.Push(nextDirectory);
                        }

                        // File
                        else
                        {
                            // Next File (current directory is parent)
                            var nextFile = treeConstructor(currentDirectory.RecursionDepth + 1, file, 0, currentDirectory);

                            // Current -> Next (add to child list)
                            currentDirectory.Add(nextFile);
                        }

                    }

                    // Current Directory: IsLoaded = true
                    currentDirectory.SetLoaded();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading files:  " + ex.Message);
            }
            finally
            {
                fileTree.EndUpdate();
            }
        }
    }
}
