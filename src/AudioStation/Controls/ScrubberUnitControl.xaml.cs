using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioStation.Controls
{
    public partial class ScrubberUnitControl : UserControl
    {
        public static readonly DependencyProperty CurrentOffsetProperty =
            DependencyProperty.Register("CurrentOffset", typeof(float), typeof(ScrubberUnitControl), new PropertyMetadata(0.0f));

        public static readonly DependencyProperty ScrubberHandleBrushProperty =
            DependencyProperty.Register("ScrubberHandleBrush", typeof(Brush), typeof(ScrubberUnitControl), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty ScrubberTimelineBrushProperty =
            DependencyProperty.Register("ScrubberTimelineBrush", typeof(Brush), typeof(ScrubberUnitControl), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty ScrubberTimelineSizeProperty =
            DependencyProperty.Register("ScrubberTimelineSize", typeof(int), typeof(ScrubberUnitControl), new PropertyMetadata(5));

        public static readonly DependencyProperty ScrubberHandleSizeProperty =
            DependencyProperty.Register("ScrubberHandleSize", typeof(int), typeof(ScrubberUnitControl), new PropertyMetadata(15));

        public float CurrentOffset
        {
            get { return (float)GetValue(CurrentOffsetProperty); }
            set { SetValue(CurrentOffsetProperty, value); }
        }
        public Brush ScrubberHandleBrush
        {
            get { return (Brush)GetValue(ScrubberHandleBrushProperty); }
            set { SetValue(ScrubberHandleBrushProperty, value); }
        }
        public Brush ScrubberTimelineBrush
        {
            get { return (Brush)GetValue(ScrubberTimelineBrushProperty); }
            set { SetValue(ScrubberTimelineBrushProperty, value); }
        }
        public int ScrubberTimelineSize
        {
            get { return (int)GetValue(ScrubberTimelineSizeProperty); }
            set { SetValue(ScrubberTimelineSizeProperty, value); }
        }
        public int ScrubberHandleSize
        {
            get { return (int)GetValue(ScrubberHandleSizeProperty); }
            set { SetValue(ScrubberHandleSizeProperty, value); }
        }
        public ScrubberUnitControl()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (!double.IsNaN(sizeInfo.NewSize.Width) &&
                !double.IsNaN(this.CurrentOffset))
                this.ScrubberCursor.Margin = new Thickness(sizeInfo.NewSize.Width * this.CurrentOffset, 0, 0, 0);
        }
    }
}
