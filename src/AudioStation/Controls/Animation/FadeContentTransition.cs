using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AudioStation.Controls.Animation
{
    public class FadeContentTransition : ContentTransition
    {
        public Duration Time { get; set; }

        public FadeContentTransition()
        {
            this.Time = new Duration(new TimeSpan(0, 0, 0, 0, 300));
        }

        public override void BeginTransition(ContentControl transitionElement, FrameworkElement oldContent, FrameworkElement newContent)
        {
            var fadeOutAnimation = new DoubleAnimation(1.0, 0.0, this.Time);

            fadeOutAnimation.Completed += (obj, e) =>
            {
                oldContent.BeginAnimation(FrameworkElement.OpacityProperty, null);

                EndTransition(transitionElement, oldContent, newContent);
            };

            oldContent.BeginAnimation(FrameworkElement.OpacityProperty, fadeOutAnimation);

        }
        public override void EndTransition(ContentControl transitionElement, FrameworkElement oldContent, FrameworkElement newContent)
        {
            transitionElement.Content = newContent;

            oldContent.Opacity = 1.0;
        }
    }
}
