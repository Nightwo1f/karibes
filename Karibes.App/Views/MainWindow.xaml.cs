using System.Windows;
using Karibes.App.ViewModels;

namespace Karibes.App.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}





