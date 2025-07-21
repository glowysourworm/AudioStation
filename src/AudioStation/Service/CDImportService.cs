using System.IO;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.CDPlayer.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility;
using AudioStation.Service.Interface;

using NAudio.Lame;
using NAudio.Wave;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Service
{
    [IocExport(typeof(ICDImportService))]
    public class CDImportService : ICDImportService
    {
        private readonly IDialogController _dialogController;
        private readonly IConfigurationManager _configurationManager;
        private readonly ICDDrive _cdDrive;
        private readonly IFileController _fileController;

        [IocImportingConstructor]
        public CDImportService(IConfigurationManager configurationManager, 
                               IDialogController dialogController,
                               ICDDrive cdDrive,
                               IFileController fileController)
        {
            _configurationManager = configurationManager;
            _dialogController = dialogController;
            _cdDrive = cdDrive;
            _fileController = fileController;
        }

        public Task ImportTrack(int trackNumber, string artist, string album, int discNumber, int discCount, Action<double> progressCallback)
        {
            if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(album))
                throw new ArgumentException("Must have artist and album for the CD import");

            var configuration = _configurationManager.GetConfiguration();

            return Task.Run(() =>
            {
                var bufferData = new List<byte>();

                // Read Sectors -> Callback Progress %
                try
                {
                    _cdDrive.ReadTrack(trackNumber, (args) =>
                    {
                        bufferData.AddRange(args.Data);

                        ApplicationHelpers.BeginInvokeDispatcher(() =>
                        {
                            // Progress %
                            progressCallback(args.TotalBytesRead / (double)args.TotalBytesToRead);

                        }, DispatcherPriority.Background);
                    });

                    // Complete the progress bar (TODO: Problem knowing the last sector read)
                    ApplicationHelpers.BeginInvokeDispatcher(() => progressCallback(1), DispatcherPriority.Background);
                }
                catch (Exception ex)
                {
                    _dialogController.ShowAlert("CD-ROM Read Error", "There was an error reading from the CD-ROM", ex.Message);
                    return;
                }
                

                var directory = configuration.DownloadFolder;
                var artistFolder = _fileController.MakeFriendlyPath(false, artist);
                var albumFolder = _fileController.MakeFriendlyPath(false, album);
                var hasDiscFolder = discCount > 1;
                var discFolder = "Disc " + discNumber.ToString();

                var filePath = hasDiscFolder ? _fileController.MakeFriendlyPath(true, directory, artistFolder, albumFolder, discFolder, "Track" + trackNumber + ".mp3") :
                                               _fileController.MakeFriendlyPath(true, directory, artistFolder, albumFolder, "Track" + trackNumber + ".mp3");

                var artistDirectory = Path.Combine(directory, artistFolder);
                var albumDirectory = Path.Combine(directory, artistFolder, albumFolder);
                var discDirectory = Path.Combine(directory, artistFolder, albumFolder, discFolder);

                // Download -> Artist
                if (!Directory.Exists(artistDirectory))
                    Directory.CreateDirectory(artistDirectory);

                // Download -> Artist -> Album
                if (!Directory.Exists(albumDirectory))
                    Directory.CreateDirectory(albumDirectory);

                if (hasDiscFolder && !Directory.Exists(discDirectory))
                    Directory.CreateDirectory(discDirectory);

                // Collect the buffer into an array
                var buffer = bufferData.ToArray();

                // Convert .raw format to .wav format
                using (var reader = new RawSourceWaveStream(buffer, 0, buffer.Length, new WaveFormat()))
                {
                    ID3TagData tag = new ID3TagData
                    {
                        Title = "Track " + trackNumber.ToString(),
                        Artist = artist,
                        Album = album                                                
                    };

                    // NAudio.Lame (extension package)
                    using (var writer = new LameMP3FileWriter(filePath, reader.WaveFormat, 128, tag))
                    {
                        reader.CopyTo(writer);
                    }
                }
            });
        }
    }
}
