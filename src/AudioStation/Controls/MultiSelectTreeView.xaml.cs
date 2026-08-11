using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.ObservableCollection;

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

        public static readonly DependencyProperty CanHaveChildrenPathProperty =
            DependencyProperty.Register("CanHaveChildrenPath", typeof(string), typeof(MultiSelectTreeView));

        public static readonly DependencyProperty HoverBrushProperty =
            DependencyProperty.Register("HoverBrush", typeof(Brush), typeof(MultiSelectTreeView));

        public static readonly DependencyProperty SelectionBrushProperty =
            DependencyProperty.Register("SelectionBrush", typeof(Brush), typeof(MultiSelectTreeView));

        public static readonly DependencyProperty IsSelectedPathProperty =
            DependencyProperty.Register("IsSelectedPath", typeof(string), typeof(MultiSelectTreeView));

        public string IsSelectedPath
        {
            get { return (string)GetValue(IsSelectedPathProperty); }
            set { SetValue(IsSelectedPathProperty, value); }
        }
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

        #endregion

        protected NotifyingObservableCollection<MultiSelectTreeItemViewModel> InternalItemsSource;

        public MultiSelectTreeView()
        {
            InitializeComponent();

            this.InternalItemsSource = new NotifyingObservableCollection<MultiSelectTreeItemViewModel>();
            this.TheTreeView.ItemsSource = this.InternalItemsSource;
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            // Calculate scroll extent
            var scrollAmount = Math.Clamp(this.TheScrollViewer.VerticalOffset - e.Delta, 0, this.TheScrollViewer.ScrollableHeight);

            // Handle scroll with the viewer
            this.TheScrollViewer.ScrollToVerticalOffset(scrollAmount);

            e.Handled = true;
        }

        private void RefreshTree()
        {
            // Bound Items Source (Internal is the items for the UI)
            var collection = this.ItemsSource as IEnumerable;

            if (collection != null &&
                !string.IsNullOrWhiteSpace(this.DisplayMemberPath))
            {
                // Unhook
                this.RecurseHookItems(this.InternalItemsSource, false);

                // Clear
                this.InternalItemsSource.Clear();

                // Re-populate
                this.RecurseAddItems(null, this.InternalItemsSource, collection);

                // Hook
                this.RecurseHookItems(this.InternalItemsSource, true);
            }

            // Null or empty value given for collection
            else
            {
                // Unhook
                this.RecurseHookItems(this.InternalItemsSource, false);

                // Clear
                this.InternalItemsSource.Clear();
            }

        }

        private void OnItemSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshTree();
        }

        // Occurs when a property on the UI (target) side changes
        private void OnItemSourceItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var viewModel = sender as MultiSelectTreeItemViewModel;

            // "Bound" Properties
            if (viewModel != null &&
                viewModel.Item != null &&
                !string.IsNullOrWhiteSpace(this.IsSelectedPath))
            {
                // Destination Property Setter
                viewModel.Item.SetProperty(this.IsSelectedPath, viewModel.IsSelected);

                // Selection:  De-select anything not in this item's collection (if it is selected)
                //
                if (viewModel.IsSelected)
                {
                    RecursiveIterate(this.InternalItemsSource, item =>
                    {
                        if (item.Parent != viewModel.Parent)
                            item.IsSelected = false;
                    });
                }
            }
        }

        private void RecurseHookItems(IEnumerable<MultiSelectTreeItemViewModel> collection, bool hook)
        {
            // Procedure:  Recursively iterate the control's item collection and use the "Item" property
            //             on the control's item to access the user's item.
            //

            var notifier = collection as INotifyCollectionChanged;

            // Child Collection Properties
            if (notifier != null)
            {
                if (hook)
                    notifier.CollectionChanged += OnItemSourceCollectionChanged;
                else
                    notifier.CollectionChanged -= OnItemSourceCollectionChanged;
            }

            foreach (var item in collection)
            {
                var propertyNotifier = item as INotifyPropertyChanged;

                // Item Properties
                if (propertyNotifier != null)
                {
                    if (hook)
                        propertyNotifier.PropertyChanged += OnItemSourceItemPropertyChanged;
                    else
                        propertyNotifier.PropertyChanged -= OnItemSourceItemPropertyChanged;
                }

                if (item != null &&
                    item.Children is IEnumerable<MultiSelectTreeItemViewModel> &&
                    item.CanHaveChildren)
                    RecurseHookItems(item.Children as IEnumerable<MultiSelectTreeItemViewModel>, hook);

            }
        }

        private void RecurseAddItems(MultiSelectTreeItemViewModel destParent,
                                     IList<MultiSelectTreeItemViewModel> destCollection,
                                     IEnumerable sourceCollection)
        {
            foreach (var item in sourceCollection)
            {
                var itemValue = item;
                var itemDisplayValue = item.TryGetProperty(this.DisplayMemberPath);
                var itemCanHaveChildrenValue = (bool?)item.TryGetProperty(this.CanHaveChildrenPath);

                if (itemValue != null)
                {
                    var itemViewModel = new MultiSelectTreeItemViewModel(destParent, itemValue, itemDisplayValue, itemCanHaveChildrenValue ?? false);
                    var itemChildren = item.TryGetProperty(this.ChildItemsSourcePath);

                    // Add items recursively
                    if (itemChildren != null &&
                        itemChildren is IEnumerable &&
                        itemViewModel.Children != null)
                        RecurseAddItems(itemViewModel,
                                        itemViewModel.Children as IList<MultiSelectTreeItemViewModel>,
                                        itemChildren as IEnumerable);

                    destCollection.Add(itemViewModel);
                }
            }
        }

        private void RecursiveIterate(IEnumerable<MultiSelectTreeItemViewModel> collection, Action<MultiSelectTreeItemViewModel> action)
        {
            foreach (var item in collection)
            {
                action(item);

                if (item.CanHaveChildren)
                    RecursiveIterate(item.Children as IEnumerable<MultiSelectTreeItemViewModel>, action);
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MultiSelectTreeView;

            if (control != null)
                control.RefreshTree();
        }
    }
}
