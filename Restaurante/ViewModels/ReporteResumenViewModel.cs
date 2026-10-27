namespace Restaurante.ViewModels
{
    public class ReporteResumenViewModel
    {
        public FiltroReporteViewModel Filtro { get; set; } = new();

        public decimal TotalVentas { get; set; }

        public int PedidosEntregados { get; set; }

        public int PedidosRegistrados { get; set; }

        public int UnidadesVendidas { get; set; }

        public decimal TicketPromedio { get; set; }
    }
}
