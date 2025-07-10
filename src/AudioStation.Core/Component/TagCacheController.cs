using System.IO;
using System.Windows;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model.Vendor.TagLibExtension;
using AudioStation.Core.Utility;
using AudioStation.Model;

using AutoMapper;

using Microsoft.Extensions.Logging;

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
                ApplicationHelpers.Log("Error pasting tag data:  {0}", LogMessageType.LibraryLoader, LogLevel.Error, ex.Message);
                return null;
            }
        }

        public bool CopyToClipboard(TagLib.Tag tag)
        {
            try
            {
                // Map to our tag extension class and serialize
                var tagExtension = Map<TagLib.Tag, TagExtension>(tag);
                var buffer = Serialize(tagExtension);

                Clipboard.SetDataObject(buffer);

                return true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error copying tag data:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
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
                ApplicationHelpers.Log("Error initializing tag data:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                return null;
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
                ApplicationHelpers.Log("Error initializing tag data:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }
        }

        public TDestination Map<TSource, TDestination>(TSource tagSource) 
            where TSource : TagLib.Tag
            where TDestination : TagLib.Tag
        {
            try
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<TSource, TDestination>();
                });

                var mapper = config.CreateMapper();

                // Use our implementation for an instance
                var tagDestination = Activator.CreateInstance(typeof(TDestination));

                return (TDestination)mapper.Map(tagSource, tagDestination, typeof(TSource), typeof(TDestination));
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error mapping tag types:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                throw ex;
            }
        }

        public TDestination MapOnto<TSource, TDestination>(TSource tagSource, TDestination tagDestination)
            where TSource : TagLib.Tag
            where TDestination : TagLib.Tag
        {
            try
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<TSource, TDestination>();
                });

                var mapper = config.CreateMapper();

                return (TDestination)mapper.Map(tagSource, tagDestination, typeof(TSource), typeof(TDestination));
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error mapping tag types:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
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
