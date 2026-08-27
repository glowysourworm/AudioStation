using AudioStation.Core.Component.BitmapConverterComponent;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Model;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IFileController))]
    public class FileController : IFileController
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IModelFileService _modelFileService;

        [IocImportingConstructor]
        public FileController(IConfigurationManager configurationManager, IModelFileService modelFileService)
        {
            _configurationManager = configurationManager;
            _modelFileService = modelFileService;
        }

        public BitmapImageData GetImage(string album, string artist, FileTypes fileType, IFileController.StorageType storageType = IFileController.StorageType.DiskCache)
        {
            // Assert (configuration valid)
            var configuration = _configurationManager.GetValidConfiguration();

            throw new NotImplementedException();
        }

        public string StoreImage(BitmapImageData imageData, string album, string artist, FileTypes fileType, IFileController.StorageType storageType = IFileController.StorageType.DiskCache)
        {
            throw new NotImplementedException();
        }
    }
}
