using Arctos.Game.MapGenerator.View;
using System.Windows;

namespace Arctos.Game.MapGenerator
{
    /// <summary>
    /// Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : Window
    {
        public GameView(RobotViewModel robotViewModel, int columns, int rows)
        {
            this.DataContext = new GameViewModel(robotViewModel, columns, rows);
            InitializeComponent();
        }
    }
}