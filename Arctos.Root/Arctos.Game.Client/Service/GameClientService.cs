using System;
using System.Net.Sockets;

namespace Arctos.Game.Client.Service
{
    public class GameClientService
    {
        public void Connect(String server, String message)
        {
            try
            {
                var port = 13000;
                var client = new TcpClient(server, port);

                var data = System.Text.Encoding.ASCII.GetBytes(message);
                var stream = client.GetStream();

                data = new Byte[256];

                var responseData = String.Empty;

                var bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }
    }
}