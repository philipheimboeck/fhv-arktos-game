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
        private TcpCommunicatorClient _client;
        public event ReceivedEvent ReceivedDataEvent;

        public GameTcpClient(string host, int port)
        {
            _client = new TcpCommunicatorClient(host, port);

            _protocol = new PresentationLayer(
               new SessionLayer(
                   new TransportLayer(_client))
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

        /// <summary>
        /// Actual implementation of the tcp client
        /// </summary>
        private class TcpCommunicatorClient : ITcpCommunicator
        {
            private TcpClient _client;

            public TcpCommunicatorClient(string host, int port)
            {
                _client = new TcpClient();
                _client.Connect(host, port);
            }

            public char? Read()
            {
                var serverStream = _client.GetStream();

                if (serverStream.DataAvailable)
                {
                    var read = serverStream.ReadByte();
                    if (read > 0)
                    {
                        return (char)read;
                    }
                    
                }
                
                return null;
            }

            public bool Write(string data)
            {
                var serverStream = _client.GetStream();
                using (var writer = new StreamWriter(serverStream))
                {
                    writer.Write(data);
                }

                serverStream.Flush();

                return true;
            }

            public bool Connected
            {
                get { return _client.Connected; }
            }

            public void Close()
            {
                _client.Close();
            }
        }
    }
}