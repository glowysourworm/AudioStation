using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioStation.Core.Component.CDPlayer.Interface;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Component.CDPlayer
{
    public class CDDeviceNotifier : ICDDeviceNotifier
    {
        public event SimpleEventHandler<CDDeviceChangeEventArgs> DeviceChange;


    }
}
