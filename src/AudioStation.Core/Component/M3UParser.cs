using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.RegularExpressions;

using AudioStation.Core.Model.M3U;

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Event;
using SimpleWpf.NativeIO;

namespace AudioStation.Core.Component
{
    public static class M3UParser
    {
        // Wikipedia:  https://en.wikipedia.org/wiki/M3U
        //
        // Apple used the extended M3U format as a base for their HTTP Live Streaming(HLS)
        // which was documented in an Independent Submission Stream RFC in 2017 as RFC 8216.
        // Therein, a master playlist references segment playlists which usually contain URLs
        // for short parts of the media stream.Some tags only apply to the former type and
        // some only to the latter type of playlist, but they all begin with #EXT-X-.
        //
        // Apple (2017):  https://datatracker.ietf.org/doc/html/rfc8216
        //

        // Assume the first group of headers (the old standard) is mostly for single-file m3u's. It
        // is probably not followed exactly; and may depend on what #EXT-X- tags are also included in
        // the file; and also on what device / platform expects them to be a certain way / format.
        //
        public const string M3U_HEADER = "#EXTM3U";
        public const string M3U_HEADER_INFORMATION = "#EXTINF";                                        // (see function below:  ParseTrackInformation)
        public const string M3U_HEADER_PLAYLIST = "#PLAYLIST";                                         // Playlist Display Title
        public const string M3U_HEADER_BEGIN_GROUPING = "#EXTGRP";                                     // Begin Named Group
        public const string M3U_HEADER_ALBUM_TITLE = "#EXTALB";                                        // Display Album Title
        public const string M3U_HEADER_ALBUM_ARTIST = "#EXTART";                                       // Display Album Artist
        public const string M3U_HEADER_ALBUM_GENRE = "#EXTGENRE";                                      // Display Album Genre
        public const string M3U_HEADER_PLAYLIST_FOR_TRACKS = "#EXTM3A";                                // Playlist (in current file) tracks
        public const string M3U_HEADER_FILE_SIZE_BYTES = "#EXTBYT";                                    // File Size in Bytes
        public const string M3U_HEADER_FILE_DATA = "#EXTBIN";                                          // Binary Data (mp3 (!?))
        public const string M3U_HEADER_URL_ALBUM_ART = "#EXTALBUMARTURL";                              // Album Art URL

        // #EXT-X-
        public const string M3U_EXT_HEADER_TIME_OFFSET = "#EXT-X-START";                                // TIME-OFFSET=0
        public const string M3U_EXT_HEADER_SEGMENTS_TOGGLE = "#EXT-X-INDEPENDENT-SEGMENTS";
        public const string M3U_EXT_HEADER_PLAYLIST_TYPE = "#EXT-X-PLAYLIST-TYPE";                      // VOD (or) EVENT
        public const string M3U_EXT_HEADER_TARGET_DURATION_SECONDS = "#EXT-X-TARGET-DURATION";
        public const string M3U_EXT_HEADER_TARGET_VERSION = "#EXT-X-VERSION";
        public const string M3U_EXT_HEADER_MEDIA_SEQUENCE = "#EXT-X-MEDIA-SEQUENCE";                    // Where the current playlist began (in this file)
        public const string M3U_EXT_HEADER_MEDIA = "#EXT-X-MEDIA";                                      // NAME="English", TYPE=AUDIO, GROUP-ID="audio-stereo-64", LANGUAGE="en", DEFAULT=YES, AUTOSELECT=YES, URI="english.m3u8"
        public const string M3U_EXT_HEADER_STREAM_INF = "#EXT-X-STREAM-INF";                            // (comma separators) BANDWIDTH=1123000, CODECS="avc1.64001f,mp4a.40.2"
        public const string M3U_EXT_HEADER_BYTERANGE = "#EXT-X-BYTERANGE";                              // 1024@256000
        public const string M3U_EXT_HEADER_DISCONTINUITY_TOGGLE = "#EXT-X-DISCONTINUITY";               // Sequence discontinuity start/end
        public const string M3U_EXT_HEADER_DISCONTINUITY_SEQUENCE = "#EXT-X-DISCONTINUITY-SEQUENCE";    // Start of sequence "period" (numbering)
        public const string M3U_EXT_HEADER_GAP_TOGGLE = "#EXT-X-GAP";                                   // Spacer before a new period
        public const string M3U_EXT_HEADER_ENCRYPTION = "#EXT-X-KEY";                                   // METHOD=NONE
        public const string M3U_EXT_HEADER_MAP = "#EXT-X-MAP";                                          // URI=MediaInitializationSection
        public const string M3U_EXT_HEADER_PROGRAM_TIMESTAMP = "#EXT-X-PROGRAM-DATE-TIME";              // ISO 8601
        public const string M3U_EXT_HEADER_DATERANGE = "#EXT-X-DATERANGE";                              // ID=foo (?? not used i guess)
        public const string M3U_EXT_HEADER_IFRAME_TOGGLE = "#EXT-X-I-FRAMES-ONLY";                      // Toggle w/o parameters
        public const string M3U_EXT_HEADER_SESSION_DATA = "#EXT-X-SESSION-DATA";                        // DATA-ID=com.example.movie.title
        public const string M3U_EXT_HEADER_SESSION_KEY = "#EXT-X-SESSION-KEY";
        public const string M3U_EXT_HEADER_ENDLIST = "#EXT-X-ENDLIST";                                  // End of list. Signal w/o parameters.

