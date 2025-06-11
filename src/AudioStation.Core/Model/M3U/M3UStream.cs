namespace AudioStation.Core.Model.M3U
{
    /// <summary>
    /// Representation of an M3U file standard (see parser for more information). We're only applying the
    /// older part of the standard. There's a lot of extra tags and processing that has gotten platform
    /// specific, newer for streaming video, and doesn't apply to streaming radio. Any errors for tag 
    /// parsing will be dealt with to see what we're missing.
    /// 
    /// Update:  This "File" spec. will be (was) shortened due to library usage of the details. 
    /// </summary>
    public class M3UStream
    {
        // Allowed platform values for duration (for streams)
        public const int DURATION_STREAMING_1 = -1;
        public const int DURATION_STREAMING_2 = 0;
        public const int DURATION_EMPTY = int.MinValue;

        public static readonly string TVG_LOGO_ATTRIBUTE = "tvg-logo";
        public static readonly string TVG_URL_ATTRIBUTE = "tvg-url";
        public static readonly string GROUP_TITLE_ATTRIBUTE = "group-title";

        /// <summary>
        /// #EXTINF:  Title for the stream / file. 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// #EXTINF:  Duration. For streams, the -1, or 0 allowed values signal the platform of a streaming media (see wikipedia).
        /// </summary>
        public int DurationSeconds { get; set; }

        /// <summary>
        /// This is the actual media entry. These would be preceded by an #EXTINF tag, or other header tag. The sequential processing
        /// of a file puts these into several playlists / groups / as well as the primary group. Other information about the entry must
        /// precede the stream source; and the location of the source may be either a file or a network location. This is not a binary
        /// source, nor does it come from the #EXTBIN (binary) tag data.
        /// </summary>
        public string StreamSource { get; set; }

        // Specific Attributes
        public string TvgLogo { get; set; }
        public string TvgHomepage { get; set; }
        public string GroupName { get; set; }

        /// <summary>
        /// #EXTINF:  Tag (optional) attribute(s) (name=value) pairs
        /// </summary>
        public Dictionary<string, string> InformationAttributes { get; set; }

        public M3UStream()
        {
            this.InformationAttributes = new Dictionary<string, string>();
            this.StreamSource = string.Empty;
            this.DurationSeconds = DURATION_EMPTY;
            this.Title = string.Empty;
        }
        public M3UStream(M3UStream copy)
        {
            this.InformationAttributes = new Dictionary<string, string>();
            this.StreamSource = copy.StreamSource;
            this.DurationSeconds = copy.DurationSeconds;
            this.Title = copy.Title;

            foreach (var pair in copy.InformationAttributes)
            {
                this.InformationAttributes.Add(pair.Key, pair.Value);
            }
        }
    }
}
