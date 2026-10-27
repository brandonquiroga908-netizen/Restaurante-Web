using Restaurante.Models;

namespace Restaurante.ViewModels
{
    public class ReportePedidosViewModel
    {
        public FiltroReporteViewModel Filtro { get; set; } = new();

        public int TotalPedidos { get; set; }

        public decimal TotalRegistrado { get; set; }

        public List<EstadoPedidoConteoViewModel> PorEstado { get; set; } = new();

        public List<Pedido> Pedidos { get; set; } = new();
    }

    public class EstadoPedidoConteoViewModel
    {
        public EstadoPedido Estado { get; set; }

        public int Cantidad { get; set; }

        public decimal Total { get; set; }
    }
}
