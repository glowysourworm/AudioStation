using System.Configuration;
using System.Data;
using System.Windows;

namespace AudioStation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // These aren't seen unless they're loaded by hand. This is some common WPF issue.

            var resourceUri = new Uri("pack://application:,,,/AudioStation;Component/Resources/ListBoxItemStyles.xaml");

            var resourceDictionary = new ResourceDictionary();

            resourceDictionary.Source = resourceUri;

            this.Resources.MergedDictionaries.Add(resourceDictionary);
        }
    }

}
