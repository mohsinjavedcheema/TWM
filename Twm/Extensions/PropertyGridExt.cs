using System.ComponentModel;
using System.Linq;
using System.Windows;
using Twm.Core.Attributes;
using Twm.Core.Enums;
using Microsoft.Xaml.Behaviors;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Twm.Extensions
{
    public class PropertyGridExt
    {
        public static readonly DependencyProperty PropsVisibilityProperty = DependencyProperty.RegisterAttached(
            "PropsVisibility", typeof(PropertyVisibility), typeof(PropertyGridExt), new FrameworkPropertyMetadata(PropertyVisibility.NotSetted, FrameworkPropertyMetadataOptions.AffectsRender));

        public static void SetPropsVisibility(DependencyObject element, PropertyVisibility value)
        {
            element.SetValue(PropsVisibilityProperty, value);
        }

        public static PropertyVisibility GetPropsVisibility(DependencyObject element)
        {
            return (PropertyVisibility)element.GetValue(PropsVisibilityProperty);
        }
    }
    
    
    public class PropsVisibilityBehavior : Behavior<PropertyGrid>
    {
        protected override void OnAttached()
        {
            AssociatedObject.IsPropertyBrowsable += AssociatedObjectOnIsPropertyBrowsable;
        }

        private void AssociatedObjectOnIsPropertyBrowsable(object sender, IsPropertyBrowsableArgs e)
        {

            var pro = (PropertyVisibility)AssociatedObject.GetValue(PropertyGridExt.PropsVisibilityProperty);
            var visibility = (VisibleAttribute) e.PropertyDescriptor.Attributes[typeof(VisibleAttribute)];
            var browsable = (BrowsableAttribute) e.PropertyDescriptor.Attributes[typeof(BrowsableAttribute)];
            
            if (visibility == null)
            {
                e.IsBrowsable = DefaultBrowserable(browsable);
                return;
            }

            if (pro == PropertyVisibility.NotSetted)
            {
                e.IsBrowsable = DefaultBrowserable(browsable);
                return;
            }
            e.IsBrowsable = visibility.Visibilities().Contains(PropertyVisibility.Everywhere) || visibility.Visibilities().Contains(pro) ? visibility.Visible() : !visibility.Visible();
        }

        private bool DefaultBrowserable(BrowsableAttribute browsable)
        {
            if (browsable != null)
            {
                return browsable.Browsable;
            }

            return true;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.IsPropertyBrowsable -= AssociatedObjectOnIsPropertyBrowsable;

        }
    }
}