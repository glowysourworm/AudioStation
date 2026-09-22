using System.Windows;
using System.Windows.Controls;

using AudioStation.Core.Model.Interface;

namespace AudioStation.Views.TagViews
{
    public partial class SmallTagView : UserControl
    {
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(SmallTagView));

        public static readonly DependencyProperty UseRequiredFieldsProperty =
            DependencyProperty.Register("UseRequiredFields", typeof(bool), typeof(SmallTagView));

        public static readonly DependencyProperty CopyTargetProperty =
            DependencyProperty.Register("CopyTarget", typeof(ITagSmall), typeof(SmallTagView));

        public static readonly DependencyProperty CanCopyProperty =
            DependencyProperty.Register("CanCopy", typeof(bool), typeof(SmallTagView));

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

        public SmallTagView()
        {
            InitializeComponent();
        }

        private void AlbumArtistCtrl_CopyEvent(string newValue)
        {
            if (this.CopyTarget != null)
                this.CopyTarget.AlbumArtist = newValue;
        }

        private void AlbumCtrl_CopyEvent(string newValue)
        {
            if (this.CopyTarget != null)
                this.CopyTarget.Album = newValue;
        }

        private void TitleCtrl_CopyEvent(string newValue)
        {
            if (this.CopyTarget != null)
                this.CopyTarget.Title = newValue;
        }

        private void GenreCtrl_CopyEvent(string newValue)
        {
            if (this.CopyTarget != null)
                this.CopyTarget.Genre = newValue;
        }

        private void TrackNumberCtrl_CopyEvent(int newValue)
        {
            if (this.CopyTarget != null)
                this.CopyTarget.TrackNumber = newValue;
        }

        private void TrackTotalCtrl_CopyEvent(int newValue)
        {
            if (this.CopyTarget != null)
                this.CopyTarget.TrackTotal = newValue;
        }

        private void MediaNumberCtrl_CopyEvent(int newValue)
        {
            if (this.CopyTarget != null)
                this.CopyTarget.MediaNumber = newValue;
        }

        private void MediaTotalCtrl_CopyEvent(int newValue)
        {
            if (this.CopyTarget != null)
                this.CopyTarget.MediaTotal = newValue;
        }

        private void DurationMillisecondsCtrl_CopyEvent(int newValue)
        {
            if (this.CopyTarget != null)
                this.CopyTarget.DurationMilliseconds = newValue;
        }

        private void MediaFormatCtrl_CopyEvent(string newValue)
        {
            if (this.CopyTarget != null)
                this.CopyTarget.MediaFormat = newValue;
        }
    }
}
