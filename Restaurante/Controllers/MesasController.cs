using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.Data;
using Restaurante.Models;

namespace Restaurante.Controllers
{
    public class MesasController : Controller
    {
        private readonly RestauranteDbContext _context;

        public MesasController(RestauranteDbContext context)
        {
            _context = context;
        }

        // Listar mesas como tarjetas
        public async Task<IActionResult> Index()
        {
            var mesas = await _context.Mesas
                .OrderBy(m => m.Numero)
                .ToListAsync();

            return View(mesas);
        }

        // Mostrar formulario de creación
        public IActionResult Create()
        {
            return View(new Mesa());
        }

        // Guardar mesa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mesa mesa)
        {
            if (await _context.Mesas.AnyAsync(m => m.Numero == mesa.Numero))
            {
                ModelState.AddModelError(nameof(Mesa.Numero),
                    "Ya existe una mesa con ese número.");
            }

            if (!ModelState.IsValid)
                return View(mesa);

            _context.Mesas.Add(mesa);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Mesa {mesa.Numero} creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // Mostrar formulario de edición
        public async Task<IActionResult> Edit(int id)
        {
            var mesa = await _context.Mesas.FindAsync(id);

            if (mesa == null)
                return NotFound();

            return View(mesa);
        }

        // Guardar cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Mesa mesa)
        {
            if (id != mesa.ID_Mesa)
                return NotFound();

            await using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            if (!await _context.Mesas.AnyAsync(m => m.ID_Mesa == id))
                return NotFound();

            if (!Enum.IsDefined(mesa.Estado))
                ModelState.AddModelError(nameof(Mesa.Estado), "El estado de la mesa no es válido.");
            else if (mesa.Estado != EstadoMesa.Ocupada && await TienePedidosActivos(id))
                ModelState.AddModelError(nameof(Mesa.Estado), "La mesa tiene pedidos pendientes o en preparación y debe permanecer ocupada.");

            if (await _context.Mesas.AnyAsync(
                m => m.Numero == mesa.Numero && m.ID_Mesa != mesa.ID_Mesa))
            {
                ModelState.AddModelError(nameof(Mesa.Numero),
                    "Ya existe una mesa con ese número.");
            }

            if (!ModelState.IsValid)
                return View(mesa);

            _context.Update(mesa);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Mensaje"] = $"Mesa {mesa.Numero} actualizada.";
            return RedirectToAction(nameof(Index));
        }

        // Cambiar estado rápido desde la grilla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, EstadoMesa estado)
        {
            if (!ModelState.IsValid || !Enum.IsDefined(estado))
            {
                TempData["Error"] = "El estado de la mesa no es válido.";
                return RedirectToAction(nameof(Index));
            }

            await using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var mesa = await _context.Mesas.FindAsync(id);

            if (mesa == null)
                return NotFound();

            if (estado != EstadoMesa.Ocupada && await TienePedidosActivos(id))
            {
                TempData["Error"] = "La mesa tiene pedidos pendientes o en preparación y debe permanecer ocupada.";
                return RedirectToAction(nameof(Index));
            }

            mesa.Estado = estado;
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        private Task<bool> TienePedidosActivos(int id) => _context.Pedidos.AnyAsync(p =>
            p.ID_Mesa == id &&
            (p.Estado == EstadoPedido.Pendiente || p.Estado == EstadoPedido.EnPreparacion));

        // Confirmar eliminación
        public async Task<IActionResult> Delete(int id)
        {
            var mesa = await _context.Mesas.FindAsync(id);

            if (mesa == null)
                return NotFound();

            ViewBag.TienePedidos = await _context.Pedidos
                .AnyAsync(p => p.ID_Mesa == id);

            return View(mesa);
        }

        // Eliminar mesa
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mesa = await _context.Mesas.FindAsync(id);

            if (mesa == null)
                return RedirectToAction(nameof(Index));

            if (await _context.Pedidos.AnyAsync(p => p.ID_Mesa == id))
            {
                TempData["Error"] =
                    $"La mesa {mesa.Numero} tiene pedidos registrados y no se puede eliminar.";
                return RedirectToAction(nameof(Index));
            }

            _context.Mesas.Remove(mesa);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Mesa {mesa.Numero} eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
