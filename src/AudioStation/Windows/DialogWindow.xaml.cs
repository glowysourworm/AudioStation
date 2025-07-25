using System.Windows;

using AudioStation.Controls.PropertyGrid;

using EMA.ExtendedWPFVisualTreeHelper;

namespace AudioStation.Windows
{
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate / Accept
            //
            // 1) Search tree for any property grid controls
            // 2) Run Validation to get feedback for the user
            // 3) Run Accept to commit changes to the data sources
            // 4) Set Dialog Result
            //

            // Find PropertyGridControl(s):  Separate case for lazy-loading controls (TabControl)
            //
            //var propertyGridTabControls = WpfVisualFinders.FindAllChildren<PropertyGridTabControl>(this);
            var propertyGridControls = WpfVisualFinders.FindAllChildren<PropertyGridControl>(this);

            // Validation (this may leave things for the user to do "IsValid") (red highlights)
            //
            var valid = !propertyGridControls.Any() || propertyGridControls.All(x => x.Validate());

            // -> TabControl(s)
            //valid &= !propertyGridTabControls.Any() || propertyGridTabControls.All(x => x.Validate()); 

            // Commit (some controls will cause an updating of the binding source)
            //
            //if (valid)
            //{
                // We may need a try/catch here to see what's going on; but most of these are very simple
                //
                foreach (var control in propertyGridControls)
                {
                    // There could also be a boolean result, if there are problems with the binding. Validation
                    // should cover issues with the data, however.
                    control.CommitChanges();
                }

                // Success!
                //this.DialogResult = true;
            //}

            // DialogResult:  There could be settings for accepting only partial data.. Mostly, there'd be a cancel
            //                if the data is invalid.
            this.DialogResult = true;
        }
    }
}
