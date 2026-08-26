using AudioStation.Core.Component.BitmapConverterComponent;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Model;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IFileController))]
    public class FileController : IFileController
    {
        [IocImportingConstructor]
        public FileController()
        {

        }

        public BitmapImageData GetImage(string album, string artist, FileTypes fileType, IFileController.StorageType storageType = IFileController.StorageType.DiskCache)
        {
            throw new NotImplementedException();
        }

        public string StoreImage(BitmapImageData imageData, string album, string artist, FileTypes fileType, IFileController.StorageType storageType = IFileController.StorageType.DiskCache)
        {
            throw new NotImplementedException();
        }
    }
}
