using IdSharp.Tagging.APEv2;
using IdSharp.Tagging.ID3v1;
using IdSharp.Tagging.ID3v2;
using IdSharp.Tagging.Lyrics3;
using IdSharp.Tagging.VorbisComment;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.Vendor.IdSharpViewModel
{
    public class TagViewModel : ViewModelBase
    {
        ID3v1Tag _id3v1Tag;
        ID3v2Tag _id3v2Tag;
        APEv2Tag _apev2Tag;
        Lyrics3Tag _lyrics3Tag;
        VorbisComment _vorbisComment;

        public ID3v1Tag Id3v1Tag
        {
            get { return _id3v1Tag; }
            set { this.RaiseAndSetIfChanged(ref _id3v1Tag, value); }
        }
        public ID3v2Tag Id3v2Tag
        {
            get { return _id3v2Tag; }
            set { this.RaiseAndSetIfChanged(ref _id3v2Tag, value); }
        }
        public APEv2Tag Apev2Tag
        {
            get { return _apev2Tag; }
            set { this.RaiseAndSetIfChanged(ref _apev2Tag, value); }
        }
        public Lyrics3Tag Lyrics3Tag
        {
            get { return _lyrics3Tag; }
            set { this.RaiseAndSetIfChanged(ref _lyrics3Tag, value); }
        }
        public VorbisComment VorbisComment
        {
            get { return _vorbisComment; }
            set { this.RaiseAndSetIfChanged(ref _vorbisComment, value); }
        }
    }
}
