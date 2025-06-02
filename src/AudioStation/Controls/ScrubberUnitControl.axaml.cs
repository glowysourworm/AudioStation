using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AudioStation.Controls;

public partial class ScrubberUnitControl : UserControl
{
    public static readonly StyledProperty<float> CurrentOffsetProperty = StyledProperty<ScrubberUnitControl>.Register<ScrubberUnitControl, float>("CurrentOffset", 0);

    public float CurrentOffset
    {
        get { return (float)GetValue(CurrentOffsetProperty); }
        set { SetValue(CurrentOffsetProperty, value); }
    }
    
    public ScrubberUnitControl()
    {
        InitializeComponent();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        this.ScrubberCursor.Margin = new Thickness(e.NewSize.Width * this.CurrentOffset, 0, 0, 0);
    }
}