using System;
using System.Windows.Input;
using Arctos.Game.Model;
using ArctosGameServer.Controller;
using ArctosGameServer.Domain;
using ArctosGameServer.ViewModel.Command;

namespace ArctosGameServer.ViewModel
{
    public class PlayerViewModel : PropertyChangedBase
    {
        private GameController _controller;

        public Player Player { get; private set; }
        public ICommand KickCommand { get; set; }

        public PlayerViewModel(Player player, GameController controller)
        {
            Player = player;
            KickCommand = new KickCommand() { Action = KickAction };
            _controller = controller;
            Connected = true;
        }

        public string Name
        {
            get { return Player.Name; }
        }

        public string GuiStatusImagePath
        {
            get
            {
                return Player.GuiId.Equals(Guid.Empty)
                    ? "pack://application:,,,/ArctosGameServer;component/Resources/monitor_disconnected.png"
                    : "pack://application:,,,/ArctosGameServer;component/Resources/monitor_connected.png";
            }
        }

        private bool _connected;
        public bool Connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                OnPropertyChanged();
            }
        }

        public void ChangeProperty(string property)
        {
            OnPropertyChanged(property);
        }

        /// <summary>
        /// Kicks the player
        /// </summary>
        /// <param name="playerId"></param>
        private void KickAction()
        {
            _controller.KickPlayer(Player);
        }
    }
}