using System;
using System.Windows;
using Xceed.Wpf.Toolkit;

namespace Twm.Extensions
{
    public static class WatermarkPasswordBoxHelper
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password",
                typeof(String), typeof(WatermarkPasswordBoxHelper),
                new FrameworkPropertyMetadata(String.Empty, OnPasswordPropertyChanged));

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached(nameof(Attach),
                typeof(Boolean), typeof(WatermarkPasswordBoxHelper),
                new PropertyMetadata(false, Attach));

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(Boolean),
                typeof(WatermarkPasswordBoxHelper));

        public static void SetAttach(DependencyObject dp, Boolean value) =>
            dp.SetValue(AttachProperty, value);

        public static Boolean GetAttach(DependencyObject dp) =>
            (Boolean) dp.GetValue(AttachProperty);

        public static String GetPassword(DependencyObject dp) =>
            (String) dp.GetValue(PasswordProperty);

        public static void SetPassword(DependencyObject dp, String value) =>
            dp.SetValue(PasswordProperty, value);

        private static Boolean GetIsUpdating(DependencyObject dp) => 
            (Boolean) dp.GetValue(IsUpdatingProperty);

        private static void SetIsUpdating(DependencyObject dp, Boolean value) =>
            dp.SetValue(IsUpdatingProperty, value);

        private static void OnPasswordPropertyChanged(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is WatermarkPasswordBox passwordBox))
            {
                return;
            }

            passwordBox.PasswordChanged -= HandlePasswordChanged;

            if (!GetIsUpdating(passwordBox))
            {
                passwordBox.Password = (String) e.NewValue;
            }

            passwordBox.PasswordChanged += HandlePasswordChanged;
        }

        private static void Attach(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is WatermarkPasswordBox passwordBox))
            {
                return;
            }

            if ((Boolean) e.OldValue)
            {
                passwordBox.PasswordChanged -= HandlePasswordChanged;
            }

            if ((Boolean) e.NewValue)
            {
                passwordBox.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void HandlePasswordChanged(
            Object sender,
            RoutedEventArgs e)
        {
            if (!(sender is WatermarkPasswordBox passwordBox))
            {
                return;
            }

            SetIsUpdating(passwordBox, true);
            SetPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }
    }
}