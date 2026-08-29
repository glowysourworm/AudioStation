using System.ComponentModel.DataAnnotations;

namespace AudioStation.Core.Model
{
    public enum TrackGroupingType
    {
        [Display(Name = "None", ShortName = "None", Description = "Keep folders as is")]
        None = 0,

        [Display(Name = "Artist / Album", ShortName = "ArtistAlbum", Description = "../Artist/Album/... (Artist & Album required for imports)")]
        ArtistAlbum = 1,

        [Display(Name = "Genre / Artist / Album", ShortName = "GenreArtistAlbum", Description = "..Genre/Artist/Album/... (Genre required for imports)")]
        GenreArtistAlbum = 2,
    }
}
