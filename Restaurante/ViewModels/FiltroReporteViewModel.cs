using Restaurante.Models;

namespace Restaurante.ViewModels
{
    public class FiltroReporteViewModel
    {
        public DateTime Desde { get; set; }

        public DateTime Hasta { get; set; }

        public bool MostrarEstado { get; set; }

        public EstadoPedido? Estado { get; set; }
    }
}
