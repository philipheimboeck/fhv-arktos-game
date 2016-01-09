using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Arctos.Game.Model;
using ArctosGameServer.Controller;

namespace ArctosGameServer.ViewModel
{
    public class GameViewModel : PropertyChangedBase
    {
        private GameController _game;

        public ObservableCollection<PlayerViewModel> Players { get; set; }

        private bool _gameStartable;
        public bool GameStartable
        {
            get { return _gameStartable; }
            set
            {
                _gameStartable = value;
                OnPropertyChanged();
            }
        }

        public GameViewModel(GameController game)
        {
            _game = game;

            Players = new ObservableCollection<PlayerViewModel>();

            // React on Events
            _game.PlayerJoinedEvent += PlayerJoinedEvent;
            _game.GuiJoinedEvent += GuiJoinedEvent;
            _game.GameReadyEvent += GameReadyEvent;
            _game.GameStartEvent += GameStartEvent;
        }

        private void GameStartEvent(object sender, Controller.Events.GameStartEventArgs e)
        {
            GameStartable = false;
        }

        private void GameReadyEvent(object sender, Controller.Events.GameReadeEventArgs e)
        {
            GameStartable = true;
        }

        private void GuiJoinedEvent(object sender, Controller.Events.GuidJoinedEventArgs e)
        {
            var playerViewModel = Players.FirstOrDefault(x => x.Player.Equals(e.Player));
            if (playerViewModel != null) playerViewModel.ChangeProperty("GuiStatusImagePath");
        }

        private void PlayerJoinedEvent(object sender, Controller.Events.PlayerJoinedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action) delegate { Players.Add(new PlayerViewModel(e.Player)); });
        }

        public override void Execute(object parameter)
        {
            try
            {
                switch (parameter.ToString())
                {
                    case "StartGame":
                    {
                        if (GameStartable)
                        {
                            _game.StartGame();
                        }
                    }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}