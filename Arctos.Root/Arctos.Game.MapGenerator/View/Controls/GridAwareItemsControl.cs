using System.Windows;
using System.Windows.Controls;

namespace Arctos.Game.MapGenerator
{
    public class GridAwareItemsControl : ItemsControl
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            var container = (ContentPresenter) base.GetContainerForItemOverride();
            if (ItemTemplate == null)
            {
                return container;
            }

            var content = (FrameworkElement) ItemTemplate.LoadContent();
            var rowBinding = content.GetBindingExpression(Grid.RowProperty);
            var columnBinding = content.GetBindingExpression(Grid.ColumnProperty);

            if (rowBinding != null)
            {
                container.SetBinding(Grid.RowProperty, rowBinding.ParentBinding);
            }

            if (columnBinding != null)
            {
                container.SetBinding(Grid.ColumnProperty, columnBinding.ParentBinding);
            }

            return container;
        }
    }
}