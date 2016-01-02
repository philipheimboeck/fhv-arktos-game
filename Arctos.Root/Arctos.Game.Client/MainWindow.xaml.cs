using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Arctos.Game.Client.Model;

namespace Arctos.Game.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GameArea Game { get; set; }

        public MainWindow()
        {
            InitializeComponent(); 
            this.DataContext = this;

            GetExampleGame();
        }

        /// <summary>
        /// Generate a example game field
        /// </summary>
        private void GetExampleGame()
        {
            ObservableCollection<Area> areas = new ObservableCollection<Area>();
            for (int rows = 0; rows < 8; rows++)
            {
                for (int cols = 0; cols < 4; cols++)
                {
                    areas.Add(new Area
                    {
                        AreaID = "",
                        Column = cols,
                        Row =  rows,
                        IsActive = false
                    });
                }
            }

            GameArea game = new GameArea
            {
                AreaWidth = 10,
                AreaHeight = 10,
                AreaList = areas
            };

            this.Game = game;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Game.AreaList.First().IsActive = true;
        }
    }
}
