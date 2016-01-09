using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Arctos.Game.Middleware.Logic.Model.Model;

namespace Arctos.Game.Middleware.Logic.Model.Client
{
    public class GameTcpClient
    {
        private TcpClient _client;
        public event ReceivedEvent ReceivedDataEvent;

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

        public void Send(GameEvent gameEvent)
        {
            var serverStream = _client.GetStream();
            var serializer = new XmlSerializer(typeof (GameEvent));

            serializer.Serialize(serverStream, gameEvent);

            serverStream.Flush();
        }

        public void Receive()
        {
            var message = new StringBuilder();
            var serverStream = _client.GetStream();

            int sleepRetries = 0;
            while (true)
            {
                if (serverStream.DataAvailable)
                {
                    var read = serverStream.ReadByte();
                    if (read > 0)
                    {
                        message.Append((char)read);
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
                    if (message.ToString().EndsWith("</GameEvent>") || sleepRetries > 5)
                    {
                        break;
                    }

                    // TODO: improve XML protocol in order to avoid incomplete data packets!
                    Thread.Sleep(1000);
                    sleepRetries++;
                }
            }

            var xml = message.ToString();
            if (!xml.EndsWith("</GameEvent>"))
            {
                throw new Exception("Did receive a invalid packet!", new Exception(xml));
            }

            // Start of Presentation Layer
            var receivedPdus = xml.Split(new string[] { "<?xml" }, StringSplitOptions.RemoveEmptyEntries);
            var serializer = new XmlSerializer(typeof(GameEvent));

            foreach (var pdu in receivedPdus)
            {
                // Add removed separator
                var xmlString = "<?xml" + pdu;

                using (TextReader tr = new StringReader(xmlString))
                {
                    var gameEvent = (GameEvent)serializer.Deserialize(tr);
                    this.OnReceivedEvent(new ReceivedEventArgs { Data = gameEvent });
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