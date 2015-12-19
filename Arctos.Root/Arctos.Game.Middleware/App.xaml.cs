using System.Windows;
using ArctosGameServer.Communication;
using ArctosGameServer.Communication.Protocol;
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
            GameGuiService guiService = new GameGuiService();
            guiService.StartService();
        }

    }
}
