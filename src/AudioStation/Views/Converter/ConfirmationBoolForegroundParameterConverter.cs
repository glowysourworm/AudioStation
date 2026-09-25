using System.Windows;

namespace AudioStation.Views.Converter
{
    public class ConfirmationBoolForegroundParameterConverter : ConfirmationBoolForegroundParameterConverterBase
    {
        public override bool IsUnset(object value)
        {
            if (value == DependencyProperty.UnsetValue)
                return true;

            if (value == null)
                return true;

            if (value is not bool)
                return true;

            return false;
        }

        public override bool IsConfirmed(object value)
        {
            if (value == DependencyProperty.UnsetValue)
                return false;

            if (value == null)
                return false;

            if (value is not bool)
                return false;

            return (bool)value;
        }
    }
}
