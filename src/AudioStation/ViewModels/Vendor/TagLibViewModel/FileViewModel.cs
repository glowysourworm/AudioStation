using AutoMapper;
using AutoMapper.Execution;

using SimpleWpf.Extensions;
using SimpleWpf.ObjectMapping;

using TagLib;

namespace AudioStation.ViewModels.Vendor.TagLibViewModel
{
    public class FileViewModel : ViewModelBase
    {
        string _name;
        TagTypes _tagTypesOnDisk;
        TagTypes _tagTypes;
        string _mimeType;
        long _length;
        bool _possiblyCorrupt;
        IEnumerable<string> _corruptReasons;
        TagViewModel _tag;
        PropertiesViewModel _properties;

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

        public FileViewModel()
        {

        }
        public FileViewModel(TagLib.File tagLibFile)
        {
            this.Tag = new TagViewModel(tagLibFile.Tag);
            this.Properties = new PropertiesViewModel(tagLibFile.Properties);

            this.Name = tagLibFile.Name;
            this.TagTypesOnDisk = tagLibFile.TagTypesOnDisk;
            this.TagTypes = tagLibFile.TagTypes;
            this.MimeType = tagLibFile.MimeType;
            this.Length = tagLibFile.Length;
            this.PossiblyCorrupt = tagLibFile.PossiblyCorrupt;
            this.CorruptReasons = tagLibFile.CorruptionReasons;
        }
    }
}
