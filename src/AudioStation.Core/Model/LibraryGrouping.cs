using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility.FileUtility;

using SimpleWpf.Extensions;

namespace AudioStation.Core.Model
{
    public class LibraryGrouping
    {
        /// <summary>
        /// Iterator for handling iterating a directory tree by depth. The supposed entity for the current depth is supplied along with
        /// the format that produced it from an ITagSmall data structure.
        /// </summary>
        /// <param name="format">The product of the ITagSmall format attribute(s)</param>
        /// <param name="currentField">Current depth's field from the format</param>
        /// <param name="currentEntity">Current depth's library entity</param>
        /// <param name="currentCustomFormatPiece">Current depth's format piece:  (e.g. c:\root\my {artist} folder) would return "my {artist} folder" at depth=1</param>
        /// <param name="currentIndex">Current depth (index). Root = 0 is the convention.</param>
        public delegate void LibraryGroupingIterator(ILibraryFormat format, ILibraryFormatField? currentField, LibraryEntryType? currentEntity, string? currentCustomFormatPiece, int currentIndex);

        ILibraryFormat _format;
        TrackGroupingType _formatType;
        string? _customFormat;
        string[]? _customFormatPieces;
        Dictionary<int, List<ILibraryFormatField>> _depthFields;         // c:\my music\and {album}\there may not have\been\{artist}\any format
        Dictionary<int, List<LibraryEntryType>> _depthEntities;

        public LibraryGrouping(TrackGroupingType formatType, string? customFormat = null)
        {
            if (formatType == TrackGroupingType.Custom && string.IsNullOrWhiteSpace(customFormat))
                throw new ArgumentException("Must have custom format for this grouping type");

            _format = new LibraryFormat();
            _formatType = formatType;
            _customFormat = customFormat;
            _customFormatPieces = null;
            _depthEntities = new Dictionary<int, List<LibraryEntryType>>();
            _depthFields = new Dictionary<int, List<ILibraryFormatField>>();

            Initialize();
        }

