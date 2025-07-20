using System.Windows;

using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component.Interface;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.Views.DialogViews;
using AudioStation.Windows;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.IocFramework.EventAggregation;

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
            // This must happen first; and the dialog window must be called after initializing
            // the (base) "pre-module initialize" method because it tries to create the shell
            // window - which uses the configuration.
            //
            InitializeConfiguration();

            // Window Management:  The shell window must be defined as the main window before
            //                     opening another window (here, the dialog). So, perhaps it 
            //                     would be best to introduce a window management system to the
            //                     IOC framework. 
            //
            // This will only call initialize on the module(s). Any other pieces will wait
            // on their injector until they're called from the container. So, the main view
            // model will wait (for the configuration) until it's used by the MainWindow.
            //
            base.UserPreModuleInitialize();

            // Splash Screen (using Dialog pattern):  We're going to replicate some of our dialog code here
            //                                        to preserve the pattern. The owner of the dialog window
            //                                        can't be established until the main window is shown. So,
            //                                        this code will run it in the center of the primary window.
            //

            var dialogWindow = new DialogWindow();
            var dialogViewModel = new DialogLoadingViewModel()
            {
                Title = "Audio Station",
                ShowProgressBar = false
            };
            var dialogEventData = new DialogEventData(dialogViewModel);

            dialogWindow.DataContext = new LoadingView()
            {
                DataContext = dialogEventData.DataContext
            };

            dialogWindow.HeaderContainer.Visibility = Visibility.Collapsed;
            dialogWindow.ButtonPanel.Visibility = Visibility.Collapsed;
            dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialogWindow.Show();

            // Initialize View Model Data
            var viewModelController = IocContainer.Get<IViewModelController>();

            // (see DialogEventHandlers.cs)
            viewModelController.Initialize((taskCount, tasksComplete, tasksError, message) =>
            {
                dialogViewModel.Progress = tasksComplete / (double)taskCount;
                dialogViewModel.Message = message;
            });

            // Dismiss Splash Screen
            dialogWindow.Close();
        }

        /// <summary>
        /// Initialization of the configuration must occur before other components are initialized.
        /// </summary>
        private void InitializeConfiguration()
        {
            // We can inject our initialize procedure(s) here
            //
            var configurationManager = IocContainer.Get<IConfigurationManager>();

            // Get config file from the command line (or default to config folder as current executable directory)
            var configurationFile = Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[1] : string.Empty;

            // Read / Create Configuration
            configurationManager.Initialize(configurationFile);
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
