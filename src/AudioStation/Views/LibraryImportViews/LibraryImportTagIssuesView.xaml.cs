using System.Windows;
using System.Windows.Controls;

using AudioStation.Core.Model.Interface;

namespace AudioStation.Views.LibraryImportViews
{
    public partial class LibraryImportTagIssuesView : UserControl
    {
        public static readonly DependencyProperty TagHeaderProperty =
            DependencyProperty.Register("TagHeader", typeof(object), typeof(LibraryImportTagIssuesView));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(LibraryImportTagIssuesView));

        public static readonly DependencyProperty UseRequiredFieldsProperty =
            DependencyProperty.Register("UseRequiredFields", typeof(bool), typeof(LibraryImportTagIssuesView));

        public static readonly DependencyProperty CopyTargetProperty =
            DependencyProperty.Register("CopyTarget", typeof(ITagSmall), typeof(LibraryImportTagIssuesView));

        public static readonly DependencyProperty CanCopyProperty =
            DependencyProperty.Register("CanCopy", typeof(bool), typeof(LibraryImportTagIssuesView));

        public object TagHeader
        {
            get { return (object)GetValue(TagHeaderProperty); }
            set { SetValue(TagHeaderProperty, value); }
        }
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public bool UseRequiredFields
        {
            get { return (bool)GetValue(UseRequiredFieldsProperty); }
            set { SetValue(UseRequiredFieldsProperty, value); }
        }

        public ITagSmall CopyTarget
        {
            get { return (ITagSmall)GetValue(CopyTargetProperty); }
            set { SetValue(CopyTargetProperty, value); }
        }

        public bool CanCopy
        {
            get { return (bool)GetValue(CanCopyProperty); }
            set { SetValue(CanCopyProperty, value); }
        }

        public LibraryImportTagIssuesView()
        {
            InitializeComponent();
        }
    }
}
