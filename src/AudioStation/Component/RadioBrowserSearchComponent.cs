using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;

using AudioStation.Component.RadioBrowser;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

using Newtonsoft.Json;

using RadioBrowser;
using RadioBrowser.Api;
using RadioBrowser.Models;

using static System.Net.WebRequestMethods;

namespace AudioStation.Component
{
    public static class RadioBrowserSearchComponent
    {
        /// <summary>
        /// This was one of their "public docs"... I will assume as-is.
        /// </summary>
        private static string GetApiServer()
        {
            // https://api.radio-browser.info/
            return "fi1.api.radio-browser.info";

            // Get fastest ip of dns
            string baseUrl = @"all.api.radio-browser.info";
            var ips = Dns.GetHostAddresses(baseUrl);

            long lastRoundTripTime = long.MaxValue;
            string searchUrl = @"de2.api.radio-browser.info"; // Fallback

            foreach (IPAddress ipAddress in ips)
            {
                var reply = new Ping().Send(ipAddress);
                if (reply != null &&
                    reply.RoundtripTime < lastRoundTripTime)
                {
                    lastRoundTripTime = reply.RoundtripTime;
                    searchUrl = ipAddress.ToString();
                }
            }

            // Get clean name
            IPHostEntry hostEntry = Dns.GetHostEntry(searchUrl);
            if (!string.IsNullOrEmpty(hostEntry.HostName))
            {
                searchUrl = hostEntry.HostName;
            }

            return searchUrl;
        }

        public static Task<List<RadioBrowserStation>> SearchStation(string search)
        {
            return Task<List<RadioBrowserStation>>.Run(async () =>
            {
                // Initialization
                var apiUrl = GetApiServer();

                // Radio Browser Docs:  https://docs.radio-browser.info/?json#list-of-radio-stations
                var url = "https://" + apiUrl + "/json/stations/search?name=" + search;

                // Searching by name
                var httpClient = new HttpClient();
                var jsonResult = await httpClient.GetStringAsync(url);


                return JsonConvert.DeserializeObject<List<RadioBrowserStation>>(jsonResult, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore                    
                });

                // Advanced searching
                /*
                var advancedSearch = await radioBrowser.Search.AdvancedAsync(new AdvancedSearchOptions
                {                    
                    Language = "english",
                    TagList = "news",
                    Limit = 5
                });
                */
            });
        }

        public static Task<List<RadioBrowserStation>> GetTopStations(uint numberOfStations)
        {
            return Task<List<RadioBrowserStation>>.Run(async () =>
            {
                // Initialization
                var apiUrl = GetApiServer();

                // Radio Browser Docs:  https://docs.radio-browser.info/?json#list-of-radio-stations
                var url = apiUrl + "/json/stations/topvote/" + numberOfStations.ToString();

                // Searching by name
                var httpClient = new HttpClient();
                var jsonResult = await httpClient.GetStringAsync(url);

                return JsonConvert.DeserializeObject<List<RadioBrowserStation>>(jsonResult);
            });
        }
    }
}
