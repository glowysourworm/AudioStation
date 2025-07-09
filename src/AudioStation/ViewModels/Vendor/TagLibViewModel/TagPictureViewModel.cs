using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleWpf.Extensions;

using TagLib;

namespace AudioStation.ViewModels.Vendor.TagLibViewModel
{
    public class TagPictureViewModel : ViewModelBase, IPicture
    {
        string _mimeType;
        string _description;
        PictureType _type;
        ByteVector _data;

        public string MimeType
        {
            get { return _mimeType; }
            set { this.RaiseAndSetIfChanged(ref _mimeType, value); }
        }
        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }
        public PictureType Type
        {
            get { return _type; }
            set { this.RaiseAndSetIfChanged(ref _type, value); }
        }
        public ByteVector Data
        {
            get { return _data; }
            set { this.RaiseAndSetIfChanged(ref _data, value); }
        }

        public TagPictureViewModel()
        {

        }
    }
}
