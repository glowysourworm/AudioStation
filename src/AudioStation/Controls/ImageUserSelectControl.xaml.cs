using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioStation.Controls
{
    public partial class ImageUserSelectControl : UserControl
    {
        public static readonly DependencyProperty UserImageSourceProperty =
            DependencyProperty.Register("UserImageSource", typeof(ImageSource), typeof(ImageUserSelectControl));

        public ImageSource UserImageSource
        {
            get { return (ImageSource)GetValue(UserImageSourceProperty); }
            set { SetValue(UserImageSourceProperty, value); }
        }

        public ImageUserSelectControl()
        {
            InitializeComponent();
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
