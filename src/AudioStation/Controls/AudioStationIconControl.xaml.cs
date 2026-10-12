using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using FontAwesome5;

namespace AudioStation.Controls
{
    public partial class AudioStationIconControl : UserControl
    {
        public static readonly DependencyProperty IconTypeProperty =
            DependencyProperty.Register("IconType", typeof(AudioStationIconTypes), typeof(AudioStationIconControl), new PropertyMetadata(OnTypeChanged));

        public static readonly DependencyProperty IconStateProperty =
            DependencyProperty.Register("IconState", typeof(AudioStationIconState), typeof(AudioStationIconControl), new PropertyMetadata(OnStateChanged));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(AudioStationIconControl));

        public static readonly DependencyProperty ActivityBadgeCountProperty =
            DependencyProperty.Register("ActivityBadgeCount", typeof(int), typeof(AudioStationIconControl));

        public AudioStationIconTypes IconType
        {
            get { return (AudioStationIconTypes)GetValue(IconTypeProperty); }
            set { SetValue(IconTypeProperty, value); }
        }
        public AudioStationIconState IconState
        {
            get { return (AudioStationIconState)GetValue(IconStateProperty); }
            set { SetValue(IconStateProperty, value); }
        }
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public int ActivityBadgeCount
        {
            get { return (int)GetValue(ActivityBadgeCountProperty); }
            set { SetValue(ActivityBadgeCountProperty, value); }
        }

        private const string IMAGE_RESOURCE_PATH = "AudioStation.Resources.Images.";

        private ImageSource _disabledImage;
        private ImageSource _enabledImage;
        private ImageSource _activeImage;
        private ImageSource _errorImage;

        private int _activityBadgeCount;

        // Font Awesome Brushes
        private readonly Brush _foregroundDisabled;
        private readonly Brush _foregroundEnabled;
        private readonly Brush _foregroundActive;
        private readonly Brush _foregroundError;

        public AudioStationIconControl()
        {
            InitializeComponent();

            _foregroundEnabled = (Brush)this.FindResource("IconEnabledBrush");
            _foregroundDisabled = (Brush)this.FindResource("IconDisabledBrush");
            _foregroundActive = (Brush)this.FindResource("IconActiveBrush");
            _foregroundError = (Brush)this.FindResource("IconErrorBrush");

            this.ActivityBadgeCount = 0;
            this.IsEnabledChanged += AudioStationIconControl_IsEnabledChanged;

            this.DataContextChanged += AudioStationIconControl_DataContextChanged;
        }

