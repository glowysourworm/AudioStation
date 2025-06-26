using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    /// <summary>
    /// Primary loading event corresponding to the MainViewModel.Loading indicator boolean. This will
    /// be utilized to hide / show loading UI and cursor.
    /// </summary>
    public class MainLoadingChangedEvent : IocEvent<bool>
    {
    }
}
