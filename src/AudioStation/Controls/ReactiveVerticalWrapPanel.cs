using System.Windows;
using System.Windows.Controls;

using EMA.ExtendedWPFVisualTreeHelper;

using NAudio.Utils;

namespace AudioStation.Controls
{
    public class ReactiveVerticalWrapPanel : Panel
    {
        public static readonly DependencyProperty WrapColumnSizeProperty =
            DependencyProperty.Register("WrapColumnSize", typeof(double), typeof(ReactiveVerticalWrapPanel), new PropertyMetadata(300D));

        public static readonly DependencyProperty NumberOfColumnsProperty =
            DependencyProperty.Register("NumberOfColumns", typeof(int), typeof(ReactiveVerticalWrapPanel), new PropertyMetadata(1));

        public int NumberOfColumns
        {
            get { return (int)GetValue(NumberOfColumnsProperty); }
            set { SetValue(NumberOfColumnsProperty, value); }
        }

        public double WrapColumnSize
        {
            get { return (double)GetValue(WrapColumnSizeProperty); }
            set { SetValue(WrapColumnSizeProperty, value); }
        }

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

            var window = WpfVisualFinders.FindParent<Window>(this);

            window.StateChanged += Window_StateChanged;
            window.SizeChanged += Window_SizeChanged;
        }

        private void OnParentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Application.Current.Dispatcher.BeginInvoke(() =>
            //{
                InvalidateMeasure();
            //});
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Application.Current.Dispatcher.BeginInvoke(() =>
            //{
            //    InvalidateMeasure();
            //});
        }
        private void Window_StateChanged(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                InvalidateMeasure();
            });
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.ActualWidth == 0 ||
                this.ActualHeight == 0)
            {
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

            var numberColumnsMax = (int)Math.Max(1, Math.Floor(listSize.Width / this.WrapColumnSize));
            var columnWidth = listSize.Width / numberColumnsMax;
            var columnCount = 1;
            var totalHeight = 0.0D;

            foreach (UIElement element in this.Children)
            {
                element.Measure(listSize);

                totalHeight += element.DesiredSize.Height;

                // Increment number of columns desired
                if (totalHeight >= columnCount * listSize.Height)
                {
                    columnCount++;
                }
            }

            // Calculate max stack height
            var maxHeight = numberColumnsMax * listSize.Height;

            // Divide the overflow between the columns
            var overflow = totalHeight - maxHeight;

            // Set the desired column height
            var columnHeight = overflow > 0 ? listSize.Height + (overflow / numberColumnsMax) : listSize.Height;

            // Repeat measurement for the final overflow; and set the max column height based on this height.
            totalHeight = 0.0D;
            columnCount = 1;

            foreach (UIElement element in this.Children)
            {
                element.Measure(listSize);

                totalHeight += element.DesiredSize.Height;

                // Increment number of columns desired (USING NEW HEIGHT)
                if (totalHeight >= columnCount * columnHeight)
                {
                    columnCount++;
                }
            }

            // ASSERT:  column count is no more than the max + 1
            if (columnCount > numberColumnsMax + 1)
                throw new Exception("Error measuring vertical wrap panel columns:  ReactiveVerticalWrapPanel.cs");

            // Store the column count
            this.NumberOfColumns = Math.Clamp(columnCount, 1, numberColumnsMax);

            // Remeasure the overflow
            overflow = Math.Clamp(totalHeight - maxHeight, 0, columnHeight);

            // Set the measurement
            return new Size(listSize.Width, columnHeight + overflow);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            //if (this.ActualHeight == 0 ||
            //    this.ActualWidth == 0)
            //{
            //    return base.ArrangeOverride(finalSize);
            //}

            var columnWidth = finalSize.Width / this.NumberOfColumns;
            var columnHeight = 0.0D;
            var elementRect = new Rect(0, 0, columnWidth, finalSize.Height);

            foreach (UIElement element in this.Children)
            {
                // Set rect height for this element
                elementRect.Height = element.DesiredSize.Height;

                // Update the current column's height
                columnHeight += element.DesiredSize.Height;

                // Goto next column
                if (columnHeight >= finalSize.Height)
                {
                    elementRect.Y = 0;
                    elementRect.X += columnWidth;
                    columnHeight = 0;
                }

                // Arrange the element
                element.Arrange(elementRect);

                // Update the element rect in the layout
                elementRect.Y += element.DesiredSize.Height;
            }

            return finalSize;
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
