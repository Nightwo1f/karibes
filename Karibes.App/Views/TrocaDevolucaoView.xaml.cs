using System.Windows;
using System.Windows.Controls;
using Karibes.App.Models;
using Karibes.App.ViewModels;

namespace Karibes.App.Views
{
    /// <summary>
    /// Interaction logic for TrocaDevolucaoView.xaml
    /// </summary>
    public partial class TrocaDevolucaoView : UserControl
    {
        public TrocaDevolucaoView()
        {
            InitializeComponent();
        }

        private void RemoverItemNovo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ItemVenda item)
            {
                if (DataContext is TrocaDevolucaoViewModel viewModel)
                {
                    viewModel.RemoverItemNovoCommand.Execute(item);
                }
            }
        }

        private void DataGrid_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            if (e.EditingElement is System.Windows.Controls.TextBox textBox && 
                e.Row.DataContext is ItemDevolucaoViewModel itemDevolucao)
            {
                if (int.TryParse(textBox.Text, out int quantidade))
                {
                    itemDevolucao.QuantidadeDevolver = quantidade;
                    if (DataContext is TrocaDevolucaoViewModel viewModel)
                    {
                        viewModel.CalcularDiferenca();
                    }
                }
            }
        }
    }
}

