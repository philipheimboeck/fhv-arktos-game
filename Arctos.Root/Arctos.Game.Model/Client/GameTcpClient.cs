using ArctosGameServer.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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

        public void Send(GameEvent gameEvent)
        {
            var serverStream = _client.GetStream();
            var serializer = new XmlSerializer(typeof(GameEvent));

            serializer.Serialize(serverStream, gameEvent);

            serverStream.Flush();
        }

        public GameEvent Receive()
        {
            StringBuilder message = new StringBuilder();
            NetworkStream serverStream = _client.GetStream();
            serverStream.ReadTimeout = 100;

            while (true)
            {
                if (serverStream.DataAvailable)
                {
                    int read = serverStream.ReadByte();
                    if (read > 0)
                    {
                        message.Append((char)read);
                    } else
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
            string xml = message.ToString();

            var serializer = new XmlSerializer(typeof(GameEvent));
            using (TextReader tr = new StringReader(xml))
            {
                // Deserialize entity
                GameEvent gameEvent = (GameEvent)serializer.Deserialize(tr);
                return gameEvent;
            }
        }

        public void Close()
        {
            _client.Close();
        }
    }
}
