using System.Windows;
using System.Windows.Controls;

namespace AudioStation.Views.VendorEntryViews
{
    public partial class TagView : UserControl
    {
        public static readonly DependencyProperty ShowAdvancedEditorProperty =
            DependencyProperty.Register("ShowAdvancedEditor", typeof(bool), typeof(TagView));

        public static readonly DependencyProperty ShowArtworkEditorProperty =
            DependencyProperty.Register("ShowArtworkEditor", typeof(bool), typeof(TagView));

        public bool ShowAdvancedEditor
        {
            get { return (bool)GetValue(ShowAdvancedEditorProperty); }
            set { SetValue(ShowAdvancedEditorProperty, value); }
        }
        public bool ShowArtworkEditor
        {
            get { return (bool)GetValue(ShowArtworkEditorProperty); }
            set { SetValue(ShowArtworkEditorProperty, value); }
        }

        public TagView()
        {
            InitializeComponent();
        }

        private void ShowAdvancedButton_Click(object sender, RoutedEventArgs e)
        {
            this.ShowAdvancedEditor = !this.ShowAdvancedEditor;
        }

        private void ShowArtworkButton_Click(object sender, RoutedEventArgs e)
        {
            this.ShowArtworkEditor = !this.ShowArtworkEditor;
        }
    }
}