        // M3U8
        //
        // The Unicode version of M3U is M3U8, which uses UTF-8-encoded characters. M3U8 files
        // are the basis for the HTTP Live Streaming (HLS) format originally developed by Apple
        // to stream video and radio to iOS devices, and which is now a popular format for
        // adaptive streaming in general.
        //
        // The 2015 proposal for the HLS playlist format uses UTF-8 exclusively and does not
        // distinguish between the "m3u" and "m3u8" file name extensions.
        //

        // This will find [attribute-name]="[attribute-value]" handling N number of white-spaces
        public const string REGEX_ATTRIBUTE = "([a-zA-Z0-9_\\-\\.]+)\\s*=\\s*\"([a-zA-Z0-9_\\-\\.\\:\\/\\s]+)\"";

        public static List<M3UStream> Parse(string fileName, SimpleEventHandler<string, LogLevel> logHandler)
        {
            var result = new List<M3UStream>();

            //var memoryMappedFile = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open);
            //var fileLength = new FileInfo(fileName).Length;

            /*
            var fileLength = new FileInfo(fileName).Length;

            using (var reader = new StreamReader(fileName, Encoding.Default, true, new FileStreamOptions()
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                BufferSize = 0,
            }))
            */

            long bufferSize = 1000000;

            // Create streams for TINY pieces of the data - since we're reading sequentially. Just read for one line, then create a 
            // new stream from the file. I'll bet this works very well.
            //
            using (var stream = new FastFileStream(fileName, bufferSize))
            {
                var cursor = 0;
                var currentLine = string.Empty;
                var currentPlaylist = string.Empty;
                M3UStream currentMedia = null;

                long bytesLeft = 0;
                long position = 0;
                var endOfStream = false;

                do
                {
                    // NOTE** The end of stream implies that this is the final pass
                    var lines = stream.ReadLines(ref position, out bytesLeft, out endOfStream);

                    foreach (var line in lines)
                    {
                        currentLine = line.Trim();

                        if (string.IsNullOrWhiteSpace(currentLine))
                            continue;

                        // Header
                        if (currentLine.StartsWith(M3U_HEADER))
                            continue;

                        // Tag
                        else if (currentLine.StartsWith("#") && currentLine.Contains(':'))
                        {
                            var tagName = currentLine.Substring(0, currentLine.IndexOf(':'));
                            var tagValue = currentLine.Replace(tagName + ":", "").Trim();

                            switch (tagName)
                            {
                                // Begin "Old" standard format. Base Tags. (Actually, got rid of most unused tags for our purposes)

                                case M3U_HEADER_INFORMATION:
                                {
                                    currentMedia = new M3UStream();

                                    if (ParseTrackInformation(currentLine, ref currentMedia))
                                    {
                                        result.Add(currentMedia);

                                        // Fill out special attributes
                                        if (currentMedia.InformationAttributes.ContainsKey(M3UStream.TVG_URL_ATTRIBUTE))
                                            currentMedia.TvgHomepage = currentMedia.InformationAttributes[M3UStream.TVG_URL_ATTRIBUTE];

                                        if (currentMedia.InformationAttributes.ContainsKey(M3UStream.TVG_LOGO_ATTRIBUTE))
                                            currentMedia.TvgLogo = currentMedia.InformationAttributes[M3UStream.TVG_LOGO_ATTRIBUTE];

                                        if (currentMedia.InformationAttributes.ContainsKey(M3UStream.GROUP_TITLE_ATTRIBUTE))
                                            currentMedia.GroupName = currentMedia.InformationAttributes[M3UStream.GROUP_TITLE_ATTRIBUTE];
                                    }
                                    else
                                    {
                                        logHandler(string.Format("Parse Error: File={0} Line={1}", fileName, cursor), LogLevel.Error);
                                        currentMedia = null;
                                    }
                                }
                                break;

                                // BEGIN #EXT-X- (Extended M3U standard - includes multi-file playlists)

                                // Endlist (HasEndlist in m3uParser.NET) (multi-file endlist tag)
                                //case M3U_EXT_HEADER_ENDLIST:
                                //    break;

                                default:
                                    logHandler(string.Format("Unhandled M3U tag:  {0} Line={1} File={2}", tagName, cursor, fileName), LogLevel.Warning);
                                    break;
                            }
                        }

                        // Media File (or Network Source)
                        else
                        {
                            // Must fill out the stream source for the current media
                            if (currentMedia != null && string.IsNullOrEmpty(currentMedia.StreamSource))
                                currentMedia.StreamSource = currentLine.Trim();

                            // Next Media:  Adds another track. Must also put into current group, and current playlist.
                            else if (currentMedia != null)
                            {
                                // Copy Media Info
                                currentMedia = new M3UStream(currentMedia);
                                currentMedia.StreamSource = currentLine.Trim();
                            }

                            else
                            {
                                logHandler("#EXTINF tag not specified prior to track list (or parse error occurred):  " + currentLine.Trim(), LogLevel.Error);
                            }
                        }

                        cursor++;
                        //reader.DiscardBufferedData();
                        //reader.BaseStream.Flush();
                    }

                } while (!endOfStream);
            }

            return result;
        }

