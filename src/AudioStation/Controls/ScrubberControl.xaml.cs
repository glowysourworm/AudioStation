using System.Windows;
using System.Windows.Controls;

namespace AudioStation.Controls
{
    public partial class ScrubberControl : UserControl
    {
        public static readonly DependencyProperty CurrentTimeProperty
            = DependencyProperty.Register("CurrentTime", typeof(TimeSpan), typeof(ScrubberControl));

        public static readonly DependencyProperty TotalTimeProperty =
            DependencyProperty.Register("TotalTime", typeof(TimeSpan), typeof(ScrubberControl));

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

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (this.TotalTime.Milliseconds > 0)
                this.ScrubberCursor.Margin = new Thickness(sizeInfo.NewSize.Width * this.CurrentTime.TotalMilliseconds / this.TotalTime.TotalMilliseconds, 0, 0, 0);

            else
                this.ScrubberCursor.Margin = new Thickness(0);
        }
    }
}
