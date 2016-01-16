using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Arctos.Game.Middleware.Logic.Model.Model;
using ArctosGameServer.Communication;
using ArctosGameServer.Communication.ServerProtocol;

namespace Arctos.Game.Middleware.Logic.Model.Client
{
    public class GameTcpClient
    {
        private ProtocolLayer _protocol;
        private TcpCommunicator _client;
        public event ReceivedEvent ReceivedDataEvent;

        public GameTcpClient(string host, int port)
        {
            _client = new TcpCommunicator(host, port);

            _protocol = new PresentationLayer(
               new SessionLayer(
                   new SlipTransportLayer(_client))
               );
        }

        public GameTcpClient(string host) : this(host, 13000)
        {
        }

  
        public bool Connected
        {
            get { return this._client.Connected; }
        }

        public void Send(GameEvent gameEvent)
        {
            _protocol.send(new PDU<object>() {data = gameEvent});
        }

        public void Receive()
        {
            var received = _protocol.receive();
            if (received != null)
            {
                var data = received.data as GameEvent;
                if (data != null)
                {
                    this.OnReceivedEvent(new ReceivedEventArgs { Data = data });
                }
            }
        }

        /// <summary>
        /// Received data from TCP Connection
        /// </summary>
        /// <param name="e"></param>
        private void OnReceivedEvent(ReceivedEventArgs e)
        {
            if (this.ReceivedDataEvent != null) this.ReceivedDataEvent(this, e);
        }

        public void Close()
        {
            _client.Close();
        }
    }
}