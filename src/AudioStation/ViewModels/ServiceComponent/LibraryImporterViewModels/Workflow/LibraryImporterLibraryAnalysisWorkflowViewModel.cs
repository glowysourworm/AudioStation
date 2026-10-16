using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Event;
using AudioStation.Utility;
using AudioStation.ViewModels.LibraryViewModels.LibraryTree;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.UI.Command;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels.Workflow
{
    public class LibraryImporterLibraryAnalysisWorkflowViewModel : ServiceComponentPartViewModelBase
    {
        private IAudioStationMapper _audioStationMapper;
        private IAudioStationDbClient _audioStationDbClient;
        private ITagCache _tagCache;

        // Workflow Configuration
        private LibraryImporterConfigurationViewModel _workflowConfiguration;

        SimpleCommand _stageCommand;
        SimpleCommand _unstageCommand;

        // Import Library Analysis: The import tree allows for a mixture of node types - which we're going
        //                          to use to put the abstract grouping (Genre, Artist, Album, ...) to use.
        //
        //                          We're going to divide the display in half, as in staging, but the right
        //                          side will be where the user sees what will be imported prior to staging
        //                          files:  Artists, Albums, Genres, ...
        //
        //                          Disambiguation will occurr here - which also involves choosing entities
        //                          that may become unused; or whatever is chosen in the staging and import
        //                          process.
        //
        LibraryImporterTreeViewModel _sourceTree;
        LibraryImporterTreeViewModel _destinationTree;

        public LibraryImporterTreeViewModel SourceTree
        {
            get { return _sourceTree; }
            set { this.RaiseAndSetIfChanged(ref _sourceTree, value); }
        }
        public LibraryImporterTreeViewModel DestinationTree
        {
            get { return _destinationTree; }
            set { this.RaiseAndSetIfChanged(ref _destinationTree, value); }
        }

        public LibraryImporterLibraryAnalysisWorkflowViewModel(
                IDialogController dialogController,
                LibraryImporterConfigurationViewModel workflowConfiguration)
            : base("Library Analysis", "This workflow component will be used for analyzing the Audio Station library to identify where entities will be created or identified by the user")
        {
            _workflowConfiguration = workflowConfiguration;

            _sourceTree = new LibraryImporterTreeViewModel();
            _destinationTree = new LibraryImporterTreeViewModel();
        }

        public override bool CanExecute()
        {
            return this.Loaded && !this.Working;
        }
        public override bool CanLoad()
        {
            return !this.Loaded && !this.Working;
        }
        public override bool CanReset()
        {
            return this.Loaded && !this.Working;
        }

        public override void Execute(DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }
        public override void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            _audioStationDbClient = audioStationController.ServiceController.GetDataService<IAudioStationDbClient>();
            _audioStationMapper = IocContainer.Get<IAudioStationMapper>();
            _tagCache = audioStationController.ServiceController.GetCache<ITagCache>();

            var audioConverter = IocContainer.Get<IAudioConverter>();

            // Multiple File Search
            var searchPatterns = audioConverter.GetSupportedFormatExtensions()
                                               .Select(x => "*" + x)
                                               .ToArray();

            // Block Events
            this.SourceTree.BeginUpdate();

            progressHandler(1, 1, 1, 0, "Loading Library Files");

            // Library Files
            var libraryFiles = _audioStationDbClient.GetEntities<FileReference>().ToDictionary(x => x.FileName);

            string directory;
            TrackGroupingType directoryFormat;
            string customFormat;

            switch (_workflowConfiguration.ImportType)
            {
                case LibraryImportType.InPlaceDirectory:
                    directory = _workflowConfiguration.ImportDirectory.Directory;
                    directoryFormat = _workflowConfiguration.ImportDirectory.FolderFormatType;
                    customFormat = _workflowConfiguration.ImportDirectory.CustomFolderFormat;
                    break;
                case LibraryImportType.Migration:
                    directory = _workflowConfiguration.MigrationSourceDirectory;
                    directoryFormat = _workflowConfiguration.MigrationDirectoryFormat;
                    customFormat = _workflowConfiguration.MigrationDirectoryCustomFormat;
                    break;
                default:
                    throw new Exception("Unhandled import type");
            }

            var grouping = new LibraryGrouping(directoryFormat, customFormat);

            // Load tree to completion:  Option is to load to depth - which would be lazy loading
            //
            FileTreeFormatNodeViewModel? parent = null;

            this.SourceTree.BeginUpdate();

            DirectoryTreeLoader.RecurseDirectory(directory, (currentDirectory, recursionDepth, currentFile, fileCount) =>
            {
                // Add node based on the depth; and the key (which should be the file path)
                parent = this.SourceTree.Add(new FileTreeFormatNodeViewModel(recursionDepth, currentFile.FullPath, fileCount ?? 0, directoryFormat, customFormat, parent));

            }, progressHandler, searchPatterns);

            this.SourceTree.EndUpdate();

            this.Loaded = true;

            //var selectedFileCount = selectedNodes.Count;
            //var counter = 0;

            //var treeDepthArtist = 0;
            //var treeDepthAlbum = 1;
            //var treeDepthAlbumMedia = 2;
            //var treeDepthTrack = 3;


            //// Disambiguation:  Artist, and Album names have to be clarified by the user
            ////
            //// Clarification:  There will be some simple methods to move things around - 
            ////                 drag / drop, right-click, also delete from staging. The
            ////                 user can stage by dragging and dropping (also). 
            ////
            //// The final staged file collection should be imported the way it is grouped.
            ////
            //var duplicateAlbums = new SimpleDictionary<string, List<FileTreeNodeViewModel>>();
            //var duplicateArtists = new SimpleDictionary<string, List<FileTreeNodeViewModel>>();

            //// THIS HAS TO BE MADE FOR ALL GROUPINGS
            //foreach (var selectedItem in selectedNodes)
            //{
            //    // FOR NOW:  Artist -> Album

            //    var artistDirectroyNode = selectedItem.Parent as FileTreeNodeViewModel;

            //    if (artistDirectroyNode == null || !artistDirectroyNode.IsDirectory)
            //        continue;

            //    var albumDirectoryNode = artistDirectroyNode.Parent as FileTreeNodeViewModel;

            //    if (albumDirectoryNode == null || !albumDirectoryNode.IsDirectory)
            //        continue;

            //    if (string.IsNullOrWhiteSpace(artistDirectroyNode.ShortPath) ||
            //        string.IsNullOrWhiteSpace(albumDirectoryNode.ShortPath))
            //    {

            //    }

            //    // Tag
            //    var tagData = _tagCache.GetFull(selectedItem.FullPath);

            //    // Duplicate Albums
            //    if (!string.IsNullOrWhiteSpace(tagData.Album) &&
            //        !duplicateAlbums.ContainsKey(tagData.Album))
            //        duplicateAlbums.Add(tagData.Album, new List<FileTreeNodeViewModel>() { selectedItem });

            //    else if (!string.IsNullOrWhiteSpace(tagData.Album))
            //        duplicateAlbums[tagData.Album].Add(selectedItem);

            //    // Duplicate Artists (AlbumArtist is the field used by both tag formats)
            //    if (!string.IsNullOrWhiteSpace(tagData.AlbumArtist) &&
            //        !duplicateArtists.ContainsKey(tagData.AlbumArtist))
            //        duplicateArtists.Add(tagData.AlbumArtist, new List<FileTreeNodeViewModel>() { selectedItem });

            //    else if (!string.IsNullOrWhiteSpace(tagData.AlbumArtist))
            //        duplicateArtists[tagData.AlbumArtist].Add(selectedItem);
            //}

            //// Remove Non-Duplicates
            //duplicateAlbums.Filter(x => x.Value.Count <= 1);
            //duplicateArtists.Filter(x => x.Value.Count <= 1);

            //foreach (var selectedItem in selectedNodes)
            //{
            //    progressHandler(1, 1, selectedFileCount, counter++, "Loading:  " + selectedItem.FullPath);

            //    // Tag (for the initial grouping)
            //    var tagData = _tagCache.GetFull(selectedItem.FullPath);

            //    // ITagSmall
            //    var tagSmall = _audioStationMapper.Map<TagSmall, TagSmallViewModel>(TagMapper.Map(tagData));

            //    LibraryTreeArtistNodeViewModel? artistNode = null;
            //    LibraryTreeAlbumNodeViewModel? albumNode = null;
            //    LibraryTreeAlbumMediaNodeViewModel? albumMediaNode = null;

            //    // Defaults if pertinent
            //    var albumNameDefault = "(Unknown)";
            //    var artistNameDefault = "(Unknown)";

            //    var mediaNodeValid = LibraryTreeAlbumMediaNodeViewModel.ValidateKey(tagSmall.MediaNumber ?? 0,
            //                                                                        tagSmall.MediaTotal ?? 0,
            //                                                                        tagSmall.MediaFormat ?? string.Empty);

            //    // Grouping:  Artist -> Album (TODO: Complete Grouping Types)
            //    //
            //    if (!string.IsNullOrWhiteSpace(tagSmall.AlbumArtist))
            //        artistNode = this.StagedFiles.GetNode<LibraryTreeEntityNodeViewModel>(0, tagSmall.AlbumArtist);

            //    if (!string.IsNullOrWhiteSpace(tagSmall.Album))
            //        albumNode = this.StagedFiles.GetNode<LibraryTreeAlbumNodeViewModel>(1, tagSmall.Album);

            //    if (mediaNodeValid)
            //    {
            //        albumMediaNode = this.StagedFiles
            //                             .GetNode<LibraryTreeAlbumMediaNodeViewModel>(
            //        LibraryTreeAlbumMediaNodeViewModel.GenerateKey(tagSmall.MediaNumber ?? 0,
            //                                                       tagSmall.MediaTotal ?? 0,
            //                                                       tagSmall.MediaFormat ?? string.Empty));
            //    }

            //    if (artistNode == null)
            //    {
            //        // -> Parent:  Root (null)
            //        artistNode = new LibraryTreeArtistNodeViewModel(treeDepthArtist, new ArtistViewModel(Guid.Empty)
            //        {
            //            Artist = string.IsNullOrWhiteSpace(tagSmall.AlbumArtist) ? artistNameDefault : tagSmall.AlbumArtist

            //        }, null);

            //        // Staged Files:  This will check for the artist by key (artist name)
            //        if (!this.StagedFiles.Contains(artistNode))
            //            this.StagedFiles.Add(artistNode);
            //    }

            //    if (albumNode == null)
            //    {
            //        // -> Parent:  Artist Node
            //        albumNode = new LibraryTreeAlbumNodeViewModel(treeDepthAlbum, new AlbumViewModel(Guid.Empty)
            //        {
            //            Album = string.IsNullOrWhiteSpace(tagSmall.Album) ? albumNameDefault : tagSmall.Album

            //        }, artistNode);

            //        // Staged Files:  This will check for the album by key (album name)
            //        if (!this.StagedFiles.Contains(albumNode))
            //            this.StagedFiles.Add(albumNode);
            //    }

            //    if (albumMediaNode == null)
            //    {
            //        // Album Media Node:  Essentially, the media node will take care of improper media numbers; but not
            //        //                    an improper album name. The album node must be present; and we'll create an 
            //        //                    "invalid" media node until the numbers are setup properly.
            //        //

            //        // -> Parent:  Album Node
            //        albumMediaNode = new LibraryTreeAlbumMediaNodeViewModel(treeDepthAlbumMedia,
            //                                                                tagSmall.MediaFormat ?? string.Empty,
            //                                                                tagSmall.MediaNumber ?? 0,
            //                                                                tagSmall.MediaTotal ?? 0, albumNode);

            //        // Staged Files:  This will check for the album media node by key (media number)
            //        if (!this.StagedFiles.Contains(albumMediaNode))
            //            this.StagedFiles.Add(albumMediaNode);
            //    }

            //    // Leaf:  Next File Node (this would be a library track node; but we're just using those to have an abstract tree
            //    //                        for this import process)

            //    // -> Parent:  Album Node
            //    var trackNode = CreateStagedFileNode(treeDepthTrack, albumMediaNode, selectedItem, tagData, tagSmall, libraryFiles,
            //                                         tagFileMaps, acoustIDResults,
            //                                         musicBrainzAcoustIDResults, musicBrainzResults);

            //    // -> Staged Files:  Add track (leaf node) - which has a key of the file's full path. So, this should be unique.
            //    if (!this.StagedFiles.Contains(trackNode))
            //        this.StagedFiles.Add(trackNode);

            //    // Set Duplicate Messages:
            //    //
            //    if (duplicateAlbums.ContainsKey(albumNode.Album.Album))
            //        albumNode.SetDuplicate("Duplicate");

            //    if (duplicateArtists.ContainsKey(artistNode.Artist.Artist))
            //        artistNode.SetDuplicate("Duplicate");
            //}
        }
        public override void Reset(DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }

        public override void Dispose()
        {

        }
    }
}
