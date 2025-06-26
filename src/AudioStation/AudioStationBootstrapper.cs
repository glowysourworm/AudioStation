using AudioStation.Core;
using AudioStation.Core.Component.Interface;

using SimpleWpf.IocFramework.Application;

namespace AudioStation
{
    /// <summary>
    /// IOC Bootstrapper:  Takes over primary control / startup of the application. The configuration is
    ///                    read here; and the components are initialized. Most / all major components will
    ///                    inherit from an interface; and have Initialize / Dispose methods. These are 
    ///                    handled during the UserPreModuleInitialize sequence - after the configuration is
    ///                    read. This configuration will also be injected into the primary view model. Changes
    ///                    to the primary view model / configuration may be handled there; and disposing of
    ///                    the main components will also be handled by our IDisposable pattern.
    /// </summary>
    public class AudioStationBootstrapper : IocWindowBootstrapper
    {
        public AudioStationBootstrapper()
        {

        }

        protected override void UserPreModuleInitialize()
        {
            // We can inject our initialize procedure(s) here
            //
            var configurationManager = IocContainer.Get<IConfigurationManager>();
            var modelController = IocContainer.Get<IModelController>();
            var outputController = IocContainer.Get<IOutputController>();

            // Get config file from the command line (or default to config folder as current executable directory)
            var configurationFile = Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[1] : string.Empty;

            // Read / Create Configuration
            configurationManager.Initialize(configurationFile);

            // Apply configuration
            //modelController.Initialize();

            // This will only call initialize on the module(s). Any other pieces will wait
            // on their injector until they're called from the container. So, the main view
            // model will wait (for the configuration) until it's used by the MainWindow.
            //
            base.UserPreModuleInitialize();
        }

        public override IEnumerable<ModuleDefinition> DefineModules()
        {
            return new ModuleDefinition[]
            {
                new ModuleDefinition("MainModule", typeof(MainModule), true),
                new ModuleDefinition("CoreModule", typeof(CoreModule), false)
            };
        }

        public override Type DefineShell()
        {
            return typeof(MainWindow);
        }
    }
}
