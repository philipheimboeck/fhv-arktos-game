using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Arctos.Game.Client.Model;
using Arctos.Game.Client.Service;

namespace Arctos.Game.Client
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties

        public GameArea Game { get; set; }

        public int GameRows
        {
            get
            {
                return this.Game != null ? this.Game.Rows + 1 : 0;
            }
        }

        public int GameColumns
        {
            get
            {
                return this.Game != null ? this.Game.Columns + 1 : 0;
            }
        }

        #endregion

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
            #region example gamearea

            GameArea game = new GameArea
            {
                AreaWidth = 100,
                AreaHeight = 100,
                AreaList = new TrulyObservableCollection<Area>
                {
                    new Area
                    {
                        AreaID = "",
                        Column = 0,
                        Row = 0,
                        IsActive = false
                    },
                    new Area
                    {
                        AreaID = "",
                        Column = 1,
                        Row = 0,
                        IsActive = false
                    },
                    new Area
                    {
                        AreaID = "",
                        Column = 0,
                        Row = 1,
                        IsActive = true
                    },
                    new Area
                    {
                        AreaID = "",
                        Column = 1,
                        Row = 1,
                        IsActive = false
                    }
                }
            };

            this.Game = game;

            //for (int i = 0; i <= game.Columns; i++)
            //{
            //    GameAreaGrid.ColumnDefinitions.Add(new ColumnDefinition());
            //}
            //for (int i = 0; i <= game.Rows; i++)
            //{
            //    GameAreaGrid.RowDefinitions.Add(new RowDefinition());
            //}

            //for (int row = 0; row < GameAreaGrid.RowDefinitions.Count; row ++)
            //{
            //    for (int column = 0; column < GameAreaGrid.ColumnDefinitions.Count; column ++)
            //    {
            //        Label lbl = new Label
            //        {
            //            Background = new SolidColorBrush(Colors.WhiteSmoke),
            //            Width = game.AreaWidth,
            //            Height = game.AreaHeight
            //        };
            //        lbl.SetValue(Grid.ColumnProperty, column);
            //        lbl.SetValue(Grid.RowProperty, row);
            //        GameAreaGrid.Children.Add(lbl);
            //    }
            //}

            #endregion
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Game.AreaList.First().IsActive = true;
        }
    }
}
