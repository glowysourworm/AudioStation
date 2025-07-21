using System.IO;
using System.Windows;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model.Vendor.TagLibExtension;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using NAudio.Wave;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.RecursiveSerializer.Shared;
using SimpleWpf.SimpleCollections.Collection;

using TagLib;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(ITagCacheController))]
    public class TagCacheController : ITagCacheController
    {
        SimpleDictionary<string, TagLib.File> _tagFiles;

        [IocImportingConstructor]
        public TagCacheController()
        {
            _tagFiles = new SimpleDictionary<string, TagLib.File>();
        }

        public TagLib.Tag CopyFromClipboard()
        {
            try
            {
                var buffer = (byte[])Clipboard.GetDataObject().GetData(typeof(byte[]));
                var tagExtension = (TagExtension)Deserialize(buffer);

                return tagExtension as TagLib.Tag;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error pasting tag data:  {0}", LogMessageType.LibraryLoader, LogLevel.Error, ex, ex.Message);
                return null;
            }
        }

        public bool CopyToClipboard(TagLib.Tag tag)
        {
            try
            {
                // Map to our tag extension class and serialize
                var tagExtension = ApplicationHelpers.Map<TagLib.Tag, TagExtension>(tag);
                var buffer = Serialize(tagExtension);

                Clipboard.SetDataObject(buffer);

                return true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error copying tag data:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                return false;
            }
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

        public TagLib.File Get(string fileName)
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
        public void Set(string fileName)
        {
            try
            {
                if (_tagFiles.ContainsKey(fileName))
                    _tagFiles.Remove(fileName);

                var tagFile = TagLib.File.Create(fileName);

                _tagFiles.Add(fileName, tagFile);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error reading tag data (rebuilding Mp3 file):  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
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

        public void SetData(string fileName, Tag tagData, bool save = true)
        {
            if (!_tagFiles.ContainsKey(fileName))
                Set(fileName);

            try
            {
                var existingFile = Get(fileName);

                ApplicationHelpers.MapOnto(tagData, existingFile.Tag);

                if (save)
                    existingFile.Save();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving tag data:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
        public byte[] Serialize(TagExtension serializableTag)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new RecursiveSerializer<TagLib.Tag>(new RecursiveSerializerConfiguration()
                {
                    IgnoreRemovedProperties = false,
                    PreviewRemovedProperties = false
                });

                serializer.Serialize(stream, serializableTag);

                return stream.GetBuffer();
            }
        }
        public TagExtension Deserialize(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                var serializer = new RecursiveSerializer<TagExtension>(new RecursiveSerializerConfiguration()
                {
                    IgnoreRemovedProperties = false,
                    PreviewRemovedProperties = false
                });

                stream.Seek(0, SeekOrigin.Begin);

                return serializer.Deserialize(stream);
            }
        }
    }
}
