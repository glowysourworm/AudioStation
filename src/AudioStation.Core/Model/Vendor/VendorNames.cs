using System.ComponentModel.DataAnnotations;

namespace AudioStation.Core.Model.Vendor
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
}
