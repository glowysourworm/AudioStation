using ATL;
using ATL.AudioData;

namespace AudioStation.Core.Database.MusicBrainzDatabase.Model
{
    public class MusicBrainzPicture : PictureInfo
    {
        public MusicBrainzPicture(PictureInfo picInfo, bool copyPictureData = true) : base(picInfo, copyPictureData)
        {
        }

        public MusicBrainzPicture(PIC_TYPE picType, int position = 1) : base(picType, position)
        {
        }

        public MusicBrainzPicture(MetaDataIOFactory.TagType tagType, object nativePicCode, int position = 1) : base(tagType, nativePicCode, position)
        {
        }
    }
}
