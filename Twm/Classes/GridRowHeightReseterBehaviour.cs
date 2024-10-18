﻿using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Twm.Classes
{
    public class GridRowHeightReseterBehaviour : Behavior<Expander>
    {
        private Grid _parentGrid;
        public int TargetGridRowIndex { get; set; }
        protected override void OnAttached()
        {
            AssociatedObject.Expanded += AssociatedObject_Expanded;
            AssociatedObject.Collapsed += AssociatedObject_Collapsed;
            FindParentGrid();
        }

        private void FindParentGrid()
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(AssociatedObject);
            while (parent != null)
            {
                if (parent is Grid)
                {
                    _parentGrid = parent as Grid;
                    return;
                }
                parent = LogicalTreeHelper.GetParent(AssociatedObject);
            }
        }

        void AssociatedObject_Collapsed(object sender, System.Windows.RoutedEventArgs e)
        {
            _parentGrid.RowDefinitions[TargetGridRowIndex].Height = GridLength.Auto;
        }

        void AssociatedObject_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            _parentGrid.RowDefinitions[TargetGridRowIndex].Height = GridLength.Auto;
        }
    }
}