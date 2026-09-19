using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AudioStation.Views.Converter
{
    /// <summary>
    /// Applies a red brush when value is invalid
    /// </summary>
    public abstract class ValidationForegroundParameterConverterBase : IValueConverter
    {
        readonly Brush _unsetBrush;
        readonly Brush _validBrush;
        readonly Brush _invalidBrush;

        public ValidationForegroundParameterConverterBase()
        {
            _unsetBrush = (Brush)App.Current.FindResource("ValidationNotSetBrush");
            _validBrush = (Brush)App.Current.FindResource("ValidationValidBrush");
            _invalidBrush = (Brush)App.Current.FindResource("ValidationInvalidBrush");
        }

        /// <summary>
        /// Value overridden to supply the converter with a validation routine for it's inherited use.
        /// </summary>
        public abstract bool IsValid(object value);

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

            // -> Validate value
            var valid = IsValid(value);

            if (valid)
            {
                if (parameterBrush != null)
                    return parameterBrush;
                else
                    return _validBrush;
            }
            else
                return _invalidBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
