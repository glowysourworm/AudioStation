using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace AudioStation.Controls
{
    public class MouseCapturePopup : Popup
    {


        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            //this.CaptureMouse();
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            //if (this.InputHitTest(e.GetPosition(this)) == null)
            //    this.ReleaseMouseCapture();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            //if (!this.IsMouseCaptured)
                base.OnMouseLeave(e);
        }
    }
}
