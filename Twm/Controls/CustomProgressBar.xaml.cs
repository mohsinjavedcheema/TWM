using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace Twm.Controls
{
    /// <summary>
    /// Interaction logic for CustomProgressBar.xaml
    /// </summary>
    public partial class CustomProgressBar : UserControl
    {
        public CustomProgressBar()
        {
            InitializeComponent();
            SizeChanged += CustomProgressBar_SizeChanged;

            Loaded += new RoutedEventHandler(MyProgressBar_Loaded);
            IsVisibleChanged += CustomProgressBar_IsVisibleChanged;
        }

        private void CustomProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Update();
        }

        private void CustomProgressBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update();
        }

        void MyProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        public static readonly DependencyProperty FillColorProperty = DependencyProperty.Register(
            "FillColor", typeof(Brush), typeof(CustomProgressBar), new PropertyMetadata(Brushes.White));

        public Brush FillColor
        {
            get { return (Brush)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }

        private static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double?), typeof(CustomProgressBar), new PropertyMetadata(100d, OnMaximumChanged));
        public double? Maximum
        {
            get { return (double?)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }


        private static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(CustomProgressBar), new PropertyMetadata(0d, OnMinimumChanged));
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        private static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double?), typeof(CustomProgressBar), new PropertyMetadata(0d, OnValueChanged));
        public double? Value
        {
            get { return (double?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        private static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(HorizontalAlignment), typeof(CustomProgressBar), new PropertyMetadata(HorizontalAlignment.Left, OnHorizontalAlignmentChanged));
        public HorizontalAlignment Orientation
        {
            get { return (HorizontalAlignment)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        private static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Brush), typeof(CustomProgressBar), new PropertyMetadata(Brushes.Green, OnBackgroundColorChanged));
        public Brush BackgroundColor
        {
            get { return (Brush)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        private static readonly DependencyProperty ProgressBarWidthProperty = DependencyProperty.Register("ProgressBarWidth", typeof(double), typeof(CustomProgressBar), null);
        private double ProgressBarWidth
        {
            get { return (double)GetValue(ProgressBarWidthProperty); }
            set { SetValue(ProgressBarWidthProperty, value); }
        }


        private static readonly DependencyProperty IsApplyOpacityProperty = DependencyProperty.Register("IsApplyOpacity", typeof(bool), typeof(CustomProgressBar), null);
        public bool IsApplyOpacity
        {
            get { return (bool)GetValue(IsApplyOpacityProperty); }
            set { SetValue(IsApplyOpacityProperty, value); }
        }

        static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            (o as CustomProgressBar).Update();
        }

        static void OnMinimumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            (o as CustomProgressBar).Update();
        }

        static void OnMaximumChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            (o as CustomProgressBar).Update();
        }

        static void OnHorizontalAlignmentChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            (o as CustomProgressBar).Update();
        }


        static void OnBackgroundColorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            (o as CustomProgressBar).Update();
        }

        


        void Update()
        {
            var value = Value ?? 0;
            var max = Maximum ?? 0;

            if (value == 0 || max == 0)
                return;


            var width =  Math.Min((value / (max + Minimum) * this.ActualWidth) - 2, this.ActualWidth - 2);

            if (width < 0)
                width = 0;

            if (IsApplyOpacity)
            {
                if (Value == 0)
                    Opacity = 0;
                else
                {
                    Opacity = 0.2 + ((value * 0.8) / (max + Minimum));
                }
            }

            

            ProgressBarWidth = width;
        }
    }
}