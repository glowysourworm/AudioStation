using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    [Flags]
    public enum AudioControlPanelControl
    {
        None = 0,
        Volume = 1,
        Equalizer = 2
    }

    /// <summary>
    /// Event for show / hide control panel controls
    /// </summary>
    public class NowPlayingShowAudioControlPanelEvent : IocEvent<AudioControlPanelControl>
    {
    }
}
