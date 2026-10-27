using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.Data;
using Restaurante.Models;
using Restaurante.ViewModels;

namespace Restaurante.Controllers
{
    public class ReportesController : Controller
    {
        private readonly RestauranteDbContext _context;

        public ReportesController(RestauranteDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta)
        {
            var rango = ResolverRango(desde, hasta);
            var pedidos = PedidosDelPeriodo(rango);
            var entregados = pedidos.Where(p => p.Estado == EstadoPedido.Entregado);

            var totalVentas = await entregados.SumAsync(p => (decimal?)p.Total) ?? 0m;
            var pedidosEntregados = await entregados.CountAsync();

            var modelo = new ReporteResumenViewModel
            {
                Filtro = CrearFiltro(rango),
                TotalVentas = totalVentas,
                PedidosEntregados = pedidosEntregados,
                PedidosRegistrados = await pedidos.CountAsync(),
                UnidadesVendidas = await ContarUnidadesVendidas(rango),
                TicketPromedio = pedidosEntregados == 0 ? 0 : totalVentas / pedidosEntregados
            };

            return View(modelo);
        }

        public async Task<IActionResult> Ventas(DateTime? desde, DateTime? hasta)
        {
            var rango = ResolverRango(desde, hasta);
            var entregados = PedidosDelPeriodo(rango)
                .Where(p => p.Estado == EstadoPedido.Entregado);

            var porDia = await entregados
                .GroupBy(p => p.Fecha.Date)
                .Select(g => new VentaPorDiaViewModel
                {
                    Fecha = g.Key,
                    Pedidos = g.Count(),
                    Total = g.Sum(p => p.Total)
                })
                .OrderByDescending(x => x.Fecha)
                .ToListAsync();

            var totalVentas = porDia.Sum(x => x.Total);
            var pedidosEntregados = porDia.Sum(x => x.Pedidos);

            var modelo = new ReporteVentasViewModel
            {
                Filtro = CrearFiltro(rango),
                TotalVentas = totalVentas,
                PedidosEntregados = pedidosEntregados,
                TicketPromedio = pedidosEntregados == 0 ? 0 : totalVentas / pedidosEntregados,
                PorDia = porDia
            };

            return View(modelo);
        }

        public async Task<IActionResult> Pedidos(DateTime? desde, DateTime? hasta, EstadoPedido? estado)
        {
            var rango = ResolverRango(desde, hasta);

            var porEstado = await PedidosDelPeriodo(rango)
                .GroupBy(p => p.Estado)
                .Select(g => new EstadoPedidoConteoViewModel
                {
                    Estado = g.Key,
                    Cantidad = g.Count(),
                    Total = g.Sum(p => p.Total)
                })
                .ToListAsync();

            var consulta = _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                .Where(p => p.Fecha >= rango.Desde && p.Fecha < rango.HastaExclusivo);

            if (estado.HasValue)
            {
                consulta = consulta.Where(p => p.Estado == estado.Value);
            }

            var pedidos = await consulta
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            var modelo = new ReportePedidosViewModel
            {
                Filtro = CrearFiltro(rango, mostrarEstado: true, estado),
                TotalPedidos = pedidos.Count,
                TotalRegistrado = pedidos.Sum(p => p.Total),
                PorEstado = porEstado
                    .OrderBy(x => x.Estado)
                    .ToList(),
                Pedidos = pedidos
            };

            return View(modelo);
        }

        public async Task<IActionResult> Productos(DateTime? desde, DateTime? hasta)
        {
            var rango = ResolverRango(desde, hasta);

            var productos = await (
                from detalle in _context.DetallesPedido
                join producto in _context.Productos on detalle.ID_Producto equals producto.ID_Producto
                join categoria in _context.Categorias on producto.ID_Categoria equals categoria.ID_Categoria
                where detalle.Pedido!.Estado != EstadoPedido.Cancelado
                    && detalle.Pedido.Fecha >= rango.Desde
                    && detalle.Pedido.Fecha < rango.HastaExclusivo
                group detalle by new
                {
                    producto.ID_Producto,
                    producto.Nombre,
                    Categoria = categoria.Nombre
                }
                into grupo
                select new ProductoVendidoViewModel
                {
                    ID_Producto = grupo.Key.ID_Producto,
                    Nombre = grupo.Key.Nombre,
                    Categoria = grupo.Key.Categoria,
                    Cantidad = grupo.Sum(d => d.Cantidad),
                    Total = grupo.Sum(d => d.Cantidad * d.PrecioUnitario)
                })
                .OrderByDescending(x => x.Cantidad)
                .ThenBy(x => x.Nombre)
                .ToListAsync();

            var modelo = new ReporteProductosViewModel
            {
                Filtro = CrearFiltro(rango),
                UnidadesVendidas = productos.Sum(p => p.Cantidad),
                TotalVendido = productos.Sum(p => p.Total),
                Productos = productos
            };

            return View(modelo);
        }

        private IQueryable<Pedido> PedidosDelPeriodo(RangoReporte rango)
        {
            return _context.Pedidos.Where(p =>
                p.Fecha >= rango.Desde && p.Fecha < rango.HastaExclusivo);
        }

        private async Task<int> ContarUnidadesVendidas(RangoReporte rango)
        {
            return await _context.DetallesPedido
                .Where(d => d.Pedido!.Estado != EstadoPedido.Cancelado
                    && d.Pedido.Fecha >= rango.Desde
                    && d.Pedido.Fecha < rango.HastaExclusivo)
                .SumAsync(d => (int?)d.Cantidad) ?? 0;
        }

        private static FiltroReporteViewModel CrearFiltro(
            RangoReporte rango,
            bool mostrarEstado = false,
            EstadoPedido? estado = null)
        {
            return new FiltroReporteViewModel
            {
                Desde = rango.Desde,
                Hasta = rango.Hasta,
                MostrarEstado = mostrarEstado,
                Estado = estado
            };
        }

        private static RangoReporte ResolverRango(DateTime? desde, DateTime? hasta)
        {
            var hoy = DateTime.Today;
            var inicio = (desde ?? new DateTime(hoy.Year, hoy.Month, 1)).Date;
            var fin = (hasta ?? hoy).Date;

            if (fin < inicio)
            {
                fin = inicio;
            }

            return new RangoReporte(inicio, fin, fin.AddDays(1));
        }

        private readonly record struct RangoReporte(DateTime Desde, DateTime Hasta, DateTime HastaExclusivo);
    }
}
