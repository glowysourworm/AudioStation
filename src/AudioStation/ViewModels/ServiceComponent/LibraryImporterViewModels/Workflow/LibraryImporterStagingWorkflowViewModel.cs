using System.ComponentModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.Vendor.AcoustIDViewModel;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application;
using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels.Workflow
{
    public class LibraryImporterStagingWorkflowViewModel : ServiceComponentPartViewModelBase
    {
        private IAudioStationMapper _audioStationMapper;
        private IAudioStationDbClient _audioStationDbClient;
        private ITagCache _tagCache;

        // Workflow Configuration
        private LibraryImporterConfigurationViewModel _workflowConfiguration;

        SimpleCommand _stageCommand;
        SimpleCommand _unstageCommand;

        // Import Source Directory
        //
        FileTreeViewModel _importDirectory;

        // Staged Files:  These will keep changes to the tag in memory until the tag data is saved (to
        //                the same file in the source direcotry. An "import" is complete when the file 
        //                finished - with the bare minimum tag data - and moved into the library's 
        //                directory structure.
        //
        LibraryImporterFileTreeViewModel _stagedFiles;
        LibraryImporterStagedFileFilterType _stagedFileFilterType;

        public FileTreeViewModel ImportDirectory
        {
            get { return _importDirectory; }
            set { this.RaiseAndSetIfChanged(ref _importDirectory, value); }
        }
        public LibraryImporterFileTreeViewModel StagedFiles
        {
            get { return _stagedFiles; }
            set { this.RaiseAndSetIfChanged(ref _stagedFiles, value); }
        }
        public LibraryImporterStagedFileFilterType StagedFileFilterType
        {
            get { return _stagedFileFilterType; }
            set { this.RaiseAndSetIfChanged(ref _stagedFileFilterType, value); }
        }

        public SimpleCommand StageCommand
        {
            get { return _stageCommand; }
            set { this.RaiseAndSetIfChanged(ref _stageCommand, value); }
        }
        public SimpleCommand UnstageCommand
        {
            get { return _unstageCommand; }
            set { this.RaiseAndSetIfChanged(ref _unstageCommand, value); }
        }

        public LibraryImporterStagingWorkflowViewModel(IDialogController dialogController, LibraryImporterConfigurationViewModel workflowConfiguration)
            : base("Library Staging", "This workflow component will be used for staging files")
        {
            _workflowConfiguration = workflowConfiguration;

            this.StagedFiles = new LibraryImporterFileTreeViewModel();
            this.ImportDirectory = new FileTreeViewModel();

            this.ImportDirectory.TreeSelectionChangedEvent += ImportDirectory_TreeSelectionChangedEvent;
            this.StagedFiles.TreeSelectionChangedEvent += StagedFiles_TreeSelectionChangedEvent;

            // Bubble Up Events
            this.ImportDirectory.PropertyChanged += OnBubbleUpUIEvent;
            this.StagedFiles.PropertyChanged += OnBubbleUpUIEvent;

            this.StageCommand = new SimpleCommand(() => Stage(dialogController), CanStage);
            this.UnstageCommand = new SimpleCommand(Unstage, CanUnstage);
        }

        public void Stage(IDialogController dialogController)
        {
            dialogController.ShowLoading("Staging Files", Execute);
        }
        public void Unstage()
        {
            this.StagedFiles.Remove(x => x.IsSelected);
        }
        public bool CanStage()
        {
            return this.ImportDirectory.SelectedCount > 0;
        }
        public bool CanUnstage()
        {
            return this.StagedFiles.SelectedCount > 0;
        }

        public override bool CanExecute()
        {
            return CanStage();
        }
        public override bool CanLoad()
        {
            return !this.Loaded;
        }
        public override bool CanReset()
        {
            // This might where to put "UnStage", but also, just clear out staged files
            return true;
        }
        public override void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            _audioStationDbClient = audioStationController.ServiceController.GetDataService<IAudioStationDbClient>();
            _audioStationMapper = IocContainer.Get<IAudioStationMapper>();
            _tagCache = audioStationController.ServiceController.GetCache<ITagCache>();

            var audioConverter = IocContainer.Get<IAudioConverter>();

            // Multiple File Search
            var searchPattern = audioConverter.GetSupportedFormatExtensions()
                                              .Select(x => "*" + x)
                                              .ToArray();

            // Import Directory:  1) Not Initialized; or 2) A different directory
            //
            var libraryLoaderService = IocContainer.Get<ILibraryLoaderService>();
            var directory = (_workflowConfiguration.ImportType == Core.Model.LibraryImportType.Migration) ? _workflowConfiguration.MigrationSourceDirectory :
                                                                                                            _workflowConfiguration.ImportDirectory.Directory;
            // Clear Staged
            this.StagedFiles.BeginUpdate();
            this.StagedFiles.Clear();
            this.StagedFiles.EndUpdate();

            this.ImportDirectory.BeginUpdate();

            var treeRoot = libraryLoaderService.InitializeImporterTree(directory, _workflowConfiguration, progressHandler, searchPattern);

            this.ImportDirectory.Add(treeRoot);
            this.ImportDirectory.EndUpdate();
        }

        public override void Execute(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (this.ImportDirectory.SelectedNodes == null)
                throw new ArgumentException("Import selected nodes aren't being forwarded from the view");

            // Block Events
            this.StagedFiles.BeginUpdate();

            progressHandler(1, 1, 1, 0, "Loading Library Files");

            // Library Files
            var libraryFiles = _audioStationDbClient.GetEntities<FileReference>().ToDictionary(x => x.FileName);

            // Tag Records (linked to FileReference)
            var tagFileMaps = _audioStationDbClient.GetEntities<TagSmallFileReferenceMap>().ToDictionary(x => x.FileReference.FileName);

            // AcoustID Results
            var acoustIDResults = _audioStationDbClient.GetEntities<AcoustIDLookupResult>()
                                                       .GroupBy(x => x.FileName)
                                                       .ToDictionary(x => x.Key, x => x.ToList());

            // MusitBrainz-AcoustID View
            var musicBrainzAcoustIDResults = _audioStationDbClient.GetViewEntities<MusicBrainzAcoustIDResult>()
                                                                  .GroupBy(x => x.FileName)
                                                                  .ToDictionary(x => x.Key, x => x.ToList());

            // Music Brainz (basic) (TagSmallVendorMap)
            var musicBrainzResults = _audioStationDbClient.GetEntities<TagSmallVendorMap>()
                                                          .Where(x => x.MusicBrainzRecordingId != null)
                                                          .ToDictionary(x => x.TagSmallId, x => x);

            // Procedure:  We must take branches of the other tree view and 
            //             create the staged file tree. Selected Items have
            //             already been forwarded by the view.
            //              
            // Add Node:   The nodes must be added in order starting with 
            //             the root. The tree view model looks for the matching
            //             parent node by key reference.
            //
            // Directories:  Each node is treated as part of the tree. Directories
            //               are just going to be placeholders unless there is
            //               some need for them.
            //
            var selectedFileCount = this.ImportDirectory.SelectedNodes.Count(x => x.IsSelected);
            var counter = 0;

            foreach (var selectedItem in this.ImportDirectory
                                             .SelectedNodes
                                             .OrderBy(x => x.RecursionDepth)
                                             .Cast<FileTreeNodeViewModel>())
            {
                progressHandler(1, 1, selectedFileCount, counter++, "Loading:  " + selectedItem.FullPath);

                // Contains (by key)
                if (this.StagedFiles.Contains(selectedItem))
                    continue;

                // Include Ancestors
                if (selectedItem.Parent != null &&
                   !this.StagedFiles.Contains(selectedItem.Parent))
                {
                    var ancestors = this.ImportDirectory.GetBranch(selectedItem);

                    foreach (var ancestor in ancestors)
                    {
                        if (this.StagedFiles.Contains(ancestor))
                            continue;

                        // Parent (or) Root
                        var ancestorParent = ancestor.Parent != null ? this.StagedFiles.GetNode(ancestor.Parent.RecursionDepth, ancestor.Parent.Key) : null;

                        // Stage
                        var stagedAncestor = CreateStagedFileNode(ancestorParent, ancestor, libraryFiles,
                                                        tagFileMaps, acoustIDResults,
                                                        musicBrainzAcoustIDResults, musicBrainzResults);

                        // -> Add (root)
                        this.StagedFiles.Add(stagedAncestor);
                    }
                }

                // Leaf Only
                else
                {
                    // Parent (or) Root
                    var parent = selectedItem.Parent != null ? this.StagedFiles.GetNode(selectedItem.Parent.RecursionDepth, selectedItem.Parent.Key) : null;

                    var node = CreateStagedFileNode(parent, selectedItem, libraryFiles,
                                                    tagFileMaps, acoustIDResults,
                                                    musicBrainzAcoustIDResults, musicBrainzResults);

                    // -> Add (root)
                    this.StagedFiles.Add(node);
                }
            }

            //var removeList = new List<LibraryImporterFileTreeNodeViewModel>();

            //// Set Mis-matching node parents
            //this.StagedFiles.ForEach(stagedFile =>
            //{
            //    // Import File
            //    var importFile = this.ImportDirectory.First(x => x.Key == stagedFile.Key);

            //    if (importFile == null)
            //        throw new Exception("Application error: improper handling of tree nodes");

            //    if (importFile.Parent == null &&
            //        stagedFile.Parent != null)
            //        throw new Exception("Improper handling of staged file tree");

            //    else if (importFile.Parent != null)
            //    {
            //        if (stagedFile.Parent == null)
            //        {
            //            var stagedParent = this.StagedFiles.First(x => x.Key == importFile.Parent.Key);

            //            if (stagedParent == null)
            //                throw new Exception("Parent node of import files not staged!");

            //            removeList.Add(stagedFile);
            //        }
            //    }
            //});

            //// 1) Remove, 2) Set Parent, 3) Add
            //foreach (var removeNode in removeList)
            //{
            //    this.StagedFiles.Remove(removeNode);
            //}
            //foreach (var removeNode in removeList)
            //{
            //    var stagedParent = this.StagedFiles.First(x => x.Key == importFile.Parent.Key);
            //}


            this.StagedFiles.EndUpdate();
        }

        public override void Reset(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Most of the memory
            this.StagedFiles.Clear();

            // This gets re-instantiated; but there are lots of event hooks that need to
            // be unhooked or there will be memory leaks from repeat imports.
            //
            this.ImportDirectory.Clear();
            this.ImportDirectory = null;

            this.Loaded = false;
        }
        private void StagedFiles_TreeSelectionChangedEvent(IEnumerable<TreeViewNodeModelBase> selectedNodes)
        {
            UpdateCommands();

            // Bubble Up Events (these must be forwarded to the parent view model
            OnPropertyChanged("ImportDirectory");
            OnPropertyChanged("StagedFiles");
        }

        private void ImportDirectory_TreeSelectionChangedEvent(IEnumerable<TreeViewNodeModelBase> selectedNodes)
        {
            UpdateCommands();

            // Bubble Up Events (these must be forwarded to the parent view model
            OnPropertyChanged("ImportDirectory");
            OnPropertyChanged("StagedFiles");
        }
        private void OnBubbleUpUIEvent(object? sender, PropertyChangedEventArgs e)
        {
            // Bubble Up Events (these must be forwarded to the parent view model
            OnPropertyChanged("ImportDirectory");
            OnPropertyChanged("StagedFiles");
        }
        public void UpdateCommands()
        {
            this.StageCommand.RaiseCanExecuteChanged();
            this.UnstageCommand.RaiseCanExecuteChanged();
        }

        private LibraryImporterFileTreeNodeViewModel CreateStagedFileNode(
                    LibraryImporterFileTreeNodeViewModel? parent, FileTreeNodeViewModel fileNode,
                    Dictionary<string, FileReference> libraryFiles,
                    Dictionary<string, TagSmallFileReferenceMap> tagFileMaps,
                    Dictionary<string, List<AcoustIDLookupResult>> acoustIDResults,
                    Dictionary<string, List<MusicBrainzAcoustIDResult>> musicBrainzAcoustIDResults,
                    Dictionary<Guid, TagSmallVendorMap> musicBrainzResults)
        {
            var stagedFileNode = new LibraryImporterFileTreeNodeViewModel(fileNode.FullPath, fileNode.BaseDirectory, fileNode.IsDirectory, parent, _workflowConfiguration);

            // Directory
            if (fileNode.IsDirectory)
                return stagedFileNode;

            // Tag
            var tagData = _tagCache.GetFullTag(stagedFileNode.FullPath);

            // (AcoustID / Music Brainz) Stored in Tag
            stagedFileNode.ImportFile.MusicBrainzReleaseTrackIDTag = tagData.GetMusicBrainzReleaseTrackId();

            // Tag (read only)
            if (tagData != null)
            {
                stagedFileNode.ImportFile.Tag = _audioStationMapper.Map<TagSmall, TagSmallViewModel>(TagMapper.Map(tagData));
            }


            // AcoustID Result
            if (acoustIDResults.ContainsKey(stagedFileNode.FullPath))
            {
                stagedFileNode.ImportFile.ImportOutput.AcoustIDResults.AddRange(acoustIDResults[stagedFileNode.FullPath].Select(x =>
                {
                    return _audioStationMapper.Map<AcoustIDLookupResult, AcoustIDLookupResultViewModel>(x);
                }));
            }

            // Music Brainz (basic)
            if (musicBrainzAcoustIDResults.ContainsKey(stagedFileNode.FullPath))
            {
                // Get all tag-recording maps for this file
                var results = musicBrainzAcoustIDResults[stagedFileNode.FullPath];

                foreach (var result in results)
                {
                    var tagSmall = _audioStationDbClient.GetEntity<TagSmall>(result.TagSmallId);

                    if (tagSmall != null)
                    {
                        stagedFileNode.ImportFile
                                  .ImportOutput
                                  .MusicBrainzRecordingMatches
                                  .Add(_audioStationMapper.Map<TagSmall, TagSmallViewModel>(tagSmall));
                    }

                    // Also, add this combined entity to our import output
                    stagedFileNode.ImportFile
                              .ImportOutput
                              .MusicBrainzAcoustIDResults.Add(result);
                }
            }

            // Tag (Record) (Preference)
            switch (_workflowConfiguration.TagSourcePreference)
            {
                case LibraryImportSource.File:
                {
                    if (tagData != null)
                    {
                        var tagSmall = TagMapper.Map(tagData);

                        _audioStationMapper.MapOnto(tagSmall, stagedFileNode.ImportFile.TagRecordDirty);
                        _audioStationMapper.MapOnto(tagSmall, stagedFileNode.ImportFile.TagRecordClean);

                        //// Audio Duration (Deduce from IAudioConverter)
                        //stagedFile.TagRecordDirty.DurationMilliseconds = (int)duration.TotalMilliseconds;
                        //stagedFile.TagRecordClean.DurationMilliseconds = (int)duration.TotalMilliseconds;
                    }
                    else if (tagFileMaps.ContainsKey(stagedFileNode.FullPath))
                    {
                        _audioStationMapper.MapOnto(tagFileMaps[stagedFileNode.FullPath].TagSmall, stagedFileNode.ImportFile.TagRecordDirty);
                        _audioStationMapper.MapOnto(tagFileMaps[stagedFileNode.FullPath].TagSmall, stagedFileNode.ImportFile.TagRecordClean);
                    }
                }
                break;
                case LibraryImportSource.DataService:
                {
                    if (tagFileMaps.ContainsKey(stagedFileNode.FullPath))
                    {
                        _audioStationMapper.MapOnto(tagFileMaps[stagedFileNode.FullPath].TagSmall, stagedFileNode.ImportFile.TagRecordDirty);
                        _audioStationMapper.MapOnto(tagFileMaps[stagedFileNode.FullPath].TagSmall, stagedFileNode.ImportFile.TagRecordClean);
                    }
                    else if (tagData != null)
                    {
                        var tagSmall = TagMapper.Map(tagData);

                        _audioStationMapper.MapOnto(tagSmall, stagedFileNode.ImportFile.TagRecordDirty);
                        _audioStationMapper.MapOnto(tagSmall, stagedFileNode.ImportFile.TagRecordClean);

                        //// Audio Duration (Deduce from IAudioConverter)
                        //stagedFile.TagRecordDirty.DurationMilliseconds = (int)duration.TotalMilliseconds;
                        //stagedFile.TagRecordClean.DurationMilliseconds = (int)duration.TotalMilliseconds;
                    }
                }
                break;
                default:
                    break;
            }

            // Check For Library Conflict
            //
            stagedFileNode.ImportFile.LibraryConflict = libraryFiles.ContainsKey(stagedFileNode.FullPath);
            stagedFileNode.ImportFile.FileConflict = false;

            return stagedFileNode;
        }

        public override void Dispose()
        {

        }
    }
}
