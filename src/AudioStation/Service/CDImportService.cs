using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component.CDPlayer.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.LibraryLoaderViewModels;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Service
{
    [IocExport(typeof(ICDImportService))]
    public class CDImportService : ICDImportService
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly ICDDrive _cdDrive;

        [IocImportingConstructor]
        public CDImportService(IConfigurationManager configurationManager, ICDDrive cdDrive)
        {
            _configurationManager = configurationManager;
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
                _cdDrive.ReadTrack(trackNumber, (args) =>
                {
                    bufferData.AddRange(args.Data);

                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        // Progress %
                        progressCallback(args.TotalBytesRead / (double)args.TotalBytesToRead);

                    }, DispatcherPriority.Background);
                });

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

                File.WriteAllBytes(filePath, bufferData.ToArray());
            });
        }
    }
}
