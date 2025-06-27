using AudioStation.Controls;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Model;
using AudioStation.Views;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.IocFramework.RegionManagement.Interface;

namespace AudioStation
{
    [IocExportDefault]
    public class MainModule : ModuleBase
    {
        private const string MAIN_REGION = "MainRegion";
        private readonly IIocRegionManager _regionManager;

        [IocImportingConstructor]
        public MainModule(IIocRegionManager regionManager, IIocEventAggregator eventAggregator) : base(regionManager, eventAggregator)
        {
            _regionManager = regionManager;

            //eventAggregator.GetEvent<NowPlayingExpandedViewEvent>().Subscribe(showExpanded =>
            //{
                //if (!showExpanded)
                //    regionManager.LoadNamedInstance(MAIN_REGION, typeof(MainView));
                //else
                //    regionManager.LoadNamedInstance(MAIN_REGION, typeof(NowPlayingView));
           // });
        }

        public override void Initialize()
        {
            base.Initialize();

            _regionManager.LoadNamedInstance(MAIN_REGION, typeof(MainView));

            // ManagerView -> FileTreeView (needs loading)
            //var treeView = _regionManager.LoadNamedInstance<FileTreeView>("ManagerViewFileTreeViewRegion", true);

            // -> to set the search pattern
            //treeView.SearchPattern = "*.mp3";
        }

        public override void Run()
        {
            base.Run();

            ApplicationHelpers.Log("Welcome to Audio Station!", LogMessageType.General, LogLevel.Information);
        }
    }
}
