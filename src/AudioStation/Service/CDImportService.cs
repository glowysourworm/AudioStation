using System.IO;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.CDPlayer.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility;
using AudioStation.Service.Interface;

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

        [IocImportingConstructor]
        public CDImportService(IConfigurationManager configurationManager, 
                               IDialogController dialogController,
                               ICDDrive cdDrive)
        {
            _configurationManager = configurationManager;
            _dialogController = dialogController;
            _cdDrive = cdDrive;
        }

        public Task ImportTrack(int trackNumber, string artist, string album, Action<double> progressCallback)
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

                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            // Progress %
                            progressCallback(args.TotalBytesRead / (double)args.TotalBytesToRead);

                        }, DispatcherPriority.Background);
                    });
                }
                catch (Exception ex)
                {
                    _dialogController.ShowAlert("CD-ROM Read Error", "There was an error reading from the CD-ROM", ex.Message);
                    return;
                }
                

                var directory = configuration.DownloadFolder;
                var artistFolder = StringHelpers.MakeFriendlyPath(false, artist);
                var albumFolder = StringHelpers.MakeFriendlyPath(false, album);
                var filePath = StringHelpers.MakeFriendlyPath(true, directory, artistFolder, albumFolder, "Track" + trackNumber + ".wav");

                var artistDirectory = Path.Combine(directory, artistFolder);
                var albumDirectory = Path.Combine(directory, artistFolder, albumFolder);

                // Download -> Artist
                if (!Directory.Exists(artistDirectory))
                    Directory.CreateDirectory(artistDirectory);

                // Download -> Artist -> Album
                if (!Directory.Exists(albumDirectory))
                    Directory.CreateDirectory(albumDirectory);

                // Collect the buffer into an array
                var buffer = bufferData.ToArray();

                // Convert .raw format to .wav format
                using (var rawWaveStream = new RawSourceWaveStream(buffer, 0, buffer.Length, new WaveFormat()))
                {
                    WaveFileWriter.CreateWaveFile(filePath, rawWaveStream);
                }
            });
        }
    }
}
