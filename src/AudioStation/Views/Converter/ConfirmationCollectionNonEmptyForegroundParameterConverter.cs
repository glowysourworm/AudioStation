using System.Collections;
using System.Windows;

namespace AudioStation.Views.Converter
{
    public class ConfirmationCollectionNonEmptyForegroundParameterConverter : ConfirmationBoolForegroundParameterConverterBase
    {
        public override bool IsUnset(object value)
        {
            if (value == DependencyProperty.UnsetValue)
                return true;

            else if (value == null)
                return true;

            return false;
        }

        public override bool IsConfirmed(object value)
        {
            if (value == DependencyProperty.UnsetValue)
                return false;

            if (value == null)
                return false;

            var collection = value as IEnumerable;

            if (collection == null)
                return false;

            // TODO: Needs extension method
            foreach (var collectionItem in collection)
                return true;

            return false;
        }
    }
}
