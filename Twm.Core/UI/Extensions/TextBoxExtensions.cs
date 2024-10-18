using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Twm.Core.UI.Extensions
{
    public static class TextBoxExtensions
    {
        #region Placeholder
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.RegisterAttached(
            "Placeholder", typeof(string), typeof(TextBoxExtensions), new PropertyMetadata(default(string), propertyChangedCallback: PlaceholderChanged));
       
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetPlaceholder(DependencyObject element, string value)
        {
            element.SetValue(PlaceholderProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static string GetPlaceholder(DependencyObject element)
        {
            return (string)element.GetValue(PlaceholderProperty);
        }

        private static void PlaceholderChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var tb = dependencyObject as TextBox;
            if (tb == null)
                return;            

            tb.LostFocus -= OnLostFocus;
            tb.GotFocus -= OnGotFocus;
            tb.TextChanged -= OnTextChanged;

            if (args.NewValue != null)
            {
                tb.GotFocus += OnGotFocus;
                tb.LostFocus += OnLostFocus;                
                tb.TextChanged += OnTextChanged;
            }
            if (string.IsNullOrEmpty(tb.Text) || string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = GetPlaceholder(tb);
                tb.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                tb.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (!tb.IsFocused)
            {
                if (string.IsNullOrEmpty(tb.Text) || string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = GetPlaceholder(tb);
                    tb.Foreground = new SolidColorBrush(Colors.Gray);
                }
                else
                {
                    tb.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
        }

        private static void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrEmpty(tb.Text) || string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = GetPlaceholder(tb);
                tb.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                tb.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private static void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            var tb = sender as TextBox;
            var ph = GetPlaceholder(tb);
            if (tb.Text == ph)
            {
                tb.Text = string.Empty;
                tb.Foreground = new SolidColorBrush(Colors.Black);
            }
        }
        #endregion

       


    }
}