using System.IO;

using Newtonsoft.Json;

namespace AudioStation.Core.Component
{
    public static class Serializer
    {
        /// <summary>
        /// NOTE*** No try / catch - please wrap execution
        /// </summary>
        public static void Serialize<T>(T graph, string file)
        {
            var serializer = CreateSerializer();

            if (File.Exists(file))
                File.Delete(file);

            using (var stream = File.OpenWrite(file))
            {
                using (var writer = new StreamWriter(stream))
                {
                    serializer.Serialize(writer, graph);
                }
            }
        }

        /// <summary>
        /// NOTE*** No try / catch - please wrap execution
        /// </summary>
        public static void Serialize<T>(T graph, Stream stream)
        {
            var serializer = CreateSerializer();

            using (var writer = new StreamWriter(stream))
            {
                serializer.Serialize(writer, graph);
            }
        }

        /// <summary>
        /// NOTE*** No try / catch - please wrap execution
        /// </summary>
        public static T Deserialize<T>(string file)
        {
            var serializer = CreateSerializer();

            using (var stream = File.OpenRead(file))
            {
                using (var reader = new StreamReader(stream))
                {
                    return (T)serializer.Deserialize(reader, typeof(T));
                }
            }
        }

        /// <summary>
        /// NOTE*** No try / catch - please wrap execution
        /// </summary>
        public static T Deserialize<T>(Stream stream)
        {
            var serializer = CreateSerializer();

            using (var reader = new StreamReader(stream))
            {
                return (T)serializer.Deserialize(reader, typeof(T));
            }
        }

        /// <summary>
        /// NOTE*** No try / catch - please wrap execution
        /// </summary>
        public static T Deserialize<T>(byte[] buffer)
        {
            var serializer = CreateSerializer();

            using (var stream = new MemoryStream(buffer))
            {
                using (var reader = new StreamReader(stream))
                {
                    return (T)serializer.Deserialize(reader, typeof(T));
                }
            }
        }


        private static JsonSerializer CreateSerializer()
        {
            return new JsonSerializer()
            {
                Formatting = Formatting.Indented
            };
        }
    }
}
