using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Twm.Core.DataCalc.Optimization;

namespace Twm.Core.CustomProperties.Controls
{
    /// <summary>
    /// Логика взаимодействия для EnumControl.xaml
    /// </summary>
    public partial class EnumControl : UserControl
    {
        private EnumOptimizerParameter _optimizerParameter;

        private List<CheckBox> _checkBoxes;

        public EnumControl()
        {
            InitializeComponent();
            DataContextChanged += EnumControl_DataContextChanged;
        }

        private void EnumControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is EnumOptimizerParameter optimizerParameter)
            {
                _optimizerParameter = optimizerParameter;

                _checkBoxes = new List<CheckBox>();

                var names = Enum.GetNames(optimizerParameter.Type);

                var i = 0;
                foreach (var name in names)
                {
                    var checkBox = new CheckBox()
                    {
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        Margin = new Thickness(0, 2, 10, 2),
                        FontSize = 14,
                        Content = name
                    };
                    checkBox.Checked += CheckBox_Checked;
                    checkBox.Unchecked += CheckBox_Unchecked;

                    checkBox.SetBinding(CheckBox.IsCheckedProperty, new Binding($"Values[{i}]"));
                    _checkBoxes.Add(checkBox);

                    i++;

                    panel.Children.Add(checkBox);
                }

                CheckForOne();
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _optimizerParameter.UpdateProperty("CombinationCount");
            if (_optimizerParameter.Values.Count(x => x) == 1)
            {
                CheckForOne();
            }
        }

        private void CheckForOne()
        {
            if (_optimizerParameter.Values.Count(x => x) == 1)
            {
                for (int i = 0; i < _checkBoxes.Count; i++)
                {
                    if (_optimizerParameter.Values[i])
                    {
                        _checkBoxes[i].IsEnabled = false;
                    }
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _optimizerParameter.UpdateProperty("CombinationCount");
            if (_optimizerParameter.Values.Count(x => x) > 1)
            {
                foreach (var checkBox in _checkBoxes)
                {
                    checkBox.IsEnabled = true;
                }
            }
        }
    }
}