using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using AudioStation.Component;

namespace AudioStation.Controls
{
    /// <summary>
    /// Image that supports lazy loading from a file
    /// </summary>
    public class LibraryImageControl : Image
    {
        public static readonly DependencyProperty ImageFileProperty =
            DependencyProperty.Register("ImageFile", typeof(string), typeof(LibraryImageControl), new PropertyMetadata(string.Empty, OnImageFileChanged));

        public string ImageFile
        {
            get { return (string)GetValue(ImageFileProperty); }
            set { SetValue(ImageFileProperty, value); }
        }

        public LibraryImageControl()
        {
            this.Unloaded += LibraryImageControl_Unloaded;
            this.IsVisibleChanged += LibraryImageControl_IsVisibleChanged;
        }

        ~LibraryImageControl()
        {
            // DESTRUCTOR CALED FROM NON-DISPATCHER ?!?
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                this.Unloaded -= LibraryImageControl_Unloaded;
                this.IsVisibleChanged -= LibraryImageControl_IsVisibleChanged;

            }, DispatcherPriority.ApplicationIdle);            
        }

        private void LibraryImageControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Source = null;
        }
        private void LibraryImageControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible &&
                !string.IsNullOrEmpty(this.ImageFile) &&
                this.Source == null)
            {
                LoadImageAsync();
            }
        }

        private void LoadImageAsync()
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                this.Source = LibraryImageCache.Get(this.ImageFile).FirstOrDefault();

            }, DispatcherPriority.ApplicationIdle);
        }

        private static void OnImageFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as LibraryImageControl;
            var imageFile = e.NewValue as string;

            if (control != null && !string.IsNullOrEmpty(imageFile))
            {
                control.LoadImageAsync();
            }
        }
    }
}
