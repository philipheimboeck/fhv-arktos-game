﻿using System;
using System.Xml.Serialization;

namespace Arctos.Game.Middleware.Logic.Model.Model
{
    [Serializable]
    public class GameEventTuple<T1, T2>
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }
    }
}