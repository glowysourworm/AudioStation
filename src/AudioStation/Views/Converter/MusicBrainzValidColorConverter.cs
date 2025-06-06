﻿using System.Drawing;
using System.Globalization;
using System.Windows.Data;

using AudioStation.ViewModels;

namespace AudioStation.Views.Converter
{
    public class MusicBrainzValidColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Brushes.Gray;

            return (bool)value ? Brushes.Green : Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
