namespace Restaurante.ViewModels
{
    public class ReporteProductosViewModel
    {
        public FiltroReporteViewModel Filtro { get; set; } = new();

        public int UnidadesVendidas { get; set; }

        public decimal TotalVendido { get; set; }

        public List<ProductoVendidoViewModel> Productos { get; set; } = new();
    }

    public class ProductoVendidoViewModel
    {
        public int ID_Producto { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal Total { get; set; }
    }
}
