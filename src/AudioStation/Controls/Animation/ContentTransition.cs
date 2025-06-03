using System.Windows;
using System.Windows.Controls;

namespace AudioStation.Controls.Animation
{
    public abstract class ContentTransition
    {
        public virtual void BeginTransition(ContentControl transitionElement, FrameworkElement oldContent, FrameworkElement newContent)
        {

        }
        public virtual void EndTransition(ContentControl transitionElement, FrameworkElement oldContent, FrameworkElement newContent)
        {

        }
    }
}
