using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Arctos.Game.Middleware.Logic.Model.Client
{
    public class DiscoveryServiceClient
    {
        /// <summary>
        /// Send a broadcast to find a server
        /// </summary>
        /// <param name="port"></param>
        public string Discover(int port)
        {
            try
            {
                var ip = new IPEndPoint(IPAddress.Broadcast, port);
                var client = new UdpClient();
                client.Client.SendTimeout = 5000;
                client.Client.ReceiveTimeout = 5000;

                var data = Encoding.ASCII.GetBytes("Searching for Server");

                // Send broadcast
                client.Send(data, data.Length, ip);

                // Get response
                var receiveEndpoint = new IPEndPoint(IPAddress.Any, port);

                var answer = client.Receive(ref receiveEndpoint);

                // Get the IP
                var ipAddress = receiveEndpoint.Address.ToString();

                // Close the client
                client.Close();

                return ipAddress;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string Discover()
        {
            return Discover(12000);
        }
    }
}