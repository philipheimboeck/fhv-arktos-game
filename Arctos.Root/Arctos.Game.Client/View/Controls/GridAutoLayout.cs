using System.Windows;
using System.Windows.Controls;

namespace Arctos.Game.Client
{
    public class GridAutoLayout
    {
        // Using a DependencyProperty as the backing store for NumberOfColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberOfColumnsProperty =
            DependencyProperty.RegisterAttached("NumberOfColumns", typeof (int), typeof (GridAutoLayout),
                new PropertyMetadata(1, NumberOfColumnsUpdated));

        // Using a DependencyProperty as the backing store for NumberOfRows.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberOfRowsProperty =
            DependencyProperty.RegisterAttached("NumberOfRows", typeof (int), typeof (GridAutoLayout),
                new PropertyMetadata(1, NumberOfRowsUpdated));

        public static int GetNumberOfColumns(DependencyObject obj)
        {
            return (int) obj.GetValue(NumberOfColumnsProperty);
        }

        public static void SetNumberOfColumns(DependencyObject obj, int value)
        {
            obj.SetValue(NumberOfColumnsProperty, value);
        }

        public static int GetNumberOfRows(DependencyObject obj)
        {
            return (int) obj.GetValue(NumberOfRowsProperty);
        }

        public static void SetNumberOfRows(DependencyObject obj, int value)
        {
            obj.SetValue(NumberOfRowsProperty, value);
        }

        private static void NumberOfRowsUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (Grid) d;

            grid.RowDefinitions.Clear();
            for (var i = 0; i < (int) e.NewValue; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() {Height = new GridLength(1, GridUnitType.Star)});
            }
        }

        private static void NumberOfColumnsUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (Grid) d;

            grid.ColumnDefinitions.Clear();
            for (var i = 0; i < (int) e.NewValue; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(1.0, GridUnitType.Star)});
            }
        }
    }
}