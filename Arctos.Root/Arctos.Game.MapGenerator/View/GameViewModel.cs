using System;
using System.Threading;
using System.Windows.Threading;
using Arctos.Game.Model;
using Arctos.Game.MapGenerator.View.Events;
using System.Xml.Serialization;
using Microsoft.Win32;
using Arctos.Game.MapGenerator.Model;

namespace Arctos.Game.MapGenerator.View
{
    /// <summary>
    /// Game View Model
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
        public GameViewModel() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="robotViewModel"></param>
        /// <param name="columns"></param>
        /// <param name="rows"></param>
        public GameViewModel(RobotViewModel robotViewModel, int columns, int rows)
        {
            try
            {
                InitializeGameViewModel(columns, rows);

                // robot viewmodel to receive incoming rfid events
                robotViewModel.RFIDUpdateEvent += OnRFIDUpdateEvent;

                // show start message
                GameInformation = "Start scanning RFID for Start Field";
                ShowGameInformation = true;
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
            // create game configuration for export
            GameConfiguration = new GameConfiguration(columns, rows);
            GameConfiguration.Columns = columns;
            GameConfiguration.Rows = rows;
            CurrentGameArea = new GameArea();
            GameConfiguration.GameAreas.Add(CurrentGameArea);

            // create all areas only for gui yet
            GameArea guiGameArea = new GameArea();
            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
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

        #region Map Methods

        /// <summary>
        /// If the active field change
        /// mark that field as wrongly passed
        /// </summary>
        private void OnActiveChanged()
        {
            if (Active == null) return;

            GuiArea area = GetActiveArea();
            if (area != null)
            {
                area.Status = Area.AreaStatus.WronglyPassed;
            }
        }

        /// <summary>
        /// Get current active GUI area
        /// </summary>
        /// <returns>Returns active GUI area or null if active is not set yet (start field)</returns>
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

        /// <summary>
        /// Increase active field
        /// 
        /// If active is null, this means that it is the start field
        /// otherwise it is a visible gui area field
        /// 
        /// Increasing means iterate through each row and their columns
        /// e.g. for map with one row and one column
        /// R | C
        /// 0 | 0
        /// 0 | 1
        /// 1 | 1
        /// 
        /// If all fields are scanned, map will be exported
        /// </summary>
        private void IncreaseActive()
        {
            if (Active == null)
            {
                Active = new Tuple<int, int>(0, 0);
                return;
            }

            int currentColumn = Active.Item1;
            int currentRow = Active.Item2;

            // check if column is max
            // afterwards check if row is max
            if (currentColumn < GameConfiguration.Columns - 1)
            {
                currentColumn++;
            }
            else
            {
                if (currentRow < GameConfiguration.Rows - 1)
                {
                    currentRow++;
                    currentColumn = 0;
                }
                else
                {
                    // row and column max
                    // all fields scanned
                    // gui area complete;
                    ShowInformationOverlay("Exporting map and close app");
                    ExportMap();
                    return;
                }
            }

            Active = new Tuple<int, int>(currentColumn, currentRow);
        }

        /// <summary>
        /// A pop up will be displayed to export
        /// game configuration
        /// Afterwards exit application
        /// </summary>
        private void ExportMap()
        {
            try
            {
                // export GameConfiguration
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
                saveFileDialog.FileName = "game_map";
                saveFileDialog.DefaultExt = ".map";
                saveFileDialog.Filter = "Map Files (.map)|*.map";

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
            finally
            {
                Environment.Exit(1);
            }
        }

        #endregion

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

            // get current area
            GuiArea guiArea = GetActiveArea();
            if (guiArea != null)
            {
                // one gui field is passed
                guiArea.Status = Area.AreaStatus.CorrectlyPassed;

                // add field as area to current game area
                CurrentGameArea.AreaList.Add(new Area()
                {
                    AreaId = rfidUpdateEventArgs.RFID,
                    Column = guiArea.Column,
                    Row = guiArea.Row,
                    Status = Area.AreaStatus.None
                });

                ShowInformationOverlay("Waiting for next RFID");
            }
            else
            {
                // start field is scanned
                GameInformation = "";
                ShowGameInformation = false;
                ShowInformationOverlay("Scanned Start Field");
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