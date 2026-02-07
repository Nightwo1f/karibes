using Karibes.App.Models;
using Karibes.App.Services;

namespace Karibes.App.Services
{
    public class EstoqueService
    {
        private readonly ExcelService _excelService;

        public EstoqueService(ExcelService excelService)
        {
            _excelService = excelService;
        }

        public void RegistrarMovimento(MovimentoEstoque movimento)
        {
            // Implementar lógica de registro de movimento de estoque
        }

        public int ConsultarEstoque(int produtoId)
        {
            // Implementar lógica de consulta de estoque
            return 0;
        }

        public void AtualizarEstoque(int produtoId, int quantidade, string tipo)
        {
            // Implementar lógica de atualização de estoque
        }
    }
}





