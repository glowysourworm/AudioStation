using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioStation.Core.Utility;

using AutoMapper;

using SimpleWpf.Extensions;

using TagLib;

namespace AudioStation.ViewModels.Vendor.TagLibViewModel
{
    /// <summary>
    /// Represents the TagLib.Properties object
    /// </summary>
    public class PropertiesViewModel : ViewModelBase, IAudioCodec, ICodec, IVideoCodec, IPhotoCodec
    {
        int _photoWidth;

        public int PhotoWidth
        {
            get { return _photoWidth; }
            set { this.RaiseAndSetIfChanged(ref _photoWidth, value); }
        }
        int _photoHeight;

        public int PhotoHeight
        {
            get { return _photoHeight; }
            set { this.RaiseAndSetIfChanged(ref _photoHeight, value); }
        }
        int _photoQuality;

        public int PhotoQuality
        {
            get { return _photoQuality; }
            set { this.RaiseAndSetIfChanged(ref _photoQuality, value); }
        }
        TimeSpan _duration;

        public TimeSpan Duration
        {
            get { return _duration; }
            set { this.RaiseAndSetIfChanged(ref _duration, value); }
        }
        MediaTypes _mediaTypes;

        public MediaTypes MediaTypes
        {
            get { return _mediaTypes; }
            set { this.RaiseAndSetIfChanged(ref _mediaTypes, value); }
        }
        string _description;

        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }
        int _videoWidth;

        public int VideoWidth
        {
            get { return _videoWidth; }
            set { this.RaiseAndSetIfChanged(ref _videoWidth, value); }
        }
        int _videoHeight;

        public int VideoHeight
        {
            get { return _videoHeight; }
            set { this.RaiseAndSetIfChanged(ref _videoHeight, value); }
        }
        int _audioBitrate;

        public int AudioBitrate
        {
            get { return _audioBitrate; }
            set { this.RaiseAndSetIfChanged(ref _audioBitrate, value); }
        }
        int _audioSampleRate;

        public int AudioSampleRate
        {
            get { return _audioSampleRate; }
            set { this.RaiseAndSetIfChanged(ref _audioSampleRate, value); }
        }
        int _audioChannels;

        public int AudioChannels
        {
            get { return _audioChannels; }
            set { this.RaiseAndSetIfChanged(ref _audioChannels, value); }
        }

        public PropertiesViewModel()
        {

        }
        public PropertiesViewModel(TagLib.Properties properties)
        {
            ApplicationHelpers.MapOnto(properties, this);
        }
    }
}
