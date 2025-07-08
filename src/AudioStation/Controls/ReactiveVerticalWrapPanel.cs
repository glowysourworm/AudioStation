using System.Windows;
using System.Windows.Controls;

using EMA.ExtendedWPFVisualTreeHelper;

namespace AudioStation.Controls
{
    public class ReactiveVerticalWrapPanel : Panel
    {
        public static readonly DependencyProperty WrapColumnSizeProperty =
            DependencyProperty.Register("WrapColumnSize", typeof(double), typeof(ReactiveVerticalWrapPanel), new PropertyMetadata(300D));

        public double WrapColumnSize
        {
            get { return (double)GetValue(WrapColumnSizeProperty); }
            set { SetValue(WrapColumnSizeProperty, value); }
        }

        protected double[] ColumnHeights { get; private set; }

        public ReactiveVerticalWrapPanel()
        {
            this.Loaded += ReactiveVerticalWrapPanel_Loaded;
        }

        private void ReactiveVerticalWrapPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // Need the render size of the items control parent
            var itemsControl = GetParentItemsControl();
            var scrollViewer = GetParentScrollViewer();

            itemsControl.SizeChanged += OnParentSizeChanged;
            scrollViewer.SizeChanged += OnParentSizeChanged;
        }

        private void OnParentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.ActualWidth == 0 ||
                this.ActualHeight == 0)
            {
                this.ColumnHeights = new double[1] { availableSize.Height };
                return base.MeasureOverride(availableSize);
            }

            // Need the render size of the items control parent; and adjusted scroll bar size.
            var itemsControl = GetParentItemsControl();
            var scrollViewer = GetParentScrollViewer();


            // Actual Width / Height are used for the panel; but these dimensions aren't constrained. So, 
            // we need the items control render size to constrain the items.
            //
            var listSize = new Size(scrollViewer.ViewportWidth > 0 ? scrollViewer.ViewportWidth : itemsControl.RenderSize.Width, 
                                    scrollViewer.ViewportHeight > 0 ? scrollViewer.ViewportHeight : itemsControl.RenderSize.Height);

            var numberColumns = (int)Math.Max(1, Math.Floor(listSize.Width / this.WrapColumnSize));
            var columnWidth = listSize.Width / numberColumns;
            var columnIndex = 0;

            // Store column dimensions for the arrange pass
            this.ColumnHeights = new double[numberColumns];

            foreach (UIElement element in this.Children)
            {
                element.Measure(listSize);

                this.ColumnHeights[columnIndex] += element.DesiredSize.Height;

                // ITEMS CONTROL SIZE CHECK
                if (this.ColumnHeights[columnIndex] > listSize.Height && 
                    columnIndex < this.ColumnHeights.Length - 1)
                    columnIndex++;
            }

            // Check for unused columns
            for (int index = this.ColumnHeights.Length - 1; index >= 0; index--)
            {
                // Remove the last column, and continue
                if (this.ColumnHeights[index] <= 0)
                {
                    var columnHeights = new double[this.ColumnHeights.Length - 1];
                    Array.Copy(this.ColumnHeights, columnHeights, columnHeights.Length);
                    this.ColumnHeights = columnHeights;
                }
            }

            // ITEMS CONTROL WIDTH + our column max height
            return new Size(listSize.Width, this.ColumnHeights.Max());
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var columnIndex = 0;
            var columnWidth = finalSize.Width / this.ColumnHeights.Length;
            var columnHeight = 0.0D;
            var finalItemHeight = this.ColumnHeights.Length * finalSize.Height;     // Constraint
            var elementRect = new Rect(0, 0, columnWidth, this.ColumnHeights[0]);

            foreach (UIElement element in this.Children)
            {
                // Set rect height for this element
                elementRect.Height = element.DesiredSize.Height;

                // Arrange the element
                element.Arrange(elementRect);

                // Update the element rect in the layout
                elementRect.Y += element.DesiredSize.Height;

                // Update the current column's height
                columnHeight += element.DesiredSize.Height;

                // Goto next column
                if (columnHeight >= this.ColumnHeights[columnIndex])
                {
                    elementRect.Y = 0;
                    elementRect.X += columnWidth;
                    columnHeight = 0;
                    columnIndex++;
                }
            }

            return base.ArrangeOverride(finalSize);
        }

        private ItemsControl GetParentItemsControl()
        {
            // Need the render size of the items control parent
            var itemsControl = WpfVisualFinders.FindParent<ItemsControl>(this);

            if (itemsControl == null)
                throw new Exception("ReactiveVerticalWrapPanel must be the panel for an items control");

            return itemsControl;
        }

        private ScrollViewer GetParentScrollViewer()
        {
            return WpfVisualFinders.FindParent<ScrollViewer>(this);
        }
    }
}
