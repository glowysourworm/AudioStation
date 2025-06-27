using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace AudioStation.Core.Component.Vendor.RadioBrowser
{
    public class RadioBrowserStation
    {
        [JsonProperty("changeuuid")]
        public string Changeuuid { get; set; }

        [JsonProperty("stationuuid")]
        public string Stationuuid { get; set; }

        [JsonProperty("serveruuid")]
        public string Serveruuid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("url_resolved")]
        public string UrlResolved { get; set; }

        [JsonProperty("homepage")]
        public string Homepage { get; set; }

        [JsonProperty("favicon")]
        public string Favicon { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("countrycode")]
        public string Countrycode { get; set; }

        [JsonProperty("iso_3166_2")]
        public string Iso31662 { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("languagecodes")]
        public string Languagecodes { get; set; }

        [JsonProperty("votes")]
        public int Votes { get; set; }

        [JsonProperty("lastchangetime")]
        public string Lastchangetime { get; set; }

        [JsonProperty("lastchangetime_iso8601")]
        public DateTime LastchangetimeIso8601 { get; set; }

        [JsonProperty("codec")]
        public string Codec { get; set; }

        [JsonProperty("bitrate")]
        public int Bitrate { get; set; }

        [JsonProperty("hls")]
        public int Hls { get; set; }

        [JsonProperty("lastcheckok")]
        public int Lastcheckok { get; set; }

        [JsonProperty("lastchecktime")]
        public string Lastchecktime { get; set; }

        [JsonProperty("lastchecktime_iso8601")]
        public DateTime LastchecktimeIso8601 { get; set; }

        [JsonProperty("lastcheckoktime")]
        public string Lastcheckoktime { get; set; }

        [JsonProperty("lastcheckoktime_iso8601")]
        public DateTime LastcheckoktimeIso8601 { get; set; }

        [JsonProperty("lastlocalchecktime")]
        public string Lastlocalchecktime { get; set; }

        [JsonProperty("lastlocalchecktime_iso8601")]
        public DateTime LastlocalchecktimeIso8601 { get; set; }

        [JsonProperty("clicktimestamp")]
        public string Clicktimestamp { get; set; }

        [JsonProperty("clicktimestamp_iso8601")]
        public object ClicktimestampIso8601 { get; set; }

        [JsonProperty("clickcount")]
        public int Clickcount { get; set; }

        [JsonProperty("clicktrend")]
        public int Clicktrend { get; set; }

        [JsonProperty("ssl_error")]
        public int SslError { get; set; }

        [JsonProperty("geo_lat")]
        public double GeoLat { get; set; }

        [JsonProperty("geo_long")]
        public double GeoLong { get; set; }

        [JsonProperty("has_extended_info")]
        public bool HasExtendedInfo { get; set; }
    }
}
