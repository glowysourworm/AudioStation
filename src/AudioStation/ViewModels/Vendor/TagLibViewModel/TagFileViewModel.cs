using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.Application;

using TagLib;

namespace AudioStation.ViewModels.Vendor.TagLibViewModel
{
    public class TagFileViewModel : ViewModelBase
    {
        // User Control Parameters
        TagTypes _tagTypeUserSelect;                // User selects filter

        string _name;
        TagTypes _tagTypesOnDisk;
        TagTypes _tagTypes;
        string _mimeType;
        long _length;
        bool _possiblyCorrupt;
        IEnumerable<string> _corruptReasons;
        TagViewModel _tag;
        PropertiesViewModel _properties;

        SimpleCommand _saveCommand;
        SimpleCommand _copyCommand;
        SimpleCommand _pasteCommand;

        public TagTypes TagTypeUserSelect
        {
            get { return _tagTypeUserSelect; }
            set { this.RaiseAndSetIfChanged(ref _tagTypeUserSelect, value); }
        }

        public TagViewModel Tag
        {
            get { return _tag; }
            set { this.RaiseAndSetIfChanged(ref _tag, value); }
        }
        public PropertiesViewModel Properties
        {
            get { return _properties; }
            set { this.RaiseAndSetIfChanged(ref _properties, value); }
        }
        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public TagTypes TagTypesOnDisk
        {
            get { return _tagTypesOnDisk; }
            set { this.RaiseAndSetIfChanged(ref _tagTypesOnDisk, value); }
        }
        public TagTypes TagTypes
        {
            get { return _tagTypes; }
            set { this.RaiseAndSetIfChanged(ref _tagTypes, value); }
        }
        public string MimeType
        {
            get { return _mimeType; }
            set { this.RaiseAndSetIfChanged(ref _mimeType, value); }
        }
        public long Length
        {
            get { return _length; }
            set { this.RaiseAndSetIfChanged(ref _length, value); }
        }
        public bool PossiblyCorrupt
        {
            get { return _possiblyCorrupt; }
            set { this.RaiseAndSetIfChanged(ref _possiblyCorrupt, value); }
        }
        public IEnumerable<string> CorruptReasons
        {
            get { return _corruptReasons; }
            set { this.RaiseAndSetIfChanged(ref _corruptReasons, value); }
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

        /*
        public TagFileViewModel()
        {
            var dialogController = IocContainer.Get<IDialogController>();
            var tagCacheController = IocContainer.Get<ITagCacheController>();

            this.TagTypeUserSelect = TagTypes.AllTags;
            this.TagTypes = TagTypes.AllTags;
            this.TagTypesOnDisk = TagTypes.AllTags;

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
        */
        public TagFileViewModel(TagLib.File tagLibFile)
        {
            var dialogController = IocContainer.Get<IDialogController>();
            var tagCacheController = IocContainer.Get<ITagCacheController>();

            this.Tag = new TagViewModel(tagLibFile.Tag);
            this.Properties = new PropertiesViewModel(tagLibFile.Properties);

            this.Tag.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged("Tag");
            };

            this.Name = tagLibFile.Name;
            this.TagTypeUserSelect = TagTypes.AllTags;
            this.TagTypesOnDisk = tagLibFile.TagTypesOnDisk;
            this.TagTypes = tagLibFile.TagTypes;
            this.MimeType = tagLibFile.MimeType;
            this.Length = tagLibFile.Length;
            this.PossiblyCorrupt = tagLibFile.PossiblyCorrupt;
            this.CorruptReasons = tagLibFile.CorruptionReasons;

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
            var messageLines = new List<string>() { "This data will be saved to the following tags:" };
            messageLines.Add(this.Name);
            messageLines.Add("Are you sure you want to do this?");

            if (dialogController.ShowConfirmation("Save Mp3 Tag to File?", messageLines.ToArray()))
            {
                try
                {
                    // Create file in memory (load)
                    var tagFile = tagCacheController.Get(this.Name);

                    // Map tag data onto the cache instance
                    ApplicationHelpers.MapOnto(this.Tag, tagFile.Tag);

                    // No need to evict cache - this is the same reference
                    tagFile.Save();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error saving tag data:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                }
            }
        }
        private void Copy(IDialogController dialogController, ITagCacheController tagCacheController)
        {
            try
            {
                tagCacheController.CopyToClipboard(this.Tag);

                dialogController.ShowAlert("Mp3 File Data Copy", "Tag data copied to clipboard:", this.Name);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error copying tag data:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }
        }
        private void Paste(IDialogController dialogController, ITagCacheController tagCacheController)
        {
            var messageLines = new List<string>() { "This data will be saved to the following tag:" };
            messageLines.Add(this.Name);
            messageLines.Add("Are you sure you want to do this?");

            if (dialogController.ShowConfirmation("Copy Mp3 Tag From Clipboard?", messageLines.ToArray()))
            {
                try
                {
                    var tagData = tagCacheController.CopyFromClipboard();

                    // Map tag data onto this instance
                    ApplicationHelpers.MapOnto(tagData, this.Tag);
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error pasting tag data:  {0}", LogMessageType.LibraryLoader, LogLevel.Error, ex.Message);
                }
            }
        }
    }
}
