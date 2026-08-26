using System.ComponentModel.DataAnnotations;

namespace AudioStation.Core.Model
{
    /// <summary>
    /// Enum used to specify specific vendor names. These must align with database Vendor table.
    /// </summary>
    public enum VendorNames
    {
        [Display(Name = "AudioDB", ShortName = "AudioDB", Description = "AudioDB metadata service")]
        AudioDB,

        [Display(Name = "Discogs", ShortName = "Discogs", Description = "Discogs metadata service")]
        Discogs,

        [Display(Name = "iTunes", ShortName = "iTunes", Description = "iTunes metadata service")]
        iTunes,

        [Display(Name = "LastFm", ShortName = "LastFm", Description = "LastFm metadata service")]
        LastFm,

        [Display(Name = "MusicBrainz", ShortName = "MusicBrainz", Description = "Music Brainz metadata service")]
        MusicBrainz,

        [Display(Name = "Spotify", ShortName = "Spotify", Description = "Spotify metadata service")]
        Spotify
    }

    /// <summary>
    /// Enum used to specify specific file types. These must align with database FileType table.
    /// </summary>
    public enum FileTypes
    {
        [Display(Name = "AudioFile", ShortName = "AudioFile", Description = "Some sort of audio file")]
        AudioFile,

        [Display(Name = "FrontCover", ShortName = "FrontCover", Description = "Front cover artwork for an album")]
        FrontCover,

        [Display(Name = "BackCover", ShortName = "BackCover", Description = "Back cover artwork for an album")]
        BackCover,

        [Display(Name = "FanArt", ShortName = "FanArt", Description = "Artwork that originates from the FanArt.tv web service")]
        FanArt,
    }
}
