using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using AudioStation.Component;
using AudioStation.Core.Component.Interface;
using AudioStation.ViewModel.LibraryViewModels;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Controls
{
    public class LibraryArtistImage : Image
    {
        private readonly IModelController _modelController;

        protected string CurrentImageFile { get; private set; }
        protected string LastImageFile { get; private set; }
        protected ArtistViewModel ViewModel { get; private set; }

        public LibraryArtistImage()
        {
            // IocRegion doesn't have support for templates (easily)
            _modelController = IocContainer.Get<IModelController>();

            this.Unloaded += LibraryImageControl_Unloaded;
            this.Loaded += LibraryArtistImage_Loaded;
            this.IsVisibleChanged += LibraryImageControl_IsVisibleChanged;
            this.DataContextChanged += LibraryArtistImage_DataContextChanged;
        }

        ~LibraryArtistImage()
        {
            // DESTRUCTOR CALED FROM NON-DISPATCHER ?!?
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                this.Unloaded -= LibraryImageControl_Unloaded;
                this.IsVisibleChanged -= LibraryImageControl_IsVisibleChanged;
                this.DataContextChanged -= LibraryArtistImage_DataContextChanged;

            }, DispatcherPriority.ApplicationIdle);
        }

        private void Reload()
        {
            // Load Conditions
            //
            // 1) View Model is loaded
            // 2) Control is visible (virtual scroll)

            if (this.ViewModel != null && this.IsVisible)
            {
                var files = _modelController.GetArtistFiles(this.ViewModel.Id);

                // Take the first mp3 file for this artist (none of the file names will be empty)
                this.CurrentImageFile = files.FirstOrDefault(x => !string.IsNullOrEmpty(x.FileName))?.FileName;

                LoadImageAsync();
            }
            else
                this.Source = null;
        }

        private void LibraryArtistImage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.ViewModel = e.NewValue as ArtistViewModel;

            Reload();
        }
        private void LibraryArtistImage_Loaded(object sender, RoutedEventArgs e)
        {
            Reload();
        }
        private void LibraryImageControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Reload();
        }
        private void LibraryImageControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Reload();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            Reload();
        }

        private void LoadImageAsync()
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(LoadImageAsync, DispatcherPriority.Background);
            else
            {
                if (double.IsNaN(this.Width) ||
                    double.IsNaN(this.Height))
                    return;

                // CHECK LAST SETTING
                if (this.CurrentImageFile == this.LastImageFile)
                    return;

                if (!string.IsNullOrEmpty(this.CurrentImageFile))
                    this.Source = LibraryImageCache.Get(this.CurrentImageFile, (int)this.Width, (int)this.Height).FirstOrDefault();

                // Go ahead and set no source (the current image will be flushed)
                else
                    this.Source = null;

                this.LastImageFile = this.CurrentImageFile;
            }
        }
    }
}
