using System.Windows.Controls;
using System.Printing;
using System.Windows.Documents;

namespace Karibes.App.Services
{
    public class ImpressaoService
    {
        public void ImprimirDocumento(FlowDocument document)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "Documento");
            }
        }

        public void ImprimirDataGrid(DataGrid dataGrid)
        {
            // Implementar lógica de impressão de DataGrid
        }
    }
}





