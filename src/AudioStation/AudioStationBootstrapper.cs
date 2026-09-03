using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Model.Vendor.ATLExtension;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.EventHandler;
using AudioStation.ViewModels;
using AudioStation.ViewModels.MainViewModels;
using AudioStation.ViewModels.TagViewModels;
using AudioStation.ViewModels.Vendor.ATLViewModel;
using AudioStation.Views.DialogViews;
using AudioStation.Windows;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.Utilities;

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
        public AudioStationBootstrapper() : base(false)
        {

        }

        protected override void UserPreModuleInitialize()
        {
            // This mapper configuration must get set first; and the dialog window must be
            // called after initializing the (base) "pre-module initialize" method because
            // it tries to create the shell window - which uses the configuration.
            //
            InitializeMapperConfiguration();

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

            // Get config file from the command line (or default to config folder as current executable directory)
            var configurationFile = Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[1] : string.Empty;

            Task.Run(() =>
            {
                Application.Current
                           .Dispatcher
                           .Invoke(InitializeLibrary, DispatcherPriority.Normal, configurationFile);

            }).ContinueWith((state) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Show Main Window
                    Application.Current.MainWindow.WindowState = WindowState.Normal;

                }, DispatcherPriority.ApplicationIdle);
            });
        }

        private async void InitializeLibrary(string configurationFile)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("Initialization of the library must be on the main dispatcher thread");

            // Splash Screen (using Dialog pattern):  We're going to replicate some of our dialog code here
            //                                        to preserve the pattern. The owner of the dialog window
            //                                        can't be established until the main window is shown. So,
            //                                        this code will run it in the center of the primary window.
            //

            // NOTE***  The dispatcher thread must be available to create ViewModel instances. So, this
            //          initialization had some problems down stream when it is not initialized on the
            //          dispatcher. We'll try to use Dispatcher methods to await the initialization.
            //
            var dialogWindow = new DialogWindow();
            var dialogViewModel = new DialogSplashScreenViewModel()
            {
                Message = "(Initializing Components)",
                Progress = 0,
                ShowProgressBar = false,
                ShowProgressMessage = true
            };
            var dialogEventData = new DialogEventData(dialogViewModel);

            dialogWindow.DataContext = new SplashScreenView()
            {
                DataContext = dialogEventData.DataContext
            };

            dialogWindow.HeaderContainer.Visibility = Visibility.Collapsed;
            dialogWindow.ButtonPanel.Visibility = Visibility.Collapsed;
            dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialogWindow.Show();

            // Initialize -> IAudioStationController
            //
            // Primary controller calls initialize methods for the entire application's
            // component graph..! 
            //
            var primaryController = IocContainer.Get<IAudioStationController>();

            // Dialog Update Func (make the code here smaller)
            var dialogUpdater = new DialogEventHandlers.DialogProgressHandler((taskCount, tasksComplete, tasksError, message) =>
            {
                // Dispatcher Awareness:  The binding for the view must be on the dispatcher to show anything to the user..! So,
                //                        These callbacks have been careful to make sure there are no forwards from Task threads.
                //
                //                        However, if there are, we can always forward those to the dispatcher. The problem is
                //                        that MSFT isn't allowing the render updates to process anything while the background
                //                        tasks are sharing Dispatcher time on splices of the thread (if that's what is happening).
                //
                // Rendering:             The view model binding will not be processed until the dispatcher is forced to render.
                //                        I'm not aware of how this is required since we are async/await-ing and there is supposedly
                //                        time splicing of the thread. (I tried priority, also)
                //
                //                        The below solution worked.
                //
                //                        Should there be other Task waiters in the application there will be other methods 
                //                        discussed on how to process them; and how to update the dialog window. It would be
                //                        best to keep a base class, controller, or primary component in charge of calling
                //                        the dispatcher. So, where non-dispatcher Task instances are needed, they are part
                //                        of the interface; and should not introduce any issues with Dispatcher forwarding
                //                        or resource multi-threading errors.
                //
                // Further Analysis:      All application Task instances; and how they are sharing interface components.
                //

                if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                    throw new Exception("Initialization of the library must be on the main dispatcher thread");

                dialogViewModel.Progress = tasksComplete / (double)taskCount;
                dialogViewModel.Message = message;
                dialogViewModel.ShowProgressMessage = (message != string.Empty);
                dialogViewModel.ShowProgressBar = dialogViewModel.Progress > 0;

                // Dispatcher Render: This seems to be enough to force rendering.
                //
                Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Render);
            });

            // Audio Station Initialize:
            //
            // 1) Open Configuration
            // 2) Pass Configuration to other initializers
            //      -> IAudioStationServiceController
            //      -> IComponentViewModelLoader
            //      -> ILibraryLoaderService
            //
            // 3) Call their Initialize routines
            //

            // Configuration
            var configuration = primaryController.InitializeConfiguration(configurationFile, dialogUpdater);

            // Initialize
            primaryController.Initialize(configuration, dialogUpdater);

            // Dismiss Splash Screen
            dialogWindow.Close();
        }

        /// <summary>
        /// Initialization of the mapper configuration must occur before other components are initialized.
        /// </summary>
        private void InitializeMapperConfiguration()
        {
            // Mapper
            var mapper = IocContainer.Get<IAudioStationMapper>();

            // "Auto" Mapper: A simple recursive mapper to replace automapper
            //
            // 1) This mapper requires explicit type declaration for top level complex types; and
            //    any nested complex types.
            //
            // 2) Interface destination types are not supported. All interface source types must
            //    be declared during configuration. 
            //
            // 3) Source collections must implement IEnumerable. Destination collections must implement
            //    IList. These interfaces are inferred during recursion. 
            //
            // 4) Mapping may be permissive (skipping mismatched properties); but you must specify
            //    permissive mapping explicitly.
            //
            //
            // Add mappers for each complex type sub-mapping

            // Tag Types
            mapper.ConfigureMap<AudioStationTag, AudioStationTag>()
                  .DeclareSourceInterface<IAudioStationTag>();

            mapper.ConfigureMap<AudioStationTag, TagViewModel>()
                  .DeclareSourceInterface<IAudioStationTag>();

            mapper.ConfigureMap<TagSmall, TagSmall>()
                  .DeclareSourceInterface<ITagSmall>();

            mapper.ConfigureMap<TagSmallViewModel, TagSmallViewModel>()
                  .DeclareSourceInterface<ITagSmall>();

            mapper.ConfigureMap<TagViewModel, AudioStationTag>()
                  .DeclareSourceInterface<IAudioStationTag>();

            // Configuration
            mapper.ConfigureMap<LibraryDirectory, LibraryDirectoryViewModel>()
                  .DeclareSourceInterface<ILibraryDirectory>();

            mapper.ConfigureMap<LibraryDirectoryViewModel, LibraryDirectory>()
                  .DeclareSourceInterface<ILibraryDirectory>()
                  .IgnoreSourceProperty("OpenFolderCommand");

            mapper.ConfigureMap<AudioStationConfiguration, AudioStationConfigurationViewModel>()
                  .DeclareSourceInterface<IAudioStationConfiguration>()
                  .DeclarePropertyConverter<List<LibraryDirectory>, ObservableCollection<LibraryDirectoryViewModel>>("LibraryDirectories", (mapper, source, dest) =>
                  {
                      // Need to be able to declare destination interface -> constructor selectcion
                      dest.Clear();
                      foreach (var item in source)
                          dest.Add(mapper.Map<ILibraryDirectory, LibraryDirectoryViewModel>(item));
                  });

            mapper.ConfigureMap<AudioStationConfigurationViewModel, AudioStationConfiguration>()
                  .DeclareSourceInterface<IAudioStationConfiguration>()
                  .IgnoreSourceProperty("AddDirectoryCommand")
                  .DeclarePropertyConverter<ObservableCollection<LibraryDirectoryViewModel>, List<LibraryDirectory>>("LibraryDirectories", (mapper, source, dest) =>
                  {
                      // Need to be able to declare destination interface -> constructor selectcion
                      dest.Clear();
                      foreach (var item in source)
                          dest.Add(mapper.Map<LibraryDirectoryViewModel, LibraryDirectory>(item));
                  });
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
