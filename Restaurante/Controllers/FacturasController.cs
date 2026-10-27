using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Restaurante.Data;
using Restaurante.Models;
using Restaurante.Services;
using Restaurante.ViewModels;
namespace Restaurante.Controllers;
public class FacturasController(RestauranteDbContext db, IConfiguration config, ILogger<FacturasController> logger) : Controller
{
    private decimal? Tasa => config.GetValue<decimal?>("Facturacion:TasaImpuesto");
    private IQueryable<Pedido> PedidosCompletos => db.Pedidos.Include(p => p.Cliente).Include(p => p.Empleado)
        .Include(p => p.Detalles).ThenInclude(d => d.Producto);

    public async Task<IActionResult> Index(FacturasIndexViewModel model)
    {
        if (model.Estado.HasValue && !Enum.IsDefined(model.Estado.Value)) return BadRequest();
        var query = db.Facturas.AsNoTracking().Include(f => f.Pedido).ThenInclude(p => p.Cliente).AsQueryable();
        model.TotalFacturado = await db.Facturas.Where(f => f.Estado != EstadoFactura.Anulada).SumAsync(f => (decimal?)f.Total) ?? 0;
        model.Pagadas = await db.Facturas.CountAsync(f => f.Estado == EstadoFactura.Pagada);
        model.Pendientes = await db.Facturas.CountAsync(f => f.Estado == EstadoFactura.Pendiente);
        model.Anuladas = await db.Facturas.CountAsync(f => f.Estado == EstadoFactura.Anulada);
        model.Buscar = model.Buscar?.Trim();
        if (!string.IsNullOrEmpty(model.Buscar))
        {
            var texto = model.Buscar;
            int.TryParse(texto.TrimStart('#'), out var numero);
            query = query.Where(f => f.ID_Factura == numero || f.ID_Pedido == numero ||
                (f.Pedido.Cliente != null && (f.Pedido.Cliente.Nombre + " " + f.Pedido.Cliente.Apellido).Contains(texto)));
        }
        if (model.Desde.HasValue) query = query.Where(f => f.Fecha >= model.Desde.Value.Date);
        if (model.Hasta.HasValue && model.Hasta.Value.Date < DateTime.MaxValue.Date)
        {
            var fin = model.Hasta.Value.Date.AddDays(1);
            query = query.Where(f => f.Fecha < fin);
        }
        if (model.Desde > model.Hasta) ModelState.AddModelError("", "La fecha inicial no puede superar la final.");
        if (model.Estado.HasValue) query = query.Where(f => f.Estado == model.Estado);
        model.Cantidad = await query.CountAsync();
        model.Paginas = Math.Max(1, (int)Math.Ceiling(model.Cantidad / 10d));
        model.Pagina = Math.Clamp(model.Pagina, 1, model.Paginas);
        model.Facturas = await query.OrderByDescending(f => f.Fecha).ThenByDescending(f => f.ID_Factura)
            .Skip((model.Pagina - 1) * 10).Take(10).ToListAsync();
        return View(model);
    }

