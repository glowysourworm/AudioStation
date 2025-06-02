using System;

using Avalonia;
using Avalonia.Controls;

namespace AudioStation.Controls;

public partial class ScrubberControl : UserControl
{
    public static readonly StyledProperty<TimeSpan> CurrentTimeProperty = StyledProperty<ScrubberControl>.Register<ScrubberControl, TimeSpan>("CurrentTime", TimeSpan.Zero);
    public static readonly StyledProperty<TimeSpan> TotalTimeProperty = StyledProperty<ScrubberControl>.Register<ScrubberControl, TimeSpan>("TotalTime", TimeSpan.Zero);

    public TimeSpan CurrentTime
    {
        get { return (TimeSpan)GetValue(CurrentTimeProperty); }
        set { SetValue(CurrentTimeProperty, value); }
    }

    public TimeSpan TotalTime
    {
        get { return (TimeSpan)GetValue(TotalTimeProperty); }
        set { SetValue(TotalTimeProperty, value); }
    }

    public ScrubberControl()
    {
        InitializeComponent();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (this.TotalTime.Milliseconds > 0)
            this.ScrubberCursor.Margin = new Thickness(e.NewSize.Width * this.CurrentTime.TotalMilliseconds / this.TotalTime.TotalMilliseconds, 0, 0, 0);

        else
            this.ScrubberCursor.Margin = new Thickness(0);
    }
}