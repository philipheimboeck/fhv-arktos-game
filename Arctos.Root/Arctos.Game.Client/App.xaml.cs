using System.Windows;
using Arctos.View;

namespace Arctos.Game.GUIClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var connectWindow = new ConnectView {DataContext = new ConnectViewModel()};
            connectWindow.Show();
        }
    }
}