﻿using SimpleWpf.IocFramework.Application;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.IocFramework.RegionManagement.Interface;

namespace AudioStation
{
    [IocExportDefault]
    public class MainModule : ModuleBase
    {
        [IocImportingConstructor]
        public MainModule(IIocRegionManager regionManager, IIocEventAggregator eventAggregator) : base(regionManager, eventAggregator)
        {
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
