namespace Restaurante.ViewModels
{
    public class ReporteVentasViewModel
    {
        public FiltroReporteViewModel Filtro { get; set; } = new();

        public decimal TotalVentas { get; set; }

        public int PedidosEntregados { get; set; }

        public decimal TicketPromedio { get; set; }

        public List<VentaPorDiaViewModel> PorDia { get; set; } = new();
    }

    public class VentaPorDiaViewModel
    {
        public DateTime Fecha { get; set; }

        public int Pedidos { get; set; }

        public decimal Total { get; set; }
    }
}
