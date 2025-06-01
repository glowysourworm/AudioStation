using System.IO;

using SimpleWpf.RecursiveSerializer.Shared;

namespace AudioStation.Component
{
    public static class Serializer
    {
        /// <summary>
        /// NOTE*** No try / catch - please wrap execution
        /// </summary>
        public static void Serialize<T>(T graph, string file)
        {
            var serializer = new RecursiveSerializer<T>(new RecursiveSerializerConfiguration()
            {
                IgnoreRemovedProperties = false,
                PreviewRemovedProperties = false
            });

            if (File.Exists(file))
                File.Delete(file);

            using (var stream = File.OpenWrite(file))
            {
                serializer.Serialize(stream, graph);
            }
        }

        /// <summary>
        /// NOTE*** No try / catch - please wrap execution
        /// </summary>
        public static void Serialize<T>(T graph, Stream stream)
        {
            var serializer = new RecursiveSerializer<T>(new RecursiveSerializerConfiguration()
            {
                IgnoreRemovedProperties = false,
                PreviewRemovedProperties = false
            });

            serializer.Serialize(stream, graph);
        }

        /// <summary>
        /// NOTE*** No try / catch - please wrap execution
        /// </summary>
        public static T Deserialize<T>(string file)
        {
            var serializer = new RecursiveSerializer<T>(new RecursiveSerializerConfiguration()
            {
                IgnoreRemovedProperties = false,
                PreviewRemovedProperties = false
            });

            using (var stream = File.OpenRead(file))
            {
                return (T)serializer.Deserialize(stream);
            }
        }

        /// <summary>
        /// NOTE*** No try / catch - please wrap execution
        /// </summary>
        public static T Deserialize<T>(Stream stream)
        {
            var serializer = new RecursiveSerializer<T>(new RecursiveSerializerConfiguration()
            {
                IgnoreRemovedProperties = false,
                PreviewRemovedProperties = false
            });

            return (T)serializer.Deserialize(stream);
        }

        /// <summary>
        /// NOTE*** No try / catch - please wrap execution
        /// </summary>
        public static T Deserialize<T>(byte[] buffer)
        {
            var serializer = new RecursiveSerializer<T>(new RecursiveSerializerConfiguration()
            {
                IgnoreRemovedProperties = false,
                PreviewRemovedProperties = false
            });

            using (var stream = new MemoryStream(buffer))
            {
                return (T)serializer.Deserialize(stream);
            }
        }

    }
}