        private void Initialize()
        {
            switch (_formatType)
            {
                case TrackGroupingType.None:
                    // Allow user to place any node type wherever they want in the tree
                    break;
                case TrackGroupingType.ArtistAlbum:
                    _depthEntities.Add(0, new List<LibraryEntryType>() { LibraryEntryType.Artist });
                    _depthEntities.Add(1, new List<LibraryEntryType>() { LibraryEntryType.Album });
                    break;
                case TrackGroupingType.GenreArtistAlbum:
                    _depthEntities.Add(0, new List<LibraryEntryType>() { LibraryEntryType.Genre });
                    _depthEntities.Add(1, new List<LibraryEntryType>() { LibraryEntryType.Artist });
                    _depthEntities.Add(2, new List<LibraryEntryType>() { LibraryEntryType.Album });
                    break;
                case TrackGroupingType.Custom:

                    if (string.IsNullOrWhiteSpace(_customFormat))
                        throw new Exception("Custom format not set for LibraryGrouping");

                    // Library Format:  Get a format, for any folder, with extra fields, that may be used to discern an ITagSmall (see attributes)
                    //
                    var format = MigrationHelpers.GetFormat<ITagSmall>(LibraryFormatUse.Folder, LibraryExtraneousFields.FileExtension);
                    var fieldNames = Enum.GetNames<LibraryEntryType>();
                    var _customFormatPieces = _customFormat.Split("\\", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var maxDepth = _customFormatPieces.Length - 1;

                    for (int depth = 0; depth <= maxDepth; depth++)
                    {
                        var currentfield = format.Fields
                                                 .FirstOrDefault(field => _customFormatPieces[depth]
                                                    .Contains(format.LeftToken + field.TokenName + format.RightToken));

                        if (!_depthEntities.ContainsKey(depth))
                            _depthEntities.Add(depth, new List<LibraryEntryType>());

                        if (!_depthFields.ContainsKey(depth))
                            _depthFields.Add(depth, new List<ILibraryFormatField>());

                        if (currentfield != null)
                        {
                            if (!fieldNames.Any(x => x == currentfield.Name))
                                throw new Exception("Field name not found in LibraryEntryType");

                            // The "object reference" isn't used.. (TODO)
                            var enumValue = LibraryEntryType.Artist.GetValue<LibraryEntryType>(currentfield.Name);

                            _depthEntities[depth].Add(enumValue);
                            _depthFields[depth].Add(currentfield);
                        }
                    }

                    break;
                default:
                    throw new Exception("Unhandled directory format (grouping) type");
            }
        }

        /// <summary>
        /// Returns the library entity (names) for the current directory's format string
        /// </summary>
        /// <param name="recursionDepth">Current recursion depth of the directory tree</param>
        /// <param name="directoryName">Current directory string:  (e.g.  "\My {genre} Folder")</param>
        /// <param name="libraryEntities">Library entity dictionary for storing the result</param>
        public bool Parse(int recursionDepth, string directoryName, out IDictionary<LibraryEntryType, string> libraryEntities)
        {
            libraryEntities = new Dictionary<LibraryEntryType, string>();

            switch (_formatType)
            {
                case TrackGroupingType.None:
                    return true;
                case TrackGroupingType.ArtistAlbum:
                {
                    if (recursionDepth == 0)
                    {
                        libraryEntities.Add(LibraryEntryType.Artist, directoryName);
                        return true;
                    }
                    else if (recursionDepth == 1)
                    {
                        libraryEntities.Add(LibraryEntryType.Album, directoryName);
                        return true;
                    }

                    return false;
                }
                case TrackGroupingType.GenreArtistAlbum:
                {
                    if (recursionDepth == 0)
                    {
                        libraryEntities.Add(LibraryEntryType.Genre, directoryName);
                        return true;
                    }
                    else if (recursionDepth == 1)
                    {
                        libraryEntities.Add(LibraryEntryType.Artist, directoryName);
                        return true;
                    }
                    else if (recursionDepth == 2)
                    {
                        libraryEntities.Add(LibraryEntryType.Album, directoryName);
                        return true;
                    }

                    return false;
                }
                case TrackGroupingType.Custom:
                {
                    if (_customFormatPieces == null ||
                        _customFormatPieces.Length <= recursionDepth)
                        throw new Exception("Custom format invalid for current recursion depth:  " + recursionDepth);


                    if (!_depthEntities.ContainsKey(recursionDepth))
                        return false;

                    var entities = _depthEntities[recursionDepth];
                    var fields = _depthFields[recursionDepth];
                    var formatString = _customFormatPieces[recursionDepth];
                    var fieldIndexTokens = new Dictionary<int, string>();
                    var fieldIndexInverseTokens = new Dictionary<int, string>();
                    var fieldEntityOccurrences = new Dictionary<int, LibraryEntryType>();
                    var resultString = directoryName;

                    foreach (var field in fields)
                    {
                        // The "object reference" isn't used.. (TODO)
                        var entityValue = LibraryEntryType.Artist.GetValue<LibraryEntryType>(field.Name);

                        var fieldTokenized = _format.LeftToken + field.TokenName + _format.RightToken;

                        // Field Occurrences: "\My {genre} Folder" has 1 occurrence of field "Genre".
                        //
                        var fieldOccurrences = formatString.AllIndices(fieldTokenized);
                        var indexFollower = 0;

                        // "Inverse" Occurrences:  "\My {genre} Folder" has two "inverse" pieces which will be
                        //                         needed to locate the name of the Genre field.
                        //
                        //                         "My Special Indie-Rock Folder" has "My ", and " Folder", which
                        //                         must be located to get the Genre field name "Special Indie-Rock"
                        //
                        foreach (var index in fieldOccurrences)
                        {
                            fieldIndexTokens.Add(index, fieldTokenized);
                            fieldIndexInverseTokens.Add(indexFollower, formatString.Substring(indexFollower, (index - indexFollower) + 1));
                            fieldEntityOccurrences.Add(index, entityValue);

                            // Set this to the end of the field occurrence
                            indexFollower = index + fieldTokenized.Length;
                        }
                    }

                    // Now, we can extract our field value(s)
                    var counter = 0;

                    foreach (var inverseToken in fieldIndexInverseTokens.Values)
                    {
                        var splitParts = resultString.SplitFirst(inverseToken);

                        if (splitParts.Length != 3)
                            throw new Exception("Invalid token split");

                        // Part 0:  inverseToken, Part 1: field value, Part 2: next string split
                        resultString = splitParts[2];

                        // Field Result (for this directory name)
                        libraryEntities.Add(fieldEntityOccurrences.ElementAt(counter).Value, splitParts[1]);
                    }

                    return true;
                }
                default:
                    throw new Exception("Unhandled directory format (grouping) type");
            }
        }
    }
}
