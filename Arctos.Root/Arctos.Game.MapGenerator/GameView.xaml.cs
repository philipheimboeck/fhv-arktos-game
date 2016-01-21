using Arctos.Game.MapGenerator.View;
using System.Windows;

namespace Arctos.Game.MapGenerator
{
    /// <summary>
    /// Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : Window
    {
        public GameView(RobotViewModel robotViewModel)
        {
            this.DataContext = new Arctos.View.GameViewModel(robotViewModel, 3, 3);
            InitializeComponent();
        }
    }
}