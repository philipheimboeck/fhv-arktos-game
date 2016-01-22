using System;
using System.Collections.Generic;
using System.ComponentModel;
using Arctos.Controller.Events;
using Arctos.Game.GUIClient;
using Arctos.Game.Middleware.Logic.Model.Client;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;

namespace Arctos.Controller
{
    public class ViewController
    {
        #region Properties

        public GameTcpClient GameClient { get; set; }

        public GameArea GameArea { get; set; }

        /// <summary>
        /// Area and Status
        /// </summary>
        private List<Tuple<GuiArea, Area.AreaStatus>> WrongPath { get; set; }

        private readonly BackgroundWorker eventBackgroundWorker = new BackgroundWorker();

        public event AreaUpdateEventHandler AreaUpdateEvent;
        public event PlayerLeftEventHandler PlayerLeftEvent;
        public event GameReadyEventHandler GameReadyEvent;
        public event GameStartEventHandler GameStartEvent;
        public event GameFinishEventHandler GameFinishEvent;
        public event PlayerFinishEventHandler PlayerFinishEvent;
        public event PlayerKickedEventHandler PlayerKickedEvent;
        public event PlayerLostEventHandler PlayerLostEvent;
        public event ErrorEventHandler ErrorEvent;
        public event PlayerJoinedEventHandler PlayerJoinedEvent;
        public event GameResetEventHandler GameResetEvent;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="area"></param>
        public ViewController(GameTcpClient client, GameArea area)
        {
            try
            {
                this.GameClient = client;
                this.GameArea = area;

                this.GameClient.ReceivedDataEvent += GameClientOnReceivedEvent;

                eventBackgroundWorker.WorkerSupportsCancellation = true;
                eventBackgroundWorker.DoWork += WaitForReceivingData;
                eventBackgroundWorker.RunWorkerAsync();

                WrongPath = new List<Tuple<GuiArea, Area.AreaStatus>>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Received a new GameEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="receivedEventArgs"></param>
        private void GameClientOnReceivedEvent(object sender, ReceivedEventArgs receivedEventArgs)
        {
            var receivedEvent = receivedEventArgs.Data as GameEvent;
            if (receivedEvent == null) return;
            switch (receivedEvent.EventType)
            {
                // Area Update
                case GameEvent.Type.AreaUpdate:
                    {
                        OnAreaUpdateEvent(new AreaUpdateEventArgs(receivedEvent.Data as Area));
                    }
                    break;

                // Player left, close the View
                case GameEvent.Type.PlayerLeft:
                    {
                        OnPlayerLeftEvent(new PlayerLeftEventArgs());
                    }
                    break;

                // Player left, close the View
                case GameEvent.Type.PlayerLost:
                    {
                        OnPlayerLostEvent(new PlayerLostEventArgs((bool)receivedEvent.Data));
                    }
                    break;

                // Player joined after connection failure
                case GameEvent.Type.PlayerJoined:
                    {
                        OnPlayerJoinedEvent(new PlayerJoinedEventArgs());
                    }
                    break;

                // Player left, close the View
                case GameEvent.Type.PlayerKicked:
                    {
                        OnPlayerKickedEvent(new PlayerKickedEventArgs());
                    }
                    break;

                // Game Ready, show the path
                case GameEvent.Type.GameReady:
                    {
                        OnGameReadyEvent(new GameReadyEventArgs(receivedEvent.Data as Path));
                    }
                    break;

                // Area Update
                case GameEvent.Type.GameFinish:
                    {
                        OnGameFinishEvent(new GameFinishEventArgs((bool)receivedEvent.Data));
                    }
                    break;

                // Set all Areas back
                case GameEvent.Type.GameStart:
                    {
                        OnGameStartEvent(new GameStartEventArgs());
                    }
                    break;

                // Game Reset
                case GameEvent.Type.GameReset:
                    {
                        OnGameResetEvent(new GameResetEventArgs());
                    }
                    break;

                // Set all Areas back
                case GameEvent.Type.PlayerFinish:
                    {
                        OnPlayerFinishEvent(new PlayerFinishEvent((double)receivedEvent.Data));
                    }
                    break;
            }
        }

        /// <summary>
        /// Wait for incoming events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="doWorkEventArgs"></param>
        private void WaitForReceivingData(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            while (true)
            {
                try
                {
                    this.GameClient.Receive();
                    if (!eventBackgroundWorker.CancellationPending) continue;

                    doWorkEventArgs.Cancel = true;
                    return;
                }
                catch (Exception ex)
                {
                    this.OnErrorEvent(new ErrorEventArgs(ex.Message));
                }
            }
        }

        /// <summary>
        /// Send event to game Helper Utility
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public void SendEvent(GameEvent.Type type, object data)
        {
            if (GameClient != null && GameClient.Connected)
            {
                GameClient.Send(new GameEvent(type, data));
            }
        }

        #region EventHandlers

        protected virtual void OnPlayerJoinedEvent(PlayerJoinedEventArgs e)
        {
            if(PlayerJoinedEvent != null) PlayerJoinedEvent.Invoke(this, e);
        }

        protected virtual void OnErrorEvent(ErrorEventArgs e)
        {
            if (ErrorEvent != null) ErrorEvent.Invoke(this, e);
        }

        protected virtual void OnPlayerLostEvent(PlayerLostEventArgs e)
        {
            if (PlayerLostEvent != null) PlayerLostEvent.Invoke(this, e);
        }

        protected virtual void OnPlayerKickedEvent(PlayerKickedEventArgs e)
        {
            if (PlayerKickedEvent != null) PlayerKickedEvent.Invoke(this, e);
        }

        protected virtual void OnGameFinishEvent(GameFinishEventArgs e)
        {
            if (GameFinishEvent != null) GameFinishEvent.Invoke(this, e);
        }

        protected virtual void OnAreaUpdateEvent(AreaUpdateEventArgs e)
        {
            if (AreaUpdateEvent != null) AreaUpdateEvent.Invoke(this, e);
        }

        protected virtual void OnPlayerLeftEvent(PlayerLeftEventArgs e)
        {
            if (PlayerLeftEvent != null) PlayerLeftEvent.Invoke(this, e);
        }

        protected virtual void OnGameReadyEvent(GameReadyEventArgs e)
        {
            if (GameReadyEvent != null) GameReadyEvent.Invoke(this, e);
        }

        protected virtual void OnGameStartEvent(GameStartEventArgs e)
        {
            if (GameStartEvent != null) GameStartEvent.Invoke(this, e);
        }

        protected virtual void OnGameResetEvent(GameResetEventArgs e)
        {
            if (GameResetEvent != null) GameResetEvent.Invoke(this, e);
        }

        protected virtual void OnPlayerFinishEvent(PlayerFinishEvent e)
        {
            if (PlayerFinishEvent != null) PlayerFinishEvent.Invoke(this, e);
        }

        #endregion
    }
}