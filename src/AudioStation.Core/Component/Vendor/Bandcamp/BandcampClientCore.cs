using System.Net.Http;

using BCDownloader.Models;

using HtmlAgilityPack;

using Newtonsoft.Json;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AudioStation.Core.Component.Vendor.Bandcamp
{
    // Replacing the nuget package (BCDownloader), mainly to put a limit on download intervals
    //
    public class BandcampClientCore
    {
        private readonly HttpClient _client;

        public BandcampClientCore()
        {
            _client = new HttpClient();
        }

        public async Task<AlbumInfo?> GetAlbumInfoAsync(string url, int waitPeriodMilliseconds)
        {
            var document = await GetDocumentAsync(url);
            var albumInfo = GetAlbumInfo(document);
            albumInfo.CoverData = await GetCoverData(document);

            if (albumInfo?.TrackInfo is not null)
            {
                var trackInfo = albumInfo.TrackInfo.ToList();
                albumInfo.TrackInfo.Clear();

                foreach (var info in trackInfo)
                {
                    Thread.Sleep(waitPeriodMilliseconds);

                    if (info.File?.DownloadPath is not null)
                    {
                        var response = await _client.GetAsync(info.File.DownloadPath);
                        info.Artist ??= albumInfo.Artist;
                        info.Data = await response.Content.ReadAsByteArrayAsync();
                        albumInfo.TrackInfo.Add(info);
                    }
                }

                return albumInfo;
            }

            return null;
        }

        private async Task<byte[]> GetCoverData(string document)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(document);
            var url = doc.DocumentNode.SelectSingleNode("//*[@id=\"tralbumArt\"]/a/img").GetAttributeValue("src", "");

            var response = await _client.GetAsync(url);

            return await response.Content.ReadAsByteArrayAsync();
        }

        private async Task<string> GetDocumentAsync(string url)
        {
            var response = await _client.GetAsync(url);

            return await response.Content.ReadAsStringAsync();
        }

        private AlbumInfo? GetAlbumInfo(string document)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(document);
            var albumInfo = JsonConvert.DeserializeObject<AlbumInfo>(doc.DocumentNode
                .SelectNodes("//script")
                .Where(x => x.Attributes.Contains("data-tralbum"))
                .FirstOrDefault()
                ?.GetAttributeValue("data-tralbum", "1")
                .Replace("&quot;", "\"") ?? string.Empty);

            return albumInfo;
        }
    }
}
