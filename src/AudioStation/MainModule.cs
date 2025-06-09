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
        }

        public override void Run()
        {
            base.Run();
        }
    }
}
