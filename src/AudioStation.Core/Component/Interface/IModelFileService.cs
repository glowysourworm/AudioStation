﻿using AudioStation.Core.Model;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

namespace AudioStation.Core.Component.Interface
{
    /// <summary>
    /// File service having to do with managing the model. The component responsibilities include: 1) Getting
    /// status on file related tasks and issues; and 2) Calculating any file paths and directories involved with
    /// managing these tasks.
    /// </summary>
    public interface IModelFileService
    {
        /// <summary>
        /// Calculates the file name of the tag's library file. This should be the file name stored in the database, also.
        /// </summary>
        string CalculateFileName(IAudioStationTag tagFile, LibraryEntryNamingType namingType);

        /// <summary>
        /// Calculates the folder path of a tag's library folder.
        /// </summary>
        string CalculateFolderPath(IAudioStationTag tagFile, string destinationFolderBase, LibraryEntryGroupingType groupingType);
    }
}
