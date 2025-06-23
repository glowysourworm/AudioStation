using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    /// <summary>
    /// Event for entering / exiting expanded view
    /// </summary>
    public class NowPlayingExpandedViewEvent : IocEvent<bool>
    {
    }
}
