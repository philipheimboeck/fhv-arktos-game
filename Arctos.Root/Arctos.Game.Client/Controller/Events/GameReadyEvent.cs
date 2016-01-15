using System;
using Arctos.Game.Middleware.Logic.Model.Model;

namespace Arctos.Controller.Events
{
    public delegate void GameReadyEventHandler(object sender, GameReadyEventArgs e);

    public class GameReadyEventArgs : EventArgs
    {
        public Path Path { get; set; }

        public GameReadyEventArgs(Path path)
        {
            Path = path;
        }
    }
}