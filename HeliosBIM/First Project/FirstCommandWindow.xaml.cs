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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HeliosBIM
{
    /// <summary>
    /// Interaction logic for FirstWindow.xaml
    /// </summary>
    public partial class FirstCommandWindow : Window
    {
        private FirstCommandViewModel _firstCommandViewModel;
        public FirstCommandWindow(FirstCommandViewModel firstCommandViewModel)
        {
            _firstCommandViewModel = firstCommandViewModel;
            DataContext = firstCommandViewModel;
            InitializeComponent();
        }

        private void ClickToCmd(object sender, RoutedEventArgs e)
        {
            Close();
            _firstCommandViewModel.MasterMethod();
        }

        private void ClickToDrawBtl(object sender, RoutedEventArgs e)
        {
            Close();
            _firstCommandViewModel.VeBeTongLot();
        }
    }
}
