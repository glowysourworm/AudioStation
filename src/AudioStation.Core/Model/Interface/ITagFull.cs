using AudioStation.Core.Model.Vendor.IdSharp;

namespace AudioStation.Core.Model.Interface
{
    /// <summary>
    /// Interface that represents our full tag dataset (that we care about for most purposes). One primary
    /// goal is to keep performance optimal by lowering the tag cache size. Memory loading becomes a factcor
    /// very quickly. Reading and writing from the file will use ID3v2, ID3v1, and APE tag standards using
    /// IdSharp (https://github.com/judwhite/IdSharp) as recommended by (https://www.id3.org/). There may
    /// never be optimal performance here unless the library is picked over for memory consumption. Exception
    /// handling also plays a major role in how these libraries perform. (Tried:  TagLibSharp; ATL; CSCore)
    /// </summary>
    public interface ITagFull      // No inheritance forces our memory pattern
    {
        string? AlbumArtist { get; set; }                   // ID3V2 [TPE2]
        string? Album { get; set; }                         // ID3V2 [TALB]
        string? Artist { get; set; }                        // ID3V2 [TPE1]
        string? Title { get; set; }                         // ID3V2 [TIT2]

        string? Comment { get; set; }
        string? Copyright { get; set; }

        string? Genre { get; set; }                         // ID3V2 [TCON]
        int? TrackNumber { get; set; }                      // ID3V2 [TRK] String Format="{Track Number}/{Track Position}" (literally)
        int? TrackTotal { get; set; }                       // ID3V2 [TRK] String Format="{Track Number}/{Track Position}" (literally)
        int? MediaNumber { get; set; }                      // ID3V2 [TPOS] String Format="{Disc Number}/{Disc Position}" (literally)
        int? MediaTotal { get; set; }                       // ID3V2 [TPOS] String Format="{Disc Number}/{Disc Position}" (literally)
        string? MediaFormat { get; set; }                   // ID3V2 [TMED] ("CD", "Vinyl"..)
        string? Publisher { get; set; }                     // ID3V2 [TPUB] (Recording Label)
        int? DurationMilliseconds { get; set; }

        string? SortAlbumArtist { get; set; }               // ID3V2 [TSO2]
        string? SortArtist { get; set; }                    // ID3V2 [TSOP]
        string? SortAlbum { get; set; }
        string? SortTitle { get; set; }
        int? Year { get; set; }

        /// <summary>
        /// [TXXX] Field(s) from the ID3v2:  Many of these will be used by programs to show Music Brainz data (for instance)
        /// </summary>
        IList<UserDefinedField> UserDefinedFields { get; set; }

        /// <summary>
        /// [IPLS] Field from ID3v2:  This will be a character separated list (Try any / all separator characters, use semi-colon ;, unless there's problems)
        ///                           There may also be structured data:  "producer:Joe Smith;engineer:Nat Good"
        /// </summary>
        IList<InvolvedPerson> InvolvedPeople { get; set; }
    }
}
