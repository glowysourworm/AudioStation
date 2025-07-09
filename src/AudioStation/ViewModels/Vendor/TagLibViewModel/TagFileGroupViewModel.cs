using AudioStation.Controller.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using AutoMapper;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Command;
using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.ViewModels.Vendor.TagLibViewModel
{
    public class TagFileGroupViewModel : ViewModelBase
    {
        SimpleDictionary<string, TagViewModel> _tagDict;
        TagGroupViewModel _tagGroup;
        SimpleCommand _saveCommand;

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

        public TagFileGroupViewModel(IDialogController dialogController)
        {
            _tagDict = new SimpleDictionary<string, TagViewModel>();
            _tagGroup = new TagGroupViewModel();

            this.SaveCommand = new SimpleCommand(() =>
            {
                Save(dialogController);
            });
        }
        public TagFileGroupViewModel(IDialogController dialogController, IEnumerable<TagFileViewModel> tagFiles)
        {
            _tagDict = tagFiles.ToSimpleDictionary(x => x.Name, x => x.Tag);
            _tagGroup = new TagGroupViewModel(TagGroupViewModel.GroupType.None, tagFiles.Select(x => x.Tag));

            this.SaveCommand = new SimpleCommand(() =>
            {
                Save(dialogController);
            });
        }

        private void Save(IDialogController dialogController)
        {
            var messageLines = new List<string>() { "This data will be saved to the following tags:" };
            messageLines.AddRange(_tagDict.Keys);
            messageLines.Add("Are you sure you want to do this?");

            if (dialogController.ShowConfirmation("Save Mp3 Tag to File?", messageLines.ToArray()))
            {
                try
                {
                    // Use AutoMapper to map properties to the tag
                    var config = new MapperConfiguration(cfg => cfg.CreateMap<TagGroupViewModel, TagLib.Tag>(MemberList.Destination));
                    var mapper = config.CreateMapper();

                    // Save the group data to each individual tag
                    foreach (var pair in _tagDict)
                    {
                        // Create file in memory (load)
                        var tagFile = TagLib.File.Create(pair.Key);

                        // Map properties
                        mapper.Map(_tagGroup, tagFile.Tag, typeof(TagGroupViewModel), typeof(TagLib.Tag));

                        tagFile.Save();
                    }
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error saving tag data:  {0}", LogMessageType.LibraryLoader, LogLevel.Error, ex.Message);
                }
            }
        }
    }
}
