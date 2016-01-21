using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Arctos.Game.Model;
using ArctosGameServer.Controller;
using ArctosGameServer.Controller.Events;
using ArctosGameServer.ViewModel.Command;

namespace ArctosGameServer.ViewModel
{
    public class GameViewModel : PropertyChangedBase
    {
        private GameController _game;

        private bool _gameStartable;

        private string _log;

       

        public GameViewModel(GameController game)
        {
            _game = game;

            Players = new ObservableCollection<PlayerViewModel>();

            // React on Events
            _game.PlayerJoinedEvent += PlayerJoinedEvent;
            _game.PlayerLeftEvent += PlayerLeftEvent;
            _game.GuiChangedEvent += GuiChangedEvent;
            _game.GameReadyEvent += GameReadyEvent;
            _game.GameStartEvent += GameStartEvent;
            _game.LogEvent += LogEvent;
            _game.PlayerLostEvent += PlayerLostEvent;
        }

        private void PlayerLostEvent(object sender, PlayerLostEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                var playerViewModel = Players.FirstOrDefault(x => x.Player.Equals(e.Player));
                if (playerViewModel != null) playerViewModel.Connected = !e.Lost;
            });
        }

        private void PlayerLeftEvent(object sender, Controller.Events.PlayerLeftEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action) delegate
            {
                var playerViewModel = Players.FirstOrDefault(x => x.Player.Equals(e.Player));
                if (playerViewModel != null)
                {
                    Players.Remove(playerViewModel);
                }
            });
        }

        public ObservableCollection<PlayerViewModel> Players { get; set; }

        public string Log
        {
            get { return _log; }
            set
            {
                _log = value;
                OnPropertyChanged();
            }
        }

        public bool GameStartable
        {
            get { return _gameStartable; }
            set
            {
                _gameStartable = value;
                OnPropertyChanged();
            }
        }

        private void GameStartEvent(object sender, Controller.Events.GameStartEventArgs e)
        {
            GameStartable = false;
        }

        private void GameReadyEvent(object sender, Controller.Events.GameReadeEventArgs e)
        {
            GameStartable = e.Ready;
        }

        private void GuiChangedEvent(object sender, Controller.Events.GuiChangedEventArgs e)
        {
            var playerViewModel = Players.FirstOrDefault(x => x.Player.Equals(e.Player));
            if (playerViewModel != null) playerViewModel.ChangeProperty("GuiStatusImagePath");
        }

        private void LogEvent(object sender, Controller.Events.LogEventArgs e)
        {
            Log = Log + "[" + DateTime.Now.ToLongTimeString() + "] " + e.Log + "\r\n";
        }

        private void PlayerJoinedEvent(object sender, Controller.Events.PlayerJoinedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action) delegate { Players.Add(new PlayerViewModel(e.Player, _game)); });
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