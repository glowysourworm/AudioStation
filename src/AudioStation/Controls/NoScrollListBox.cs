using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AudioStation.Controls
{
    public class NoScrollListBox : ListBox
    {
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            // Handle (this) scroll (cancels the event)
            e.Handled = true;

            // Create a new event and raise it for any listener
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);

            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = this;

            RaiseEvent(eventArg);
        }
    }
}
