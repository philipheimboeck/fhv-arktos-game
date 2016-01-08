using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using Arctos.Game.Middleware.Logic.Model.Model;

namespace ArctosGameServer.Service
{
    internal class ClientRequestHandler
    {
        protected TcpClient _clientSocket;
        protected Guid _id;
        protected NetworkStream _networkStream = null;
        protected GameTcpServer _tcpServer = null;

        public ClientRequestHandler(Guid id, TcpClient clientConnected, GameTcpServer server)
        {
            this._id = id;
            this._clientSocket = clientConnected;
            this._tcpServer = server;
        }

        public void StartClient()
        {
            _networkStream = _clientSocket.GetStream();

            WaitForRequest();
        }

        private void WaitForRequest()
        {
            var buffer = new byte[_clientSocket.ReceiveBufferSize];

            _networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        private void ReadCallback(IAsyncResult result)
        {
            try
            {
                // Start of Transport Layer
                var networkStream = _clientSocket.GetStream();

                var read = networkStream.EndRead(result);
                if (read == 0)
                {
                    Close();
                    return;
                }

                var buffer = result.AsyncState as byte[];
                var data = Encoding.Default.GetString(buffer, 0, read);

                // Start of Session layer

                // Start of Presentation Layer
                var receivedPdus = data.Split(new string[] {"<?xml"}, StringSplitOptions.RemoveEmptyEntries);
                var serializer = new XmlSerializer(typeof (GameEvent));

                foreach (var pdu in receivedPdus)
                {
                    // Add removed separator
                    var xmlString = "<?xml" + pdu;

                    using (TextReader tr = new StringReader(xmlString))
                    {
                        // Start of Application Layer

                        // Deserialize entity
                        var gameEvent = (GameEvent) serializer.Deserialize(tr);

                        // Notify Server about the event
                        _tcpServer.OnReceived(_id, gameEvent);
                    }
                }
            }
            catch (IOException ex)
            {
                Close();
                return;
            }
           

            this.WaitForRequest();
        }

        private void Close()
        {
            _networkStream.Close();
            _clientSocket.Close();

            // Todo: Notify about the lost connection
        }
    }
}