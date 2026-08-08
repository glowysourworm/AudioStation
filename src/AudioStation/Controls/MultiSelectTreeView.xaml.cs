using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using SimpleWpf.Extensions;

namespace AudioStation.Controls
{
    public partial class MultiSelectTreeView : UserControl
    {
        #region (public) Dependency Properties

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(MultiSelectTreeView));

        public static readonly DependencyProperty ChildItemsSourcePathProperty =
            DependencyProperty.Register("ChildItemsSourcePath", typeof(string), typeof(MultiSelectTreeView));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MultiSelectTreeView), new PropertyMetadata(OnItemsSourceChanged));

        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(MultiSelectTreeView));

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IEnumerable), typeof(MultiSelectTreeView));

        public static readonly DependencyProperty CanHaveChildrenPathProperty =
            DependencyProperty.Register("CanHaveChildrenPath", typeof(string), typeof(MultiSelectTreeView));

        public static readonly DependencyProperty HoverBrushProperty =
            DependencyProperty.Register("HoverBrush", typeof(Brush), typeof(MultiSelectTreeView));

        public static readonly DependencyProperty SelectionBrushProperty =
            DependencyProperty.Register("SelectionBrush", typeof(Brush), typeof(MultiSelectTreeView));

        public Brush HoverBrush
        {
            get { return (Brush)GetValue(HoverBrushProperty); }
            set { SetValue(HoverBrushProperty, value); }
        }
        public Brush SelectionBrush
        {
            get { return (Brush)GetValue(SelectionBrushProperty); }
            set { SetValue(SelectionBrushProperty, value); }
        }
        public string CanHaveChildrenPath
        {
            get { return (string)GetValue(CanHaveChildrenPathProperty); }
            set { SetValue(CanHaveChildrenPathProperty, value); }
        }
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }
        public string ChildItemsSourcePath
        {
            get { return (string)GetValue(ChildItemsSourcePathProperty); }
            set { SetValue(ChildItemsSourcePathProperty, value); }
        }
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }
        public IEnumerable SelectedItems
        {
            get { return (IEnumerable)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        #endregion

        bool _clicking;
        object? _mouseDownItem;
        object? _mouseUpItem;

        protected ObservableCollection<MultiSelectTreeItemViewModel> InternalItemsSource;

        public MultiSelectTreeView()
        {
            InitializeComponent();

            _clicking = false;
            _mouseDownItem = null;
            _mouseUpItem = null;

            this.InternalItemsSource = new ObservableCollection<MultiSelectTreeItemViewModel>();
            this.TheTreeView.ItemsSource = this.InternalItemsSource;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);


        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            // Calculate scroll extent
            var scrollAmount = Math.Clamp(this.TheScrollViewer.VerticalOffset - e.Delta, 0, this.TheScrollViewer.ScrollableHeight);

            // Handle scroll with the viewer
            this.TheScrollViewer.ScrollToVerticalOffset(scrollAmount);

            e.Handled = true;
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MultiSelectTreeView;
            var collection = e.NewValue as IEnumerable;
            var notifier = e.NewValue as INotifyCollectionChanged;

            // Collection source changed
            if (control != null &&
                collection != null &&
                !string.IsNullOrWhiteSpace(control.DisplayMemberPath))
            {
                MultiSelectTreeView.RecurseHookItems(control.InternalItemsSource, false);

                control.InternalItemsSource.Clear();

                MultiSelectTreeView.RecurseAddItems(control.InternalItemsSource, collection, control.DisplayMemberPath, control.ChildItemsSourcePath, control.CanHaveChildrenPath);
            }

            // Null or empty value given for collection
            else if (control != null)
            {
                control.InternalItemsSource.Clear();
            }
        }

        private static void OnItemSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {

        }

        private static void RecurseHookItems(IEnumerable collection, bool hook)
        {
            var notifier = collection as INotifyCollectionChanged;

            if (notifier != null)
            {
                if (hook)
                    notifier.CollectionChanged += OnItemSourceCollectionChanged;
                else
                    notifier.CollectionChanged -= OnItemSourceCollectionChanged;
            }

            foreach (var item in collection)
            {
                var childNotifier = item as INotifyCollectionChanged;

                if (childNotifier != null)
                {
                    if (hook)
                        childNotifier.CollectionChanged += OnItemSourceCollectionChanged;
                    else
                        childNotifier.CollectionChanged -= OnItemSourceCollectionChanged;
                }

                if (item is IEnumerable)
                    RecurseHookItems(item as IEnumerable, hook);
            }
        }

        private static void RecurseAddItems(IList destCollection,
                                            IEnumerable sourceCollection,
                                            string displayMemberPath,
                                            string childItemsSourcePath,
                                            string canHaveChildrenPath)
        {
            var notifier = sourceCollection as INotifyCollectionChanged;

            // Hook Notifier
            if (notifier != null)
            {
                notifier.CollectionChanged -= OnItemSourceCollectionChanged;
                notifier.CollectionChanged += OnItemSourceCollectionChanged;
            }

            foreach (var item in sourceCollection)
            {
                var itemValue = item.TryGetProperty(displayMemberPath);
                var itemCanHaveChildrenValue = (bool?)item.TryGetProperty(canHaveChildrenPath);

                if (itemValue != null)
                {
                    var itemViewModel = new MultiSelectTreeItemViewModel(itemValue, itemCanHaveChildrenValue ?? false);
                    var itemChildren = item.TryGetProperty(childItemsSourcePath);

                    // Add items recursively
                    if (itemChildren != null &&
                        itemChildren is IEnumerable)
                        RecurseAddItems(itemViewModel.Children as IList, itemChildren as IEnumerable, displayMemberPath, childItemsSourcePath, canHaveChildrenPath);

                    destCollection.Add(itemViewModel);
                }
            }
        }
    }
}
