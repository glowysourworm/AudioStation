using System.Windows;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using AutoMapper;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application;
using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.ViewModels.Vendor.TagLibViewModel
{
    public class TagFileGroupViewModel : ViewModelBase
    {
        SimpleDictionary<string, TagViewModel> _tagDict;
        TagGroupViewModel _tagGroup;
        SimpleCommand _saveCommand;
        SimpleCommand _copyCommand;
        SimpleCommand _pasteCommand;

        public TagGroupViewModel TagGroup
        {
            get { return _tagGroup; }
            set { this.RaiseAndSetIfChanged(ref _tagGroup, value); }
        }
        public IEnumerable<string> TagFileNames
        {
            get { return _tagDict.Keys; }
        }
        public SimpleCommand SaveCommand
        {
            get { return _saveCommand; }
            set { this.RaiseAndSetIfChanged(ref _saveCommand, value); }
        }
        public SimpleCommand CopyCommand
        {
            get { return _copyCommand; }
            set { this.RaiseAndSetIfChanged(ref _copyCommand, value); }
        }
        public SimpleCommand PasteCommand
        {
            get { return _pasteCommand; }
            set { this.RaiseAndSetIfChanged(ref _pasteCommand, value); }
        }

        public TagFileGroupViewModel()
        {
            var dialogController = IocContainer.Get<IDialogController>();
            var tagCacheController = IocContainer.Get<ITagCacheController>();

            _tagDict = new SimpleDictionary<string, TagViewModel>();
            _tagGroup = new TagGroupViewModel();

            this.SaveCommand = new SimpleCommand(() =>
            {
                Save(dialogController, tagCacheController);
            });
            this.CopyCommand = new SimpleCommand(() =>
            {
                Copy(dialogController, tagCacheController);
            });
            this.PasteCommand = new SimpleCommand(() =>
            {
                Paste(dialogController, tagCacheController);
            });
        }
        public TagFileGroupViewModel(IEnumerable<TagFileViewModel> tagFiles)
        {
            var dialogController = IocContainer.Get<IDialogController>();
            var tagCacheController = IocContainer.Get<ITagCacheController>();

            _tagDict = tagFiles.ToSimpleDictionary(x => x.Name, x => x.Tag);
            _tagGroup = new TagGroupViewModel(TagGroupViewModel.GroupType.None, tagFiles.Select(x => x.Tag));

            this.SaveCommand = new SimpleCommand(() =>
            {
                Save(dialogController, tagCacheController);
            });
            this.CopyCommand = new SimpleCommand(() =>
            {
                Copy(dialogController, tagCacheController);
            });
            this.PasteCommand = new SimpleCommand(() =>
            {
                Paste(dialogController, tagCacheController);
            });
        }

        private void Save(IDialogController dialogController, ITagCacheController tagCacheController)
        {
            var messageLines = new List<string>() { "This data will be saved to the following tags:", "" };
            messageLines.AddRange(_tagDict.Keys);
            messageLines.Add("");
            messageLines.Add("Are you sure you want to do this?");

            if (dialogController.ShowConfirmation("Save Mp3 Tag to File?", messageLines.ToArray()))
            {
                try
                {
                    // Save the group data to each individual tag
                    foreach (var pair in _tagDict)
                    {
                        // Create file in memory (load)
                        var tagFile = tagCacheController.Get(pair.Key);

                        // Map properties
                        tagCacheController.MapOnto(_tagGroup, tagFile.Tag);

                        // No need to evict cache - this will be the same reference
                        tagFile.Save();
                    }
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error saving tag data:  {0}", LogMessageType.LibraryLoader, LogLevel.Error, ex.Message);
                }
            }
        }
        private void Copy(IDialogController dialogController, ITagCacheController tagCacheController)
        {
            try
            {
                if (tagCacheController.CopyToClipboard(this.TagGroup))
                {
                    dialogController.ShowAlert("Mp3 File Data Copy", "Tag data copied to clipboard:", "(Grouped Mp3 File(s))");
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error copying tag data:  {0}", LogMessageType.LibraryLoader, LogLevel.Error, ex.Message);
            }
        }
        private void Paste(IDialogController dialogController, ITagCacheController tagCacheController)
        {
            var messageLines = new List<string>() { "This data will be saved to the following tags:" };
            messageLines.AddRange(this.TagFileNames);
            messageLines.Add("Are you sure you want to do this?");

            if (dialogController.ShowConfirmation("Save Mp3 Tag to File?", messageLines.ToArray()))
            {
                try
                {
                    var tagData = tagCacheController.CopyFromClipboard();
                    
                    // Map data back to the tag group
                    tagCacheController.MapOnto(tagData, this.TagGroup);
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error pasting tag data:  {0}", LogMessageType.LibraryLoader, LogLevel.Error, ex.Message);
                }
            }
        }
    }
}
