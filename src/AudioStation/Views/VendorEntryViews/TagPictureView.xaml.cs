using System.Windows.Controls;

using ATL;

using AudioStation.Controller.Interface;
using AudioStation.Controller.Model;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.Vendor.ATLViewModel;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

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
            var viewModel = this.DataContext as TagViewModel;

            if (viewModel != null)
            {

                try
                {
                    // Get default image for the IPicture
                    var defaultImage = _imageCacheController.GetDefaultImage(ImageCacheType.FullSize);

                    // Create copy of the image buffer from a memory stream (using WPF API)
                    var defaultImageBuffer = defaultImage.GetBuffer();

                    // Create ATL PictureInfo from the buffer
                    var pictureInfo = PictureInfo.fromBinaryData(defaultImageBuffer);

                    // Extend the picture array by one
                    viewModel.EmbeddedPictures.Add(pictureInfo);
                }
                catch (Exception ex)
                {
                    // Throwing exception to fix our default image
                    //
                    ApplicationHelpers.Log("Error creating default ATL picture:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                    throw ex;
                }
            }
        }
    }
}
