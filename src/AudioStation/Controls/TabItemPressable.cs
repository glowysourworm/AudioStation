using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SimpleWpf.Extensions.Command;

namespace AudioStation.Controls
{
    public class TabItemPressable : TabItem
    {
        public static readonly DependencyProperty IsPressedProperty =
           DependencyProperty.Register("IsPressed", typeof(bool), typeof(TabItemPressable), new PropertyMetadata(false));

        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(SimpleCommand), typeof(TabItemPressable));

        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            set { SetValue(IsPressedProperty, value); }
        }

        // Custom Close Command - must be bound to the view model; AND the template.
        public SimpleCommand CloseCommand
        {
            get { return (SimpleCommand)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        public TabItemPressable()
        {
        }

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
