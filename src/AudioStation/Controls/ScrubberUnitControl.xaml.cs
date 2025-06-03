using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AudioStation.Controls
{
    public partial class ScrubberUnitControl : UserControl
    {
        public static readonly DependencyProperty CurrentOffsetProperty = 
            DependencyProperty.Register("CurrentOffset", typeof(float), typeof(ScrubberUnitControl));

        public float CurrentOffset
        {
            get { return (float)GetValue(CurrentOffsetProperty); }
            set { SetValue(CurrentOffsetProperty, value); }
        }

        public ScrubberUnitControl()
        {
            InitializeComponent();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            this.ScrubberCursor.Margin = new Thickness(sizeInfo.NewSize.Width * this.CurrentOffset, 0, 0, 0);
        }
    }
}
