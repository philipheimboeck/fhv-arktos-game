using Arctos.Game.MapGenerator.View;
using System.Windows;

namespace Arctos.Game.MapGenerator
{
    /// <summary>
    /// Interaction logic for ControlUnitView.xaml
    /// </summary>
    public partial class RobotView : Window
    {
        public RobotView()
        {
            this.DataContext = new RobotViewModel("COM41");
            InitializeComponent();

            LogBox.TextChanged += (sender, args) =>
            {
                LogBox.CaretIndex = LogBox.Text.Length;
                LogBox.ScrollToEnd();
            }; 
        }
    }
}