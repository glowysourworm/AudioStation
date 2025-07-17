using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Component.CDPlayer.Interface
{
    public interface ICDDeviceNotifier
    {
        event SimpleEventHandler<CDDeviceChangeEventArgs> DeviceChange;
    }
}
