using System.IO;
using System.Net.Sockets;
using ArctosGameServer.Communication;

namespace Arctos.Game.Middleware.Logic.Model.Client
{
    /// <summary>
    /// Actual implementation of the tcp client
    /// </summary>
    public class TcpCommunicator : ITcpCommunicator
    {
        private TcpClient _client;

        public TcpCommunicator(string host, int port)
        {
            _client = new TcpClient();
            _client.Connect(host, port);
        }

        public TcpCommunicator(TcpClient client)
        {
            _client = client;
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