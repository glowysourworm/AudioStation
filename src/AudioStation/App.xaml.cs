using System.Windows;

namespace AudioStation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        AudioStationBootstrapper _bootstrapper;

        public App()
        {
            _bootstrapper = new AudioStationBootstrapper();
        }

        private void InitializeResources()
        {
            // These aren't seen unless they're loaded by hand. This is some common WPF issue.

            var resourceUri0 = new Uri("pack://application:,,,/AudioStation;Component/Resources/ThemeColorsAndBrushes.xaml");
            var resourceUri1 = new Uri("pack://application:,,,/AudioStation;Component/Resources/ListBoxItemStyles.xaml");
            var resourceUri2 = new Uri("pack://application:,,,/AudioStation;Component/Resources/ButtonStyles.xaml");
            var resourceUri3 = new Uri("pack://application:,,,/AudioStation;Component/Resources/TextBlockStyles.xaml");
            var resourceUri4 = new Uri("pack://application:,,,/AudioStation;Component/Resources/MenuControlStyles.xaml");
            var resourceUri5 = new Uri("pack://application:,,,/AudioStation;Component/Resources/TabControlStyles.xaml");
            var resourceUri6 = new Uri("pack://application:,,,/AudioStation;Component/Resources/TextBoxStyles.xaml");
            var resourceUri7 = new Uri("pack://application:,,,/AudioStation;Component/Resources/ComboBoxStyles.xaml");
            var resourceUri8 = new Uri("pack://application:,,,/AudioStation;Component/Resources/LibraryWorkItemDataTemplates.xaml");
            var resourceUri9 = new Uri("pack://application:,,,/AudioStation;Component/Resources/LogDataTemplates.xaml");
            var resourceUri10 = new Uri("pack://application:,,,/AudioStation;Component/Resources/MainThemeStyles.xaml");

            var resourceDictionary0 = new ResourceDictionary();
            var resourceDictionary1 = new ResourceDictionary();
            var resourceDictionary2 = new ResourceDictionary();
            var resourceDictionary3 = new ResourceDictionary();
            var resourceDictionary4 = new ResourceDictionary();
            var resourceDictionary5 = new ResourceDictionary();
            var resourceDictionary6 = new ResourceDictionary();
            var resourceDictionary7 = new ResourceDictionary();
            var resourceDictionary8 = new ResourceDictionary();
            var resourceDictionary9 = new ResourceDictionary();
            var resourceDictionary10 = new ResourceDictionary();

            resourceDictionary0.Source = resourceUri0;
            resourceDictionary1.Source = resourceUri1;
            resourceDictionary2.Source = resourceUri2;
            resourceDictionary3.Source = resourceUri3;
            resourceDictionary4.Source = resourceUri4;
            resourceDictionary5.Source = resourceUri5;
            resourceDictionary6.Source = resourceUri6;
            resourceDictionary7.Source = resourceUri7;
            resourceDictionary8.Source = resourceUri8;
            resourceDictionary9.Source = resourceUri9;
            resourceDictionary10.Source = resourceUri10;

            this.Resources.MergedDictionaries.Add(resourceDictionary0);
            this.Resources.MergedDictionaries.Add(resourceDictionary1);
            this.Resources.MergedDictionaries.Add(resourceDictionary2);
            this.Resources.MergedDictionaries.Add(resourceDictionary3);
            this.Resources.MergedDictionaries.Add(resourceDictionary4);
            this.Resources.MergedDictionaries.Add(resourceDictionary5);
            this.Resources.MergedDictionaries.Add(resourceDictionary6);
            this.Resources.MergedDictionaries.Add(resourceDictionary7);
            this.Resources.MergedDictionaries.Add(resourceDictionary8);
            this.Resources.MergedDictionaries.Add(resourceDictionary9);
            this.Resources.MergedDictionaries.Add(resourceDictionary10);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeResources();

            // (Small splash window example)

            //var loadingWindow = SplashWindowFactory.CreatePopupWindow(SplashEventType.Loading);

            // Show loading window - allow primary thread to process
            //loadingWindow.Show();

            // Next, initialize the bootstrapper
            _bootstrapper.Initialize();

            // Loads configuration prior to other injectors (MainViewModel needs Configuration)

            // Run() -> Window.Show()
            _bootstrapper.Run();
        }
    }
}
