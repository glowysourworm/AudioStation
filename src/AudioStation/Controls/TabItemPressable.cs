using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AudioStation.Controls
{
    public class TabItemPressable : TabItem
    {
        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            set { SetValue(IsPressedProperty, value); }
        }

        public TabItemPressable()
        {
        }

        public static readonly DependencyProperty IsPressedProperty =
           DependencyProperty.Register("IsPressed", typeof(bool), typeof(TabItemPressable), new PropertyMetadata(false));

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            IsPressed = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            IsPressed = false;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            IsPressed = false;
        }
    }
}
