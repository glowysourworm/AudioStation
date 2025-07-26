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
            // Evict the cache before setting the data (no problem re-fetching)
            Evict(fileName);

            // Set directly to disk
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
                // AudioStation -> ATL:  Some properties are ready only. All fields have been put in their proper place by
                //                       this point. (see IAudioStationTag)
                //
                var atlTrack = TagMapper.MapTo(tag, fileName);

                // ATL:  Save (to ATL.Track.Path = fileName)
                atlTrack.Save();
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
                var tagFile = TagMapper.MapFrom(atlTrack);

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
