using AudioStation.Controls;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.IocFramework.RegionManagement.Interface;

namespace AudioStation
{
    [IocExportDefault]
    public class MainModule : ModuleBase
    {
        private readonly IIocRegionManager _regionManager;

        [IocImportingConstructor]
        public MainModule(IIocRegionManager regionManager, IIocEventAggregator eventAggregator) : base(regionManager, eventAggregator)
        {
            _regionManager = regionManager;
        }

        public override void Initialize()
        {
            base.Initialize();

            // ManagerView -> FileTreeView (needs loading)
            //var treeView = _regionManager.LoadNamedInstance<FileTreeView>("ManagerViewFileTreeViewRegion", true);

            // -> to set the search pattern
            //treeView.SearchPattern = "*.mp3";
        }

        public override void Run()
        {
            base.Run();
        }
    }
}
