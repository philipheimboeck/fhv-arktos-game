using System;
using Arctos.Game.Model;
using ArctosGameServer.Domain;

namespace ArctosGameServer.ViewModel
{
    public class PlayerViewModel : PropertyChangedBase
    {
        public Player Player { get; }

        public PlayerViewModel(Player player)
        {
            Player = player;
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

        public void ChangeProperty(string property)
        {
            OnPropertyChanged(property);
        }
    }
}