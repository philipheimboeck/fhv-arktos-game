using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Build.Framework;

namespace ArctosGameServer.Service
{
    public class GameGuiService
    {
        private TcpListener tcpListener;

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

                Task.Factory.StartNew(() =>
                {
                    this.tcpListener.Start();
                    while (true)
                    {
                        TcpClient client = this.tcpListener.AcceptTcpClient();
                        NetworkStream stream = client.GetStream();

                        // send game information
                        Byte[] data = System.Text.Encoding.ASCII.GetBytes("{GameConfig:SetActive-1}");
                        stream.Write(data, 0, data.Length);

                        // Shutdown and end connection
                        client.Close();
                    }
                });
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}