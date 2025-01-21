using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UI_v1
{
    public partial class ConfirmWindow : Window
    {
        public event Action<bool> UserChoice;

        public ConfirmWindow()
        {
            InitializeComponent();
            this.Background = new SolidColorBrush(Color.FromRgb(200, 230, 255));
        }

        private void ChooseA_Click(object sender, RoutedEventArgs e)
        {
            UserChoice?.Invoke(true);
            this.Close();
        }

        private void ChooseB_Click(object sender, RoutedEventArgs e)
        {
            UserChoice?.Invoke(false);
            this.Close();
        }
    }
}
