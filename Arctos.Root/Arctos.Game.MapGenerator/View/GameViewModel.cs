using System;
using System.Threading;
using System.Windows.Threading;
using Arctos.Game.Model;
using Arctos.Game.MapGenerator;
using Arctos.Game.MapGenerator.View;
using Arctos.Game.MapGenerator.View.Events;
using Arctos.Game.MapGenerator.View;
using System.Xml.Serialization;
using Microsoft.Win32;
using System.Windows;

namespace Arctos.View
{
    /// <summary>
    /// GUI View Model
    /// </summary>
    public class GameViewModel : PropertyChangedBase
    {
        #region Properties

        private bool closeTrigger;
        public bool CloseTrigger
        {
            get { return this.closeTrigger; }
            set
            {
                this.closeTrigger = value;
                OnPropertyChanged();
            }
        }

        private bool _showGameInformation = false;
        public bool ShowGameInformation
        {
            get { return _showGameInformation; }
            set
            {
                _showGameInformation = value;
                OnPropertyChanged();
            }
        }

        private string _gameInformation;
        public string GameInformation
        {
            get { return _gameInformation; }
            set
            {
                _gameInformation = value;
                OnPropertyChanged();
            }
        }

        private GuiGameArea _guiGameInstance;
        public GuiGameArea GUIGameInstance
        {
            get { return _guiGameInstance; }
            set
            {
                _guiGameInstance = value;
                OnPropertyChanged();
            }
        }

        private GameConfiguration _gameConfiguration;
        public GameConfiguration GameConfiguration
        {
            get
            {
                return _gameConfiguration;
            }

            set
            {
                _gameConfiguration = value;
            }
        }

        private GameArea _currentGameArea;
        public GameArea CurrentGameArea
        {
            get
            {
                return _currentGameArea;
            }

            set
            {
                _currentGameArea = value;
            }
        }

        private Tuple<int, int> _active;
        public Tuple<int, int> Active
        {
            get
            {
                return _active;
            }

            set
            {
                _active = value;
                OnActiveChanged();
            }
        }
        
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="area"></param>
        public GameViewModel(RobotViewModel robotViewModel, int columns, int rows)
        {
            try
            {
                InitializeGameViewModel(columns, rows);

                // robot viewmodel to receive incoming rfid events
                robotViewModel.RFIDUpdateEvent += OnRFIDUpdateEvent;
            }
            catch (Exception ex)
            {
                this.ShowInformationOverlay(ex.Message);
            }
        }
    
        /// <summary>
        /// Initialize GameView Model
        /// </summary>
        private void InitializeGameViewModel(int columns, int rows)
        {
            GameConfiguration = new GameConfiguration(columns, rows);
            GameConfiguration.Columns = columns;
            GameConfiguration.Rows = rows;
            CurrentGameArea = new GameArea();

            // create all areas
            GameArea guiGameArea = new GameArea();
            for ( int column = 0; column < columns; column++)
            {
                for ( int row = 0; row < rows; row++)
                {
                    Area field = new Area();
                    field.Column = column;
                    field.Row = row;
                    field.Status = Area.AreaStatus.None;
                    guiGameArea.AreaList.Add(field);
                }
            }

            // create gui
            GUIGameInstance = new GuiGameArea(guiGameArea)
            {
                AreaWidth = 800,
                AreaHeight = 600
            };
        }

        private void OnActiveChanged()
        {
            if (Active == null) return;

            GuiArea area = GetActiveArea();
            if (area != null)
            {
                area.Status = Area.AreaStatus.WronglyPassed;
            }
        }

        private GuiArea GetActiveArea()
        {
            foreach (GuiArea area in GUIGameInstance.AreaList)
            {
                if (Active != null && area.Column == Active.Item1 && area.Row == Active.Item2)
                {
                    return area;
                }
            }

            return null;
        }

        private void IncreaseActive()
        {
            if (Active == null)
            {
                Active = new Tuple<int, int>(0, 0);
                return;
            }

            int currentColumn = Active.Item1;
            int currentRow = Active.Item2;

            if (currentColumn < GameConfiguration.Columns - 1)
            {
                currentColumn++;
            } else
            {
                if (currentRow < GameConfiguration.Rows - 1)
                {
                    currentRow++;
                    currentColumn = 0;
                } else
                {
                    // gui area complete;
                    ShowInformationOverlay("Exporting map and close app");
                    ExportMap();
                    return;
                }
            }

            Active = new Tuple<int, int>(currentColumn, currentRow);
        }

        private void ExportMap()
        {
            GameConfiguration.GameAreas.Add(CurrentGameArea);

            try
            {
                // export GameConfiguration
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "game_map";
                saveFileDialog.DefaultExt = ".xml";
                saveFileDialog.Filter = "XML files (.xml)|*.xml";
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == true)
                {
                    XmlSerializer writer = new XmlSerializer(typeof(GameConfiguration));
                    string path = saveFileDialog.FileName;
                    System.IO.FileStream file = System.IO.File.Create(path);

                    writer.Serialize(file, GameConfiguration);
                    file.Close();
                }
            }
            catch (Exception e)
            {
                //
            }

            Environment.Exit(1);
        }

        #region EventHandlers

        /// <summary>
        /// RFID Update Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="rfidUpdateEventArgs"></param>
        private void OnRFIDUpdateEvent(object sender, RFIDUpdateEventArgs rfidUpdateEventArgs)
        {
            var receivedRFIDUpdate = rfidUpdateEventArgs.RFID;
            if (receivedRFIDUpdate == null) return;

            GuiArea guiArea = GetActiveArea();
            if (guiArea != null)
            {
                guiArea.Status = Area.AreaStatus.CorrectlyPassed;
                
                CurrentGameArea.AreaList.Add(new Area()
                {
                    AreaId = rfidUpdateEventArgs.RFID,
                    Column = guiArea.Column,
                    Row = guiArea.Row,
                    Status = Area.AreaStatus.None
                });

                ShowInformationOverlay("Waiting for next RFID");
            } else
            {
                ShowInformationOverlay("Scanned RFID for Start Field");
                CurrentGameArea.StartField = new Area()
                {
                    AreaId = rfidUpdateEventArgs.RFID
                };
            }

            IncreaseActive();
        }
        #endregion

        #region View Helper

        /// <summary>
        /// Show a message overlay
        /// </summary>
        /// <param name="message"></param>
        private void ShowInformationOverlay(string message)
        {
            this.GameInformation = message;
            this.ShowGameInformation = true;

            Wait(2);

            this.ShowGameInformation = false;
            this.GameInformation = "";
        }

        /// <summary>
        /// GUI Wait for x seconds
        /// </summary>
        /// <param name="seconds"></param>
        private void Wait(double seconds)
        {
            var frame = new DispatcherFrame();
            new Thread((ThreadStart)(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                frame.Continue = false;
            })).Start();
            Dispatcher.PushFrame(frame);
        }

        #endregion
    }
}