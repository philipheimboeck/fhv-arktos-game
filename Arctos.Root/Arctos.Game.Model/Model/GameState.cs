using System;

namespace Arctos.Game.Middleware.Logic.Model.Model
{
    [Serializable]
    public enum GameState
    {
        Waiting,
        Ready,
        Started,
        Finished
    }
}