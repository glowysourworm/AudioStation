namespace AudioStation.ViewModels.LibraryViewModels.Comparer
{
    public class PropertyComparer<K, T> : Comparer<T> where K : IComparable
    {
        Func<T, K> _propertySelector;

        public PropertyComparer(Func<T, K> propertySelector)
        {
            _propertySelector = propertySelector;
        }

        public override int Compare(T? x, T? y)
        {
            if (x == null && y == null)
                return 0;

            else if (x == null)
                return -1;

            else if (y == null)
                return 1;

            else
                return _propertySelector(x).CompareTo(_propertySelector(y));
        }
    }
}
