using ArctosGameServer.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ArctosGameServer.Service 
{
    class ClientRequestHandler
    {
        protected TcpClient _clientSocket;
        protected NetworkStream _networkStream = null;
        protected GameTcpServer _tcpServer = null;

        public ClientRequestHandler(TcpClient clientConnected, GameTcpServer server)
        {
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
            byte[] buffer = new byte[_clientSocket.ReceiveBufferSize];

            _networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        private void ReadCallback(IAsyncResult result)
        {
            NetworkStream networkStream = _clientSocket.GetStream();
            
            int read = networkStream.EndRead(result);
            if (read == 0)
            {
                _networkStream.Close();
                _clientSocket.Close();
                return;
            }

            byte[] buffer = result.AsyncState as byte[];
            string data = Encoding.Default.GetString(buffer, 0, read);

            var serializer = new XmlSerializer(typeof(GameEvent));
            using (TextReader tr = new StringReader(data))
            {
                // Deserialize entity
                GameEvent gameEvent = (GameEvent)serializer.Deserialize(tr);

                // Notify Server about the event
                _tcpServer.OnReceived(gameEvent);
            }

            this.WaitForRequest();
        }

    }
}
