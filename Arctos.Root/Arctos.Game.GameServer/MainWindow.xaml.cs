using System.Windows;

namespace ArctosGameServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private TheAlligator alligator = new TheAlligator("COM35");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (msgData.Text.Length > 0)
            {
                // alligator.Send(msgData.Text);
            }
        }
    }
}