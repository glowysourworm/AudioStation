using System.IO;

namespace AudioStation.Core.Utility.FileUtility
{
    public class FileHelpers
    {
        public static int CalculateCRC32(string filePath)
        {
            var crcHash = new System.IO.Hashing.Crc32();

            // CRC32 CALCULATION:  THIS IS NOT KEPT ON THE FILE SYSTEM
            var fileBytes = File.ReadAllBytes(filePath);

            crcHash.Append(fileBytes);

            return crcHash.GetCurrentHash(fileBytes);
        }
    }
}
