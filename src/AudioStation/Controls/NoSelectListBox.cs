using System.Windows.Controls;
using System.Windows.Input;

namespace AudioStation.Controls
{
    public class NoSelectListBox : ListBox
    {
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
