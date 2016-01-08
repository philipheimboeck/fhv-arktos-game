using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;
using Arctos.Game.Middleware.Logic.Model.Model;

namespace Arctos.Game.Middleware.Logic.Model.Client
{
    public class GameTcpClient
    {
        private TcpClient _client;

        public GameTcpClient(String host, Int32 port)
        {
            _client = new TcpClient();
            _client.Connect(host, port);
        }

        public GameTcpClient(String host)
        {
            _client = new TcpClient();
            _client.Connect(host, 13000);
        }

        public bool Connected
        {
            get { return this._client.Connected; }
        }

        public void Send<T>(GameEvent<T> gameEvent)
        {
            var serverStream = _client.GetStream();
            var serializer = new XmlSerializer(typeof (GameEvent<T>));

            serializer.Serialize(serverStream, gameEvent);

            serverStream.Flush();
        }

        public GameEvent<dynamic> Receive()
        {
            var message = new StringBuilder();
            var serverStream = _client.GetStream();
            serverStream.ReadTimeout = 100;

            while (true)
            {
                if (serverStream.DataAvailable)
                {
                    var read = serverStream.ReadByte();
                    if (read > 0)
                    {
                        message.Append((char) read);
                    }
                    else
                    {
                        // Nothing to read
                        break;
                    }
                }
                else if (message.ToString().Length > 0)
                {
                    // Completed reading
                    break;
                }
            }
            var xml = message.ToString();

            var serializer = new XmlSerializer(typeof (GameEvent<dynamic>));
            using (TextReader tr = new StringReader(xml))
            {
                // Deserialize entity
                var gameEvent = (GameEvent<dynamic>) serializer.Deserialize(tr);
                return gameEvent;
            }
        }

        public void Close()
        {
            _client.Close();
        }
    }
}