        private void AudioStationIconControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Update(true);
        }

        private void AudioStationIconControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateState();
        }

        private void Update(bool iconChange)
        {
            if (iconChange)
                LoadImageSources(this.IconType);

            UpdateState();
        }

        private void UpdateState()
        {
            switch (this.IconState)
            {
                case AudioStationIconState.Enabled:
                    this.IsEnabled = true;
                    this.Source = _enabledImage;
                    break;
                case AudioStationIconState.Disabled:
                    this.IsEnabled = false;
                    this.Source = _disabledImage;
                    break;
                case AudioStationIconState.Active:
                    this.IsEnabled = true;
                    this.Source = _activeImage;
                    break;
                case AudioStationIconState.Error:
                    this.IsEnabled = true;
                    this.Source = _errorImage;
                    break;
                default:
                    throw new Exception("Unhandled Icon State");
            }
        }

        protected override void OnIsMouseDirectlyOverChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsMouseDirectlyOverChanged(e);

            UpdateState();
        }
        private void LoadImageSources(AudioStationIconTypes iconType)
        {
            switch (this.IconType)
            {
                case AudioStationIconTypes.AudioStation:
                    _activeImage = GetImageSource("icon-active.png");
                    _enabledImage = GetImageSource("icon-enabled.png");
                    _disabledImage = GetImageSource("icon-disabled.png");
                    _errorImage = GetImageSource("icon-error.png");
                    break;
                case AudioStationIconTypes.Database:
                    _activeImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Database, AudioStationIconState.Active);
                    _enabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Database, AudioStationIconState.Enabled);
                    _disabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Database, AudioStationIconState.Disabled);
                    _errorImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Database, AudioStationIconState.Error);
                    break;
                case AudioStationIconTypes.File:
                    _activeImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Regular_File, AudioStationIconState.Active);
                    _enabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Regular_File, AudioStationIconState.Enabled);
                    _disabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Regular_File, AudioStationIconState.Disabled);
                    _errorImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Regular_File, AudioStationIconState.Error);
                    break;
                case AudioStationIconTypes.FolderClosed:
                    _activeImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Regular_Folder, AudioStationIconState.Active);
                    _enabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Regular_Folder, AudioStationIconState.Enabled);
                    _disabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Regular_Folder, AudioStationIconState.Disabled);
                    _errorImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Regular_Folder, AudioStationIconState.Error);
                    break;
                case AudioStationIconTypes.FolderOpen:
                    _activeImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Regular_FolderOpen, AudioStationIconState.Active);
                    _enabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Regular_FolderOpen, AudioStationIconState.Enabled);
                    _disabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Regular_FolderOpen, AudioStationIconState.Disabled);
                    _errorImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Regular_FolderOpen, AudioStationIconState.Error);
                    break;
                case AudioStationIconTypes.Disc:
                    _activeImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_CompactDisc, AudioStationIconState.Active);
                    _enabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_CompactDisc, AudioStationIconState.Enabled);
                    _disabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_CompactDisc, AudioStationIconState.Disabled);
                    _errorImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_CompactDisc, AudioStationIconState.Error);
                    break;
                case AudioStationIconTypes.Play:
                    _activeImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Play, AudioStationIconState.Active);
                    _enabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Play, AudioStationIconState.Enabled);
                    _disabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Play, AudioStationIconState.Disabled);
                    _errorImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Play, AudioStationIconState.Error);
                    break;
                case AudioStationIconTypes.Stop:
                    _activeImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Stop, AudioStationIconState.Active);
                    _enabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Stop, AudioStationIconState.Enabled);
                    _disabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Stop, AudioStationIconState.Disabled);
                    _errorImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Stop, AudioStationIconState.Error);
                    break;
                case AudioStationIconTypes.Pause:
                    _activeImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Pause, AudioStationIconState.Active);
                    _enabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Pause, AudioStationIconState.Enabled);
                    _disabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Pause, AudioStationIconState.Disabled);
                    _errorImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Pause, AudioStationIconState.Error);
                    break;
                case AudioStationIconTypes.AudioStationLog:
                    _activeImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Database, AudioStationIconState.Active);
                    _enabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Database, AudioStationIconState.Enabled);
                    _disabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Database, AudioStationIconState.Disabled);
                    _errorImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Solid_Database, AudioStationIconState.Error);
                    break;
                case AudioStationIconTypes.AudioStationMissingArtwork:
                    _activeImage = GetImageSource("placeholder_Thumbnail.png");
                    _enabledImage = GetImageSource("placeholder_Thumbnail.png");
                    _disabledImage = GetImageSource("placeholder_Thumbnail.png");
                    _errorImage = GetImageSource("placeholder_Thumbnail.png");
                    break;
                case AudioStationIconTypes.AudioDB:
                    _activeImage = GetImageSource("audioDB-active.png");
                    _enabledImage = GetImageSource("audioDB-enabled.png");
                    _disabledImage = GetImageSource("audioDB-disabled.png");
                    _errorImage = GetImageSource("audioDB-error.png");
                    break;
                case AudioStationIconTypes.Discogs:
                    _activeImage = GetImageSource("discogs-active.png");
                    _enabledImage = GetImageSource("discogs-enabled.png");
                    _disabledImage = GetImageSource("discogs-disabled.png");
                    _errorImage = GetImageSource("discogs-error.png");
                    break;
                case AudioStationIconTypes.iTunes:
                    _activeImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_Itunes, AudioStationIconState.Active);
                    _enabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_Itunes, AudioStationIconState.Enabled);
                    _disabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_Itunes, AudioStationIconState.Disabled);
                    _errorImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_Itunes, AudioStationIconState.Error);
                    break;
                case AudioStationIconTypes.LastFm:
                    _activeImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_Lastfm, AudioStationIconState.Active);
                    _enabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_Lastfm, AudioStationIconState.Enabled);
                    _disabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_Lastfm, AudioStationIconState.Disabled);
                    _errorImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_Lastfm, AudioStationIconState.Error);
                    break;
                case AudioStationIconTypes.Spotify:
                    _activeImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_Spotify, AudioStationIconState.Active);
                    _enabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_Spotify, AudioStationIconState.Enabled);
                    _disabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_Spotify, AudioStationIconState.Disabled);
                    _errorImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_Spotify, AudioStationIconState.Error);
                    break;
                case AudioStationIconTypes.Wikipedia:
                    _activeImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_WikipediaW, AudioStationIconState.Active);
                    _enabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_WikipediaW, AudioStationIconState.Enabled);
                    _disabledImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_WikipediaW, AudioStationIconState.Disabled);
                    _errorImage = GetFontAwesomeImageSource(EFontAwesomeIcon.Brands_WikipediaW, AudioStationIconState.Error);
                    break;
                case AudioStationIconTypes.MusicBrainz:
                    _activeImage = GetImageSource("music-brainz-active.png");
                    _enabledImage = GetImageSource("music-brainz-enabled.png");
                    _disabledImage = GetImageSource("music-brainz-disabled.png");
                    _errorImage = GetImageSource("music-brainz-error.png");
                    break;
                case AudioStationIconTypes.AcoustID:
                    _activeImage = GetImageSource("acoustID-active.png");
                    _enabledImage = GetImageSource("acoustID-enabled.png");
                    _disabledImage = GetImageSource("acoustID-disabled.png");
                    _errorImage = GetImageSource("acoustID-error.png");
                    break;
                case AudioStationIconTypes.Fanart:
                    _activeImage = GetImageSource("fanart-active.png");
                    _enabledImage = GetImageSource("fanart-enabled.png");
                    _disabledImage = GetImageSource("fanart-disabled.png");
                    _errorImage = GetImageSource("fanart-error.png");
                    break;
                case AudioStationIconTypes.RadioBrowser:
                    _activeImage = GetImageSource("radio-active.png");
                    _enabledImage = GetImageSource("radio-enabled.png");
                    _disabledImage = GetImageSource("radio-disabled.png");
                    _errorImage = GetImageSource("radio-error.png");
                    break;
                default:
                    throw new Exception("Unhandled Icon Type");
            }
        }

        private ImageSource GetImageSource(string fileName)
        {
            using (var imageStream = Assembly.GetAssembly(typeof(AudioStationBootstrapper))
                                             .GetManifestResourceStream(IMAGE_RESOURCE_PATH + fileName))
            {
                var decoder = BmpBitmapDecoder.Create(imageStream, BitmapCreateOptions.None, BitmapCacheOption.Default);

                if (decoder.Frames.Count > 0)
                    return decoder.Frames[0];
            }

            return null;
        }

        private ImageSource GetFontAwesomeImageSource(EFontAwesomeIcon icon, AudioStationIconState state)
        {
            Brush foreground = null;

            switch (state)
            {
                case AudioStationIconState.Enabled:
                    foreground = _foregroundEnabled;
                    break;
                case AudioStationIconState.Active:
                    foreground = _foregroundActive;
                    break;
                case AudioStationIconState.Error:
                    foreground = _foregroundError;
                    break;
                case AudioStationIconState.Disabled:
                default:
                    foreground = _foregroundDisabled;
                    break;
            }

            var imageAwesome = new ImageAwesome()
            {
                Icon = icon,
                Foreground = foreground,
                Width = this.Width,
                Height = this.Height
            };

            return imageAwesome.Source;
        }

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as AudioStationIconControl;

            if (control != null)
            {
                control.Update(false);
            }
        }
        private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as AudioStationIconControl;

            if (control != null)
            {
                control.Update(true);
            }
        }
    }
}
