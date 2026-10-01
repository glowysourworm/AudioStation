using System.Windows;

using Microsoft.Xaml.Behaviors;

namespace AudioStation.Views.Behavior
{
    public abstract class ResponsiveBehaviorBase<T> : Behavior<T> where T : DependencyObject
    {
        ResponsiveSize? _responsiveSize;

        public ResponsiveBehaviorBase()
        {
            Application.Current.MainWindow.SizeChanged += OnMainWindowSizeChanged;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (UpdateResponsiveSize())
                OnResponsiveSizeChanged(_responsiveSize.Value);
        }

        private void OnMainWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (UpdateResponsiveSize())
                OnResponsiveSizeChanged(_responsiveSize.Value);
        }

        private bool UpdateResponsiveSize()
        {
            var responsiveSize = ResponsiveUtility.GetWindowSize();
            var changed = responsiveSize != _responsiveSize;

            _responsiveSize = responsiveSize;

            return changed;
        }

        protected abstract void OnResponsiveSizeChanged(ResponsiveSize responsiveSize);

        //public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(object), typeof(UpdateTemplateBehavior), new FrameworkPropertyMetadata(null, OnContentChanged));
        //public object Content
        //{
        //    get => GetValue(ContentProperty);
        //    set => SetValue(ContentProperty, value);
        //}
        //static void OnContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    if (sender is UpdateTemplateBehavior behavior)
        //        behavior.Update();
        //}

        //public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(UpdateTemplateBehavior), new FrameworkPropertyMetadata(null, OnValueChanged));
        //public object Value
        //{
        //    get => GetValue(ValueProperty);
        //    set => SetValue(ValueProperty, value);
        //}
        //static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    if (sender is UpdateTemplateBehavior behavior)
        //        behavior.Update();
        //}

        //public UpdateTemplateBehavior() : base() { }

        //protected override void OnAttached()
        //{
        //    base.OnAttached();
        //    Update();
        //}

        //void Update()
        //{
        //    if (Content != null)
        //    {
        //        BindingOperations.ClearBinding(AssociatedObject, ContentPresenter.ContentProperty);
        //        AssociatedObject.Content = null;

        //        BindingOperations.SetBinding(AssociatedObject, ContentPresenter.ContentProperty, new Binding() { Path = nameof(Content), Source = this });
        //    }
        //}
    }
}
