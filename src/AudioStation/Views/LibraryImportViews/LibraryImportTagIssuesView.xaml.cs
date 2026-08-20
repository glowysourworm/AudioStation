using System.Windows;
using System.Windows.Controls;

namespace AudioStation.Views.LibraryImportViews
{
    public partial class LibraryImportTagIssuesView : UserControl
    {
        public static readonly DependencyProperty TagHeaderProperty =
            DependencyProperty.Register("TagHeader", typeof(object), typeof(LibraryImportTagIssuesView));

        public object TagHeader
        {
            get { return (object)GetValue(TagHeaderProperty); }
            set { SetValue(TagHeaderProperty, value); }
        }

        public LibraryImportTagIssuesView()
        {
            InitializeComponent();
        }
    }
}
