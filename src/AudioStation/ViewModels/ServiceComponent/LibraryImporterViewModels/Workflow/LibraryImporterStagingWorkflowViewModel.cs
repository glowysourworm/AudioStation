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
using AudioStation.ViewModels.LibraryViewModels.LibraryTree;
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
        LibraryImporterTreeViewModel _stagedFiles;
        LibraryImporterStagedFileFilterType _stagedFileFilterType;

        public FileTreeViewModel ImportDirectory
        {
            get { return _importDirectory; }
            set { this.RaiseAndSetIfChanged(ref _importDirectory, value); }
        }
        public LibraryImporterTreeViewModel StagedFiles
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

            this.StagedFiles = new LibraryImporterTreeViewModel();
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
            var selectedNodes = this.ImportDirectory
                                    .SelectedNodes
                                    .OrderBy(x => x.RecursionDepth)
                                    .Cast<FileTreeNodeViewModel>()
                                    .Where(x => !x.IsDirectory)
                                    .ToList();

            var selectedFileCount = selectedNodes.Count;
            var counter = 0;

            var treeDepthArtist = 0;
            var treeDepthAlbum = 1;
            var treeDepthAlbumMedia = 2;
            var treeDepthTrack = 3;


            foreach (var selectedItem in selectedNodes)
            {
                progressHandler(1, 1, selectedFileCount, counter++, "Loading:  " + selectedItem.FullPath);

                // Tag (for the initial grouping)
                var tagData = _tagCache.GetFullTag(selectedItem.FullPath);

                // ITagSmall
                var tagSmall = _audioStationMapper.Map<TagSmall, TagSmallViewModel>(TagMapper.Map(tagData));

                LibraryTreeArtistNodeViewModel? artistNode = null;
                LibraryTreeAlbumNodeViewModel? albumNode = null;
                LibraryTreeAlbumMediaNodeViewModel? albumMediaNode = null;

                // Defaults if pertinent
                var albumNameDefault = "(Unknown)";
                var artistNameDefault = "(Unknown)";

                var mediaNodeValid = LibraryTreeAlbumMediaNodeViewModel.ValidateKey(tagSmall.MediaNumber ?? 0,
                                                                                    tagSmall.MediaTotal ?? 0,
                                                                                    tagSmall.Album ?? albumNameDefault);

                // Grouping:  Artist -> Album (TODO: Complete Grouping Types)
                //
                if (!string.IsNullOrWhiteSpace(tagSmall.AlbumArtist))
                    artistNode = this.StagedFiles.GetNode<LibraryTreeArtistNodeViewModel>(0, tagSmall.AlbumArtist);

                if (!string.IsNullOrWhiteSpace(tagSmall.Album))
                    albumNode = this.StagedFiles.GetNode<LibraryTreeAlbumNodeViewModel>(1, tagSmall.Album);

                if (mediaNodeValid)
                {
                    albumMediaNode = this.StagedFiles
                                         .GetNode<LibraryTreeAlbumMediaNodeViewModel>(
                    LibraryTreeAlbumMediaNodeViewModel.GenerateKey(tagSmall.MediaNumber ?? 0,
                                                                   tagSmall.MediaTotal ?? 0,
                                                                   tagSmall.Album ?? albumNameDefault));
                }

                if (artistNode == null)
                {
                    // -> Parent:  Root (null)
                    artistNode = new LibraryTreeArtistNodeViewModel(treeDepthArtist, new ArtistViewModel(Guid.Empty)
                    {
                        Artist = string.IsNullOrWhiteSpace(tagSmall.AlbumArtist) ? artistNameDefault : tagSmall.AlbumArtist

                    }, null);

                    // Staged Files:  This will check for the artist by key (artist name)
                    if (!this.StagedFiles.Contains(artistNode))
                        this.StagedFiles.Add(artistNode);
                }

                if (albumNode == null)
                {
                    // -> Parent:  Artist Node
                    albumNode = new LibraryTreeAlbumNodeViewModel(treeDepthAlbum, new AlbumViewModel(Guid.Empty)
                    {
                        Album = string.IsNullOrWhiteSpace(tagSmall.Album) ? albumNameDefault : tagSmall.Album

                    }, artistNode);

                    // Staged Files:  This will check for the album by key (album name)
                    if (!this.StagedFiles.Contains(albumNode))
                        this.StagedFiles.Add(albumNode);
                }

                if (albumMediaNode == null)
                {
                    // Album Media Node:  Essentially, the media node will take care of improper media numbers; but not
                    //                    an improper album name. The album node must be present; and we'll create an 
                    //                    "invalid" media node until the numbers are setup properly.
                    //

                    // -> Parent:  Album Node
                    albumMediaNode = new LibraryTreeAlbumMediaNodeViewModel(treeDepthAlbumMedia,
                                                                            tagSmall.Album ?? albumNameDefault,
                                                                            tagSmall.MediaNumber ?? 0,
                                                                            tagSmall.MediaTotal ?? 0, albumNode);

                    // Staged Files:  This will check for the album media node by key (media number)
                    if (!this.StagedFiles.Contains(albumMediaNode))
                        this.StagedFiles.Add(albumMediaNode);
                }

                // Leaf:  Next File Node (this would be a library track node; but we're just using those to have an abstract tree
                //                        for this import process)

                // -> Parent:  Album Node
                var trackNode = CreateStagedFileNode(treeDepthTrack, albumMediaNode, selectedItem, tagData, tagSmall, libraryFiles,
                                                     tagFileMaps, acoustIDResults,
                                                     musicBrainzAcoustIDResults, musicBrainzResults);

                // -> Staged Files:  Add track (leaf node) - which has a key of the file's full path. So, this should be unique.
                if (!this.StagedFiles.Contains(trackNode))
                    this.StagedFiles.Add(trackNode);
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
                    int treeDepth,
                    LibraryTreeAlbumMediaNodeViewModel? parent,
                    FileTreeNodeViewModel fileNode,
                    ITagFull tagData,
                    TagSmallViewModel tagSmallEdit,
                    Dictionary<string, FileReference> libraryFiles,
                    Dictionary<string, TagSmallFileReferenceMap> tagFileMaps,
                    Dictionary<string, List<AcoustIDLookupResult>> acoustIDResults,
                    Dictionary<string, List<MusicBrainzAcoustIDResult>> musicBrainzAcoustIDResults,
                    Dictionary<Guid, TagSmallVendorMap> musicBrainzResults)
        {
            var stagedFileNode = new LibraryImporterFileTreeNodeViewModel(treeDepth, fileNode.FullPath, fileNode.BaseDirectory, fileNode.IsDirectory, parent, _workflowConfiguration);

            // Directory
            if (fileNode.IsDirectory)
                return stagedFileNode;

            // (AcoustID / Music Brainz) Stored in Tag
            stagedFileNode.ImportFile.MusicBrainzReleaseTrackIDTag = tagData.GetMusicBrainzReleaseTrackId();

            // Tag (read only)
            if (tagData != null)
            {
                stagedFileNode.ImportFile.Tag = tagSmallEdit;
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
