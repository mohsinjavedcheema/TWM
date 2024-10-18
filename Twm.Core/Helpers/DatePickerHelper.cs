using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Twm.Core.Helpers
{
    public static  class DatePickerHelper
    {
        public static readonly DependencyProperty ShowTodayButtonProperty = DependencyProperty.RegisterAttached("ShowTodayButton", typeof(bool), typeof(DatePickerHelper), new FrameworkPropertyMetadata(false, ShowTodayButtonChanged));
        public static readonly DependencyProperty ShowTodayButtonContentProperty = DependencyProperty.RegisterAttached("ShowTodayButtonContent", typeof(string), typeof(DatePickerHelper), new FrameworkPropertyMetadata("Today", ShowTodayButtonContentChanged));

        private static void ShowTodayButtonContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Call twice to update Button Content if was setted after ShowTodayButton
            SetShowTodayButton(d, !GetShowTodayButton(d));
            SetShowTodayButton(d, !GetShowTodayButton(d));
        }

        private static void ShowTodayButtonChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is DatePicker)
            {
                var d = (DatePicker)sender;
                var showButton = (bool)(e.NewValue);

                if (showButton)
                {
                    //I want to reproduce this xaml in c#:
                    //<Style TargetType="{x:Type Calendar}">
                    //    <Setter Property="Template">
                    //        <Setter.Value>
                    //            <ControlTemplate TargetType="{x:Type Calendar}">
                    //                <StackPanel Name="PART_Root"
                    //                            HorizontalAlignment="Center">
                    //                    <CalendarItem Name="PART_CalendarItem"
                    //                                    Background="{TemplateBinding Control.Background}"
                    //                                    BorderBrush="{TemplateBinding Control.BorderBrush}"
                    //                                    BorderThickness="{TemplateBinding Control.BorderThickness}"
                    //                                    Style="{TemplateBinding Calendar.CalendarItemStyle}" />
                    //                    <Button Command="SelectToday"
                    //                            Content="Today" />
                    //                </StackPanel>
                    //            </ControlTemplate>
                    //        </Setter.Value>
                    //    </Setter>
                    //</Style>

                    Setter setter = new Setter();
                    setter.Property = Calendar.TemplateProperty;
                    ControlTemplate template = new ControlTemplate(typeof(Calendar));
                    var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
                    stackPanel.Name = "PART_Root";
                    stackPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);

                    var calendar = new FrameworkElementFactory(typeof(CalendarItem));
                    calendar.Name = "PART_CalendarItem";

                    calendar.SetBinding(CalendarItem.BackgroundProperty,
                        new Binding(CalendarItem.BackgroundProperty.Name)
                        {
                            Path = new PropertyPath(Control.BackgroundProperty),
                            RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)
                        });

                    calendar.SetBinding(CalendarItem.BorderBrushProperty, new Binding(CalendarItem.BorderBrushProperty.Name)
                    {
                        Path = new PropertyPath(Control.BorderBrushProperty),
                        RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)
                    });

                    calendar.SetBinding(CalendarItem.BorderThicknessProperty, new Binding(CalendarItem.BorderThicknessProperty.Name)
                    {
                        Path = new PropertyPath(Control.BorderThicknessProperty),
                        RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)
                    });

                    calendar.SetBinding(CalendarItem.StyleProperty, new Binding(CalendarItem.StyleProperty.Name)
                    {
                        Path = new PropertyPath(Calendar.CalendarItemStyleProperty),
                        RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)
                    });

                    stackPanel.AppendChild(calendar);

                    var btn = new FrameworkElementFactory(typeof(Button));
                    btn.SetValue(Button.ContentProperty, GetShowTodayButtonContent(d));

                    var SelectToday = new RoutedCommand("Today", typeof(DatePickerHelper));

                    d.CommandBindings.Add(new CommandBinding(SelectToday,
              (s, ea) =>
              {
                  (s as DatePicker).SelectedDate = DateTime.Now.Date;
                  (s as DatePicker).IsDropDownOpen = false;
              },
              (s, ea) => { ea.CanExecute = true; }));

                    btn.SetValue(Button.CommandProperty, SelectToday);

                    stackPanel.AppendChild(btn);

                    template.VisualTree = stackPanel;
                    setter.Value = template;

                    Style Temp = new Style(typeof(Calendar));

                    Temp.Setters.Add(setter);

                    d.CalendarStyle = Temp;
                }
            }
        }

        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static bool GetShowTodayButton(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowTodayButtonProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static void SetShowTodayButton(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowTodayButtonProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static string GetShowTodayButtonContent(Control obj)
        {
            return (string)obj.GetValue(ShowTodayButtonContentProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(DatePicker))]
        public static void SetShowTodayButtonContent(Control obj, string value)
        {
            obj.SetValue(ShowTodayButtonContentProperty, value);
        }
    }
}