using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Arctos.Game.Client
{
    public class GridAwareItemsControl : ItemsControl
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            ContentPresenter container = (ContentPresenter)base.GetContainerForItemOverride();
            if (ItemTemplate == null)
            {
                return container;
            }

            FrameworkElement content = (FrameworkElement)ItemTemplate.LoadContent();
            BindingExpression rowBinding = content.GetBindingExpression(Grid.RowProperty);
            BindingExpression columnBinding = content.GetBindingExpression(Grid.ColumnProperty);

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