    public async Task<IActionResult> Details(int id, bool imprimir = false)
    {
        var factura = await db.Facturas.AsNoTracking().SingleOrDefaultAsync(f => f.ID_Factura == id);
        if (factura == null) return NotFound();
        ViewBag.Imprimir = imprimir;
        return View(new FacturaDetalleViewModel(factura, JsonSerializer.Deserialize<DatosFactura>(factura.DatosPedidoJson)!));
    }
    public async Task<IActionResult> PorPedido(int id)
    {
        var factura = await db.Facturas.AsNoTracking().SingleOrDefaultAsync(f => f.ID_Pedido == id);
        return factura == null ? RedirectToAction(nameof(Create), new { pedidoId = id }) : RedirectToAction(nameof(Details), new { id = factura.ID_Factura });
    }
    [HttpGet]
    public async Task<IActionResult> Create(int? pedidoId)
    {
        var model = new NuevaFacturaViewModel { ID_Pedido = pedidoId ?? 0 };
        await Preparar(model);
        return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NuevaFacturaViewModel model)
    {
        if (!ModelState.IsValid) { await Preparar(model); return View(model); }
        try
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var pedido = await PedidosCompletos.SingleOrDefaultAsync(p => p.ID_Pedido == model.ID_Pedido);
            if (pedido == null) throw new InvalidOperationException("El pedido no existe.");
            if (pedido.Estado == EstadoPedido.Cancelado) throw new InvalidOperationException("No se puede facturar un pedido cancelado.");
            if (await db.Facturas.AnyAsync(f => f.ID_Pedido == pedido.ID_Pedido))
                throw new InvalidOperationException("Este pedido ya tiene una factura, incluso si fue anulada.");
            if (!Tasa.HasValue) throw new InvalidOperationException("Configura Facturacion:TasaImpuesto antes de emitir facturas.");
            var importes = CalculoFactura.Calcular(pedido.Detalles, Tasa.Value);
            var datos = new DatosFactura(pedido.Cliente?.NombreCompleto ?? "Sin cliente asociado",
                pedido.Empleado == null ? "Sin empleado asociado" : pedido.Empleado.Nombre + " " + pedido.Empleado.Apellido,
                pedido.Detalles.Select(d => new LineaFactura(d.Producto?.Nombre ?? "Producto", d.Cantidad, d.PrecioUnitario,
                    decimal.Round(d.Subtotal, 2, MidpointRounding.AwayFromZero))).ToList());
            var factura = new Factura { ID_Pedido = pedido.ID_Pedido, Fecha = DateTime.Now,
                Subtotal = importes.Subtotal, Impuestos = importes.Impuestos, Total = importes.Total,
                MetodoPago = model.MetodoPago!.Value, Estado = EstadoFactura.Pendiente,
                DatosPedidoJson = JsonSerializer.Serialize(datos) };
            db.Facturas.Add(factura);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            TempData["Mensaje"] = "Factura generada correctamente.";
            return RedirectToAction(nameof(Details), new { id = factura.ID_Factura });
        }
        catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
        { ModelState.AddModelError("", "El pedido ya fue facturado. Actualiza el listado."); }
        catch (SqlException ex) when (ex.Number == 1205)
        { ModelState.AddModelError("", "Otra operación modificó el pedido. Vuelve a intentarlo."); }
        db.ChangeTracker.Clear();
        await Preparar(model);
        return View(model);
    }
    private async Task Preparar(NuevaFacturaViewModel model)
    {
        model.Tasa = Tasa;
        var disponibles = await db.Pedidos.AsNoTracking().Include(p => p.Cliente)
            .Where(p => p.Estado != EstadoPedido.Cancelado && p.Detalles.Any() && !db.Facturas.Any(f => f.ID_Pedido == p.ID_Pedido))
            .OrderByDescending(p => p.Fecha).ToListAsync();
        model.Pedidos = disponibles.Select(p => new SelectListItem($"Pedido #{p.ID_Pedido} · {p.Cliente?.NombreCompleto ?? "Sin cliente"}", p.ID_Pedido.ToString())).ToList();
        if (model.ID_Pedido > 0)
        {
            if (!disponibles.Any(p => p.ID_Pedido == model.ID_Pedido))
                ModelState.AddModelError("", "El pedido no está disponible para facturar.");
            else model.Pedido = await PedidosCompletos.AsNoTracking().SingleAsync(p => p.ID_Pedido == model.ID_Pedido);
        }
        if (model.Pedido != null)
        {
            try
            {
                var valores = CalculoFactura.Calcular(model.Pedido.Detalles, model.Tasa ?? 0);
                model.Subtotal = valores.Subtotal; model.Impuestos = valores.Impuestos; model.Total = valores.Total;
            }
            catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); }
        }
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Pagar(int id)
    {
        var cambiadas = await db.Facturas.Where(f => f.ID_Factura == id && f.Estado == EstadoFactura.Pendiente)
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.Estado, EstadoFactura.Pagada));
        TempData[cambiadas == 1 ? "Mensaje" : "Error"] = cambiadas == 1 ? "Factura marcada como pagada." : "Solo se pueden pagar facturas pendientes.";
        if (cambiadas == 1) logger.LogInformation("Factura {Id} marcada como pagada", id);
        return RedirectToAction(nameof(Details), new { id });
    }
    [HttpGet]
    public async Task<IActionResult> Anular(int id)
    {
        var factura = await db.Facturas.AsNoTracking().SingleOrDefaultAsync(f => f.ID_Factura == id);
        if (factura == null) return NotFound();
        return View(factura);
    }
    [HttpPost, ActionName("Anular"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AnularConfirmado(int id)
    {
        var cambiadas = await db.Facturas.Where(f => f.ID_Factura == id && f.Estado == EstadoFactura.Pendiente)
            .ExecuteUpdateAsync(s => s.SetProperty(f => f.Estado, EstadoFactura.Anulada));
        TempData[cambiadas == 1 ? "Mensaje" : "Error"] = cambiadas == 1 ? "Factura anulada. Se conserva su historial y el pedido no se modifica." : "Solo se pueden anular facturas pendientes. Una pagada requiere un procedimiento autorizado.";
        if (cambiadas == 1) logger.LogInformation("Factura {Id} anulada", id);
        return RedirectToAction(nameof(Details), new { id });
    }
}
