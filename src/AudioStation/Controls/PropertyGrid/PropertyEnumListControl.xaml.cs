using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

using SimpleWpf.Extensions;
using SimpleWpf.UI.Controls.Model;
using SimpleWpf.UI.Converter;

namespace AudioStation.Controls.PropertyGrid
{
    public partial class PropertyEnumListControl : PropertyGridControl
    {
        public static readonly DependencyProperty EnumTypeProperty =
            DependencyProperty.Register("EnumType", typeof(Type), typeof(PropertyEnumListControl), new PropertyMetadata(OnEnumChanged));

        public static readonly DependencyProperty EnumFilterProperty =
            DependencyProperty.Register("EnumFilter", typeof(Enum), typeof(PropertyEnumListControl), new PropertyMetadata(OnEnumChanged));

        public static readonly DependencyProperty EnumValueProperty =
            DependencyProperty.Register("EnumValue", typeof(Enum), typeof(PropertyEnumListControl), new PropertyMetadata(OnEnumChanged));

        public static readonly DependencyProperty ShowSetEnumValuesOnlyProperty =
            DependencyProperty.Register("ShowSetEnumValuesOnly", typeof(bool), typeof(PropertyEnumListControl));

        public static readonly DependencyProperty ShowZeroEnumProperty =
            DependencyProperty.Register("ShowZeroEnum", typeof(bool), typeof(PropertyEnumListControl));

        public Type EnumType
        {
            get { return (Type)GetValue(EnumTypeProperty); }
            set { SetValue(EnumTypeProperty, value); }
        }
        public Enum EnumFilter
        {
            get { return (Enum)GetValue(EnumFilterProperty); }
            set { SetValue(EnumFilterProperty, value); }
        }
        public Enum EnumValue
        {
            get { return (Enum)GetValue(EnumValueProperty); }
            set { SetValue(EnumValueProperty, value); }
        }
        public bool ShowSetEnumValuesOnly
        {
            get { return (bool)GetValue(ShowSetEnumValuesOnlyProperty); }
            set { SetValue(ShowSetEnumValuesOnlyProperty, value); }
        }
        public bool ShowZeroEnum
        {
            get { return (bool)GetValue(ShowZeroEnumProperty); }
            set { SetValue(ShowZeroEnumProperty, value); }
        }

        public PropertyEnumListControl()
        {
            InitializeComponent();
        }

        private static void OnEnumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PropertyEnumListControl;

            // Have to build this in code behind because we can't mix Binding / MultiBinding inside of
            // a MultiBinding
            //
            if (control != null && control.EnumValue != null && control.EnumType != null)
            {
                // Create the ObservableCollection based on the settings we have
                var converter = new EnumObservableCollectionConverter();
                var collection = converter.Convert(control.EnumValue, null, null, null) as IList;

                // Use a separate source collection (the EnumObservableCollection was setup to be readonly)
                var enumCollection = new ObservableCollection<EnumItem>();

                var typeConverter = new TypeConverter();

                // Filter the collection based on enum settings
                foreach (var item in collection.Cast<EnumItem>())
                {
                    // Flags
                    if (control.EnumType.GetAttribute<FlagsAttribute>() != null)
                    {
                        if (control.EnumFilter != null &&
                           (item.Value as Enum).HasFlag(control.EnumFilter))
                        {
                            continue;
                        }

                        // Set Values Only
                        if (control.ShowSetEnumValuesOnly &&
                           !control.EnumValue.HasFlag(item.Value as Enum))
                        {
                            continue;
                        }

                        // Remove Zero Enum
                        if (!control.ShowZeroEnum)
                        {
                            if (Enum.GetUnderlyingType(control.EnumType) == typeof(int))
                            {
                                if ((int)item.Value == 0)
                                    continue;
                            }

                            else if (Enum.GetUnderlyingType(control.EnumType) == typeof(uint))
                            {
                                if ((uint)item.Value == 0)
                                    continue;
                            }
                            else
                                throw new Exception("Unhandled enum type:  PropertyEnumListControl.cs");
                        }
                    }

                    enumCollection.Add(item);
                }

                control.EnumLB.ItemsSource = enumCollection;
            }
        }

        public override bool Validate()
        {
            return this.EnumValue != null;
        }
        public override void CommitChanges()
        {

        }
    }
}