        public static bool ParseTrackInformation(string fileLine, ref M3UStream media)
        {
            // #EXTINF:[duration seconds] [optional-attribute1]...[optional=attributeN], [title]

            var commaSplit = fileLine.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (commaSplit.Length != 2)
                return false;

            // Start right after the colon
            commaSplit[0] = commaSplit[0].Substring(M3U_HEADER_INFORMATION.Length + 1, commaSplit[0].Length - M3U_HEADER_INFORMATION.Length - 1);

            // Duration + Attributes (Leave this string untrimmed until we've parsed out the inner attributes)
            var firstSplitAttribute = commaSplit[0].IndexOf(' ');

            // String left to parse out the duration
            if (commaSplit[0].Length > 0 && firstSplitAttribute >= 0)
            {
                var duration = -1;
                if (!int.TryParse(commaSplit[0].Substring(0, firstSplitAttribute + 1), out duration))
                    return false;

                media.DurationSeconds = duration;

                // Continue..
                commaSplit[0] = commaSplit[0].Substring(firstSplitAttribute + 1, commaSplit[0].Length - firstSplitAttribute - 1);
            }

            // Otherwise, just parse out the duration
            else if (commaSplit[0].Length > 0)
            {
                var duration = -1;
                if (!int.TryParse(commaSplit[0], out duration))
                    return false;

                media.DurationSeconds = duration;

                commaSplit[0] = "";  // Finished...
            }

            // Use Regex to match the rest of the attributes
            if (commaSplit[0].Length > 0)
            {
                var matcher = (MatchCollection)Regex.Matches(commaSplit[0], REGEX_ATTRIBUTE, RegexOptions.Compiled);

                // Captures are [name]="[value]"
                foreach (Match match in matcher)
                {
                    // Regex identified pieces of the attribute. This is permissive for spaces, and other chars
                    // inside the attribute value. The attribute value has double quotes around it.
                    //
                    var attributePieces = match.Value.Split('=');

                    media.InformationAttributes.Add(attributePieces[0].Trim(),
                                                    attributePieces[1].Trim().Substring(1, attributePieces[1].Length - 2)); // Double Quotes (trimmed)
                }
            }

            // Title
            media.Title = commaSplit[1];

            return true;
        }
    }
}
