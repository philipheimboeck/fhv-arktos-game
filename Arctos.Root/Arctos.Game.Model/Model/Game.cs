using System;
using System.Collections;
using System.Collections.Generic;
using Arctos.Game.Model;

namespace Arctos.Game.Middleware.Logic.Model.Model
{
    [Serializable]
    public class Game
    {
        public GameState State { get; set; }

        public GameArea GameArea { get; set; }

        public Path Path { get; set; }
    }
}