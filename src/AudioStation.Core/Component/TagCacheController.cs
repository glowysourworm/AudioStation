using System.IO;

using ATL;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model.Vendor.ATLExtension;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using FanartTv.Types;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.RecursiveSerializer.Shared;
using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(ITagCacheController))]
    public class TagCacheController : ITagCacheController
    {
        SimpleDictionary<string, AudioStationTag> _tagFiles;

        [IocImportingConstructor]
        public TagCacheController()
        {
            _tagFiles = new SimpleDictionary<string, AudioStationTag>();
        }

        public bool Verify(string fileName)
        {
            try
            {
                if (_tagFiles.ContainsKey(fileName))
                    return true;

                Set(fileName);

                return _tagFiles.ContainsKey(fileName);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Tag data invalid:  {0}", LogMessageType.General, LogLevel.Warning, ex, fileName);
                return false;
            }
        }

        public AudioStationTag Get(string fileName)
        {
            try
            {
                if (_tagFiles.ContainsKey(fileName))
                    return _tagFiles[fileName];

                Set(fileName);

                if (!_tagFiles.ContainsKey(fileName))
                    throw new Exception("Unable to open tag file:  " + fileName);

                return _tagFiles[fileName];
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error initializing tag data:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
        public AudioStationTag GetCopy(string fileName)
        {
            // ATL -> AudioStation
            return FromFile(fileName);
        }
        public void Set(string fileName)
        {
            if (_tagFiles.ContainsKey(fileName))
                _tagFiles.Remove(fileName);

            // ATL -> AudioStation
            var tagFile = FromFile(fileName);

            _tagFiles.Add(fileName, tagFile);
        }

        public void Evict(string fileName)
        {
            try
            {
                if (_tagFiles.ContainsKey(fileName))
                    _tagFiles.Remove(fileName);

                else
                    throw new Exception("Trying to evict tag file that was not yet cached! Please use this controller to get / set all tag files!");
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error evicting tag data:  {0}", LogMessageType.General, LogLevel.Error, ex, fileName);
                throw ex;
            }
        }

        public void SetData(string fileName, IAudioStationTag tagData, bool save = true)
        {
            if (!_tagFiles.ContainsKey(fileName))
                Set(fileName);

            ToFile(fileName, tagData);
        }
        public byte[] Serialize(AudioStationTag serializableTag)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new RecursiveSerializer<AudioStationTag>(new RecursiveSerializerConfiguration()
                {
                    IgnoreRemovedProperties = false,
                    PreviewRemovedProperties = false
                });

                serializer.Serialize(stream, serializableTag);

                return stream.GetBuffer();
            }
        }
        public AudioStationTag Deserialize(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                var serializer = new RecursiveSerializer<AudioStationTag>(new RecursiveSerializerConfiguration()
                {
                    IgnoreRemovedProperties = false,
                    PreviewRemovedProperties = false
                });

                stream.Seek(0, SeekOrigin.Begin);

                return serializer.Deserialize(stream);
            }
        }

        private void ToFile(string fileName, IAudioStationTag tag)
        {
            try
            {
                var existingFile = Get(fileName);

                // Copy data onto existing tag
                ApplicationHelpers.MapOnto(tag, existingFile);

                // AudioStation -> ATL 
                var atlTrack = ApplicationHelpers.Map<IAudioStationTag, ATL.Track>(tag);

                // IAudioStationTag:  Additional Fields (taken from ATL.Track)
                atlTrack.AlbumArtist = tag.AlbumArtists.FirstOrDefault() ?? string.Empty;
                atlTrack.Genre = tag.Genres.FirstOrDefault() ?? string.Empty;

                // NEEDS TO BE FINISHED:  Figure out how to store multiple artists / genres

                atlTrack.TrackNumber = (int)tag.Track;
                atlTrack.TrackNumberStr = tag.Track.ToString();

                // ATL:  Save
                atlTrack.SaveTo(fileName);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving tag data:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        private AudioStationTag FromFile(string fileName)
        {
            try
            {
                // ATL Interface
                var atlTrack = new ATL.Track(fileName);

                // ATL -> AudioStation
                var tagFile = ApplicationHelpers.Map<ATL.Track, AudioStationTag>(atlTrack);

                // IAudioStationTag:  Additional Fields (taken from ATL.Track)
                if (!string.IsNullOrEmpty(atlTrack.AlbumArtist))
                    tagFile.AlbumArtists.Add(atlTrack.AlbumArtist);                     // NEEDS TO BE FINISHED

                tagFile.AudioFormat = atlTrack.AudioFormat.Name;
                tagFile.BitDepth = atlTrack.BitDepth;
                tagFile.BitRate = atlTrack.Bitrate;
                tagFile.Channels = atlTrack.ChannelsArrangement.NbChannels;
                tagFile.Duration = TimeSpan.FromMilliseconds(atlTrack.DurationMs);
                tagFile.Encoder = atlTrack.Encoder;

                uint trackNumber = 0;
                uint.TryParse(atlTrack.TrackNumberStr, out trackNumber);

                // NEEDS TO BE FINISHED
                if (!string.IsNullOrWhiteSpace(atlTrack.Genre))
                    tagFile.Genres.Add(atlTrack.Genre);

                tagFile.IsVariableBitRate = atlTrack.IsVBR;
                tagFile.SampleRate = atlTrack.SampleRate;
                tagFile.Track = trackNumber;
                tagFile.Year = atlTrack.Year ?? atlTrack.Date?.Year ?? 0;

                return tagFile;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error reading tag data (rebuilding Mp3 file):  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
    }
}
