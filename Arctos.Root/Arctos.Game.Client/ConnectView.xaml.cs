using System.ComponentModel;
using System.Windows;
using Arctos.Game.GUIClient;

namespace Arctos
{
    /// <summary>
    /// Interaction logic for ConnectView.xaml
    /// </summary>
    public partial class ConnectView : Window
    {
        public ConnectView()
        {
            InitializeComponent();
        }

        private void ConnectView_OnClosing(object sender, CancelEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
