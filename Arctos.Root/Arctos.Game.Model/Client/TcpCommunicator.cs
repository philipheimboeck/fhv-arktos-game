using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
            if (!Connected)
            {
                throw new Exception("Client is not connected");
            }

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
            if (!Connected)
            {
                throw new Exception("Client is not connected");
            }

            var serverStream = _client.GetStream();
            var writer = new StreamWriter(serverStream);
            writer.Write(data);
            writer.Flush();
        
            serverStream.Flush();

            return true;
        }

        public bool Connected
        {
            get
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections().Where(x => x.LocalEndPoint.Equals(_client.Client.LocalEndPoint) && x.RemoteEndPoint.Equals(_client.Client.RemoteEndPoint)).ToArray();

                if (tcpConnections != null && tcpConnections.Length > 0)
                {
                    TcpState stateOfConnection = tcpConnections.First().State;
                    return (stateOfConnection == TcpState.Established);
                }
                return false;
            }
        }

        public void Close()
        {
            _client.Close();
        }
    }
}