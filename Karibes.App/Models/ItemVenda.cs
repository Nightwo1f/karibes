using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Karibes.App.Models
{
    public class ItemVenda : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int VendaId { get; set; }
        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }

        private int _quantidade;
        public int Quantidade
        {
            get => _quantidade;
            set
            {
                if (_quantidade != value)
                {
                    _quantidade = value;
                    OnPropertyChanged();
                    AtualizarTotal();
                }
            }
        }

        private decimal _precoUnitario;
        public decimal PrecoUnitario
        {
            get => _precoUnitario;
            set
            {
                if (_precoUnitario != value)
                {
                    _precoUnitario = value;
                    OnPropertyChanged();
                    AtualizarTotal();
                }
            }
        }

        private decimal _desconto;
        public decimal Desconto
        {
            get => _desconto;
            set
            {
                if (_desconto != value)
                {
                    _desconto = value;
                    OnPropertyChanged();
                    AtualizarTotal();
                }
            }
        }

        private decimal _valorTotal;
        public decimal ValorTotal
        {
            get => _valorTotal;
            private set
            {
                if (_valorTotal != value)
                {
                    _valorTotal = value;
                    OnPropertyChanged();
                }
            }
        }

        private void AtualizarTotal()
        {
            ValorTotal = (PrecoUnitario * Quantidade) - Desconto;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
