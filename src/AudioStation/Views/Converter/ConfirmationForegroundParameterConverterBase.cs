using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AudioStation.Views.Converter
{
    public abstract class ConfirmationBoolForegroundParameterConverterBase : IValueConverter
    {
        readonly Brush _unsetBrush;
        readonly Brush _confirmedBrush;

        public ConfirmationBoolForegroundParameterConverterBase()
        {
            _unsetBrush = (Brush)App.Current.FindResource("ConfirmationNotSetBrush");
            _confirmedBrush = (Brush)App.Current.FindResource("ConfirmationConfirmedBrush");
        }

        /// <summary>
        /// Value overridden to supply the converter with a validation routine for it's inherited use.
        /// </summary>
        public abstract bool IsConfirmed(object value);

        /// <summary>
        /// Value overridden to provide an "unset value" for the converter
        /// </summary>
        public abstract bool IsUnset(object value);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // -> Unset value (?) (may not be null)
            if (IsUnset(value))
                return _unsetBrush;

            var parameterBrush = parameter as Brush;

            // -> Confirmation value
            var confirmed = IsConfirmed(value);

            if (confirmed)
            {
                if (parameterBrush != null)
                    return parameterBrush;
                else
                    return _confirmedBrush;
            }
            else
                return _unsetBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
