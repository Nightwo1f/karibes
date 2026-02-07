using System.Windows;
using System.Windows.Controls;
using Karibes.App.Models;
using Karibes.App.ViewModels;

namespace Karibes.App.Views
{
    /// <summary>
    /// Interaction logic for VendasView.xaml
    /// </summary>
    public partial class VendasView : UserControl
    {
        public VendasView()
        {
            InitializeComponent();
        }

        private void RemoverItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ItemVenda item)
            {
                if (DataContext is VendasViewModel viewModel)
                {
                    viewModel.RemoverItemCarrinhoCommand.Execute(item);
                }
            }
        }
    }
}
