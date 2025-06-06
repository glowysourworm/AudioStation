using System.IO;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using AudioStation.Constant;
using AudioStation.Core.Model;

using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;

namespace AudioStation.Component
{
    public static class LastFmClient
    {
        public static async Task<ImageSource> DownloadArtwork(LibraryEntry entry)
        {
            //if (entry.IsUnknown(x => x.Album) ||
            //    entry.IsUnknown(x => x.AlbumArtists))
            //    return await Task.FromResult<ImageSource>(null);

            //else
           // {
                try
                {
                    // Last FM API
                    var client = new LastfmClient(WebConfiguration.LastFmAPIKey, WebConfiguration.LastFmAPISecret);

                    // Web Call ...
                    var response = await client.Album.GetInfoAsync(entry.PrimaryArtist, entry.Album, false);

                    // Status OK -> Create bitmap image from the url
                    if (response.Status == LastResponseStatus.Successful &&
                        response.Content.Images.Any())
                        return await DownloadImage(response.Content.Images.ExtraLarge.AbsoluteUri);

                    else
                        return await Task.FromResult<ImageSource>(null);
                }
                catch (Exception)
                {
                    return await Task.FromResult<ImageSource>(null);
                }
            //}
        }

        public static async Task<ImageSource> DownloadImage(string imageUrl)
        {
            try
            {
                var request = WebRequest.Create(imageUrl);
                var response = await request.GetResponseAsync();

                // NOTE*** ISSUE WITH AVALONIA UI BITMAP CONSTRUCTOR - HAD TO USE MEMORY STREAM
                using (var stream = response.GetResponseStream())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);

                        memoryStream.Seek(0, SeekOrigin.Begin);

                        var decoder = BitmapDecoder.Create(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                        return decoder.Frames[0];
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
