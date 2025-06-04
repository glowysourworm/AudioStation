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

            var resourceUri0 = new Uri("pack://application:,,,/AudioStation;Component/Resources/ThemeColorsAndBrushes.xaml");
            var resourceUri1 = new Uri("pack://application:,,,/AudioStation;Component/Resources/ListBoxItemStyles.xaml");
            var resourceUri2 = new Uri("pack://application:,,,/AudioStation;Component/Resources/ButtonStyles.xaml");
            var resourceUri3 = new Uri("pack://application:,,,/AudioStation;Component/Resources/TextBlockStyles.xaml");

            var resourceDictionary0 = new ResourceDictionary();
            var resourceDictionary1 = new ResourceDictionary();
            var resourceDictionary2 = new ResourceDictionary();
            var resourceDictionary3 = new ResourceDictionary();

            resourceDictionary0.Source = resourceUri0;
            resourceDictionary1.Source = resourceUri1;
            resourceDictionary2.Source = resourceUri2;
            resourceDictionary3.Source = resourceUri3;

            this.Resources.MergedDictionaries.Add(resourceDictionary0);
            this.Resources.MergedDictionaries.Add(resourceDictionary1);
            this.Resources.MergedDictionaries.Add(resourceDictionary2);
            this.Resources.MergedDictionaries.Add(resourceDictionary3);
        }
    }

}
