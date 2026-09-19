using System.Windows;

namespace AudioStation.Views.Converter
{
    public class ConfirmationNullForegroundParameterConverter : ConfirmationBoolForegroundParameterConverterBase
    {
        public override bool IsUnset(object value)
        {
            if (value == DependencyProperty.UnsetValue)
                return true;

            return false;
        }

        public override bool IsConfirmed(object value)
        {
            if (value == DependencyProperty.UnsetValue)
                return false;

            if (value == null)
                return false;

            return true;
        }
    }
}
