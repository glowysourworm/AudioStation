using System.Windows;

namespace AudioStation.Views.Converter
{
    public class ValidationNullForegroundParameterConverter : ValidationForegroundParameterConverterBase
    {
        public override bool IsUnset(object value)
        {
            if (value == DependencyProperty.UnsetValue)
                return true;

            if (value is not bool)
                return true;

            return false;
        }

        public override bool IsValid(object value)
        {
            if (value == DependencyProperty.UnsetValue)
                return false;

            if (value == null)
                return false;

            if (value is not bool)
                return false;

            return true;
        }
    }
}
