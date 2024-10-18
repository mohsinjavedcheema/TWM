using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Twm.Core.Classes;

namespace Twm.Core.UI.Controls
{
    public class CustomizationListView : ListView
    {
        public static readonly DependencyProperty GridColumnCustomizationProperty =
            DependencyProperty.Register("GridColumnCustomization", typeof(GridColumnCustomization), typeof(CustomizationListView));


        public GridColumnCustomization GridColumnCustomization
        {
            get { return (GridColumnCustomization)GetValue(GridColumnCustomizationProperty); }
            set { SetValue(GridColumnCustomizationProperty, value); }
        }

        private GridView _gridView;

        private GridColumnCustomization _gGridColumnCustomization;

        public CustomizationListView()
        {

            Loaded += OnLoaded;
            Unloaded -= OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (GridColumnCustomization != null)
            {
                GridColumnCustomization.ColumnsCollectionChanged -= GridColumnCustomization_ColumnsCollectionChanged;
                _gGridColumnCustomization = null;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (GridColumnCustomization != null)
            {
                GridColumnCustomization.ColumnsCollectionChanged += GridColumnCustomization_ColumnsCollectionChanged;
                _gGridColumnCustomization = GridColumnCustomization;
            }


            if (View is GridView gridView)
            {
                _gridView = gridView;
                AddColumns();
                _gridView.Columns.CollectionChanged += Columns_CollectionChanged;
            }
        }

        private void GridColumnCustomization_ColumnsCollectionChanged(object sender, System.EventArgs e)
        {
            _gridView.Columns.Clear();
            AddColumns();
        }

        private void AddColumns()
        {
            foreach (var column in _gGridColumnCustomization.Columns)
            {
                if (column.Visibility == Visibility.Visible)
                {
                    var gridViewColumn = new GridViewColumn() { Header = column.Caption, Width = column.Width };
                    
                    BindingOperations.SetBinding(gridViewColumn, GridViewColumn.WidthProperty,
                        new Binding() { Source  = column, Path = new PropertyPath("Width"), Mode = BindingMode.TwoWay, });

                    column.PropertyChanged += Column_PropertyChanged;

                    _gridView.Columns.Add(gridViewColumn);
                }
            }

        }

        private void Column_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Width")
            {
                _gGridColumnCustomization.UpdateProperty("Columns");
                _gGridColumnCustomization.SaveLayout();
            }
        }

        private void OnUpdate()
        {

        }

        private void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                var item = (GridViewColumn)e.NewItems[0];

                var gridColumnInfo = _gGridColumnCustomization.Columns.FirstOrDefault(x => x.Caption == item.Header.ToString());

                if (gridColumnInfo != null)
                {
                    _gGridColumnCustomization.Columns.Move(_gGridColumnCustomization.Columns.IndexOf(gridColumnInfo), e.NewStartingIndex);
                    _gGridColumnCustomization.UpdateProperty("Columns");
                    _gGridColumnCustomization.SaveLayout();
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {

            }
        }
    }
}