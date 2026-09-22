using System.Windows;
using System.Windows.Controls;

using AudioStation.Core.Model;

namespace AudioStation.Views.LibraryImportViews
{
    public partial class LibraryImportTagSelectionView : UserControl
    {
        public static readonly DependencyProperty TagSourceProperty =
            DependencyProperty.Register("TagSource", typeof(LibraryImportSource), typeof(LibraryImportTagSelectionView));

        public LibraryImportSource TagSource
        {
            get { return (LibraryImportSource)GetValue(TagSourceProperty); }
            set { SetValue(TagSourceProperty, value); }
        }

        public LibraryImportTagSelectionView()
        {
            InitializeComponent();
        }
    }
}
