using System.IO;

using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.Utilities;

using static AudioStation.Event.DialogEventHandlers;

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
        public static FileTreeNodeViewModel Load(string path,
                                             int stopDepth,
                                             DialogProgressHandler? progressHandler = null,
                                             params string[] searchPatterns)
        {
            return Load<FileTreeNodeViewModel>(path, stopDepth, (baseDirectory, fullPath, fileCount, parent) =>
            {
                return new FileTreeNodeViewModel(baseDirectory, fullPath, fileCount, parent);

            }, progressHandler, searchPatterns);
        }

        /// <summary>
        /// Loads recursive directory tree for view model purposes
        /// </summary>
        /// <typeparam name="TTree">Type of tree (must inherit from RecursiveDispatcherViewModel)</typeparam>
        /// <typeparam name="TDirectory">Type of directory node</typeparam>
        /// <typeparam name="TFile">Type of file node</typeparam>
        /// <param name="path">Root directory</param>
        /// <param name="fileSearchPatterns">File search pattern to filter file lookup</param>
        /// <param name="stopDepth">Recursion can be halted to simulate lazy loading. The stop depth of -1 will indicate no stop depth. Anything less will cause an argument exception.</param>
        /// <param name="treeConstructor">Constructor to create tree node value</param>
        public static TTree Load<TTree>(
               string path,
               int stopDepth,
               Func<string, string, int, TTree, TTree> treeConstructor,
               DialogProgressHandler? progressHandler = null,
               params string[] fileSearchPatterns) where TTree : FileTreeNodeViewModel
        {
            if (stopDepth < -1)
                throw new ArgumentException("Must have a stop depth of -1 or greater. Please set stop depth properly.");

            // Current Directory
            var fileData = BasicHelpers.FastGetFileData(path, true, SearchOption.TopDirectoryOnly, fileSearchPatterns);
            var directoryFileCount = fileData.Count(x => !x.IsDirectory);

            // Directory (Root -> null)
            var root = treeConstructor(path, path, directoryFileCount, null);

            // Load to depth
            LoadToDepth(root, stopDepth, treeConstructor, progressHandler, fileSearchPatterns);

            return root;
        }

        /// <summary>
        /// Loads recursive directory tree for view model purposes
        /// </summary>
        /// <typeparam name="TTree">Type of tree (must inherit from RecursiveDispatcherViewModel)</typeparam>
        /// <param name="directoryTree">Current or root directory</param>
        /// <param name="stopDepth">Recursion can be halted to simulate lazy loading. The stop depth of -1 will indicate no stop depth. Must otherwise have a stop depth greater or equal to the directory tree</param>
        /// <param name="treeConstructor">Constructor to create node of the tree</param>
        public static void LoadToDepth<TTree>(
               TTree directoryTree,
               int stopDepth,
               Func<string, string, int, TTree, TTree> treeConstructor,
               DialogProgressHandler? progressHandler = null,
               params string[] searchPatterns) where TTree : FileTreeNodeViewModel
        {
            // Stop Depth
            if (stopDepth < -1)
                throw new ArgumentException("Must have a stop depth of -1 or greater. Please set stop depth properly.");

            else if (stopDepth < directoryTree.RecursionDepth && stopDepth != -1)
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
                    if (currentDirectory.RecursionDepth >= stopDepth &&
                        stopDepth != -1)
                        break;

                    // Previously Loaded 
                    //
                    if (currentDirectory.IsLoaded)
                    {
                        // Load next directories to continue
                        foreach (var item in currentDirectory.Children.Cast<TTree>())
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
                            var nextDirectory = treeConstructor(directoryTree.BaseDirectory, file.FullPath, directoryData.Count(x => !x.IsDirectory), currentDirectory);

                            // Current -> Next (add to child list)
                            currentDirectory.Add(nextDirectory);

                            // Push (NodeValue, Parent)
                            directories.Push(nextDirectory);
                        }

                        // File
                        else
                        {
                            // Next File (current directory is parent)
                            var nextFile = treeConstructor(directoryTree.BaseDirectory, file.FullPath, 0, currentDirectory);

                            // Current -> Next (add to child list)
                            currentDirectory.Add(nextFile);
                        }

                    }

                    // Current Directory: IsLoaded = true
                    currentDirectory.IsLoaded = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading files:  " + ex.Message);
            }
        }
    }
}
