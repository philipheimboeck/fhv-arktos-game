using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ArctosGameServer.Service
{
    public class DiscoveryService
    {
        private UdpClient client;
        private int _port;

        public DiscoveryService() : this(12000)
        {
        }

        public DiscoveryService(int port)
        {
            client = new UdpClient(port);
            _port = port;
        }

        public void Start()
        {
            client.BeginReceive(DataReceived, null);
        }

        private void DataReceived(IAsyncResult result)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, _port);
            client.EndReceive(result, ref ipEndPoint);

            // Send IP address
            var data = GetData();
            client.Send(data, data.Length, ipEndPoint);

            // Start again
            Start();
        }

        private byte[] GetData()
        {
            return Encoding.ASCII.GetBytes(GameTcpServer.FindIp().ToString());
        }
    }
}