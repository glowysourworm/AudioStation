using System;
using System.Collections.Generic;
using System.Text;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Component.Interface
{
    /// <summary>
    /// Primary component of Audio Station core. Each of these components will be displayed to the user
    /// on the status bar as "working", or "not working". This interface will help them to know whether
    /// each piece has had some malfunction; and this interface will be called during startup to gather
    /// component status.
    /// </summary>
    public interface IAudioStationComponent
    {
        public enum Status
        {
            Idle = 0,
            Working = 1,
            Error = 2
        }

        /// <summary>
        /// Gets name of component
        /// </summary>
        string GetName();

        /// <summary>
        /// Gets display name of component
        /// </summary>
        string GetDisplayName();

        /// <summary>
        /// Signals a status change event
        /// </summary>
        event SimpleEventHandler<IAudioStationComponent, Status> StatusChangeEvent;

        /// <summary>
        /// Returns current status of component
        /// </summary>
        Status GetStatus();

        /// <summary>
        /// Performs startup task for component
        /// </summary>
        Task<Status> Initialize();

        /// <summary>
        /// Returns status message for the component
        /// </summary>
        string GetStatusMessage();
    }
}
