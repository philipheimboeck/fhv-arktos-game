using Arctos.Game.MapGenerator.View;
using System.Windows;

namespace Arctos.Game.MapGenerator
{
    /// <summary>
    /// Interaction logic for ControlUnitView.xaml
    /// </summary>
    public partial class RobotView : Window
    {
        public RobotView()
        {
            this.DataContext = new RobotViewModel("COM41");
            InitializeComponent();

            LogBox.TextChanged += (sender, args) =>
            {
                LogBox.CaretIndex = LogBox.Text.Length;
                LogBox.ScrollToEnd();
            }; 
        }
    
        private void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);

        }

        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);
        }
    }
}