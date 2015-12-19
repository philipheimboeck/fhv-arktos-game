using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace ArctosGameServer.Service
{
    public class GameGuiService
    {
        private TcpListener tcpListener;

        public GameGuiService()
        {
            
        }
        
        /// <summary>
        /// Start the GameGui Service
        /// </summary>
        public void StartService()
        {
            this.tcpListener = null;

            try
            {
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                this.tcpListener = new TcpListener(localAddr, port);

                this.tcpListener.Start();
                String data = null;
                Byte[] bytes = new Byte[256];

                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // blocking call...
                    TcpClient client = this.tcpListener.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;
                    NetworkStream stream = client.GetStream();

                    int i;
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);
                    }
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}