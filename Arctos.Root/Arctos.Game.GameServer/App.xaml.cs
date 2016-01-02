using System.Windows;
using ArctosGameServer.Controller;
using ArctosGameServer.Service;

namespace ArctosGameServer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            GameGuiService guiService = new GameGuiService(System.Net.IPAddress.Parse("127.0.0.1"), 13000);
            guiService.StartService();
        }

    }
}
