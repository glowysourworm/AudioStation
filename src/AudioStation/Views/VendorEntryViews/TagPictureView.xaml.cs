using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.Controller.Model;

using SimpleWpf.IocFramework.Application.Attribute;

using TagLib;

namespace AudioStation.Views.VendorEntryViews
{
    [IocExportDefault]
    public partial class TagPictureView : UserControl
    {
        readonly IImageCacheController _imageCacheController;

        [IocImportingConstructor]
        public TagPictureView(IImageCacheController imageCacheController)
        {
            _imageCacheController = imageCacheController;

            InitializeComponent();
        }

        private void AddButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = this.DataContext as TagLib.Tag;

            if (viewModel != null)
            {
                // Extend the picture array by one
                var pictures = viewModel.Pictures;
                viewModel.Pictures = new IPicture[viewModel.Pictures.Length + 1];
                pictures.CopyTo(viewModel.Pictures, 0);

                // Get default image for the IPicture
                var defaultImage = _imageCacheController.GetDefaultImage(ImageCacheType.FullSize);

                viewModel.Pictures[viewModel.Pictures.Length - 1] = new TagLib.Picture();
            }
        }
    }
}
