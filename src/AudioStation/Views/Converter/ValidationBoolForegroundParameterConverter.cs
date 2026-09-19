using System.Windows;

namespace AudioStation.Views.Converter
{
    public class ValidationBoolForegroundParameterConverter : ValidationForegroundParameterConverterBase
    {
        public override bool IsUnset(object value)
        {
            if (value == DependencyProperty.UnsetValue)
                return true;

            if (value == null)
                return true;

            if (value is not bool)
                return true;

            return (bool)value;
        }

        public override bool IsValid(object value)
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
