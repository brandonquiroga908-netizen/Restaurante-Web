using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.Data;
using Restaurante.Models;

namespace Restaurante.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly RestauranteDbContext _context;

        public EmpleadosController(RestauranteDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? busqueda)
        {
            var consulta = _context.Empleados.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var termino = busqueda.Trim();
                consulta = consulta.Where(e => e.Nombre.Contains(termino) ||
                                               e.Apellido.Contains(termino) ||
                                               e.Cargo.Contains(termino));
            }
            ViewBag.Busqueda = busqueda;
            return View(await consulta.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var empleado = await _context.Empleados.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ID_Empleado == id);
            return empleado == null ? NotFound() : View(empleado);
        }

        public IActionResult Create() => View(new Empleado());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Apellido,Cargo,Telefono")] Empleado empleado)
        {
            if (!ModelState.IsValid) return View(empleado);
            Limpiar(empleado);
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Empleado registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            return empleado == null ? NotFound() : View(empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Empleado,Nombre,Apellido,Cargo,Telefono")] Empleado modelo)
        {
            if (id != modelo.ID_Empleado) return BadRequest();
            if (!ModelState.IsValid) return View(modelo);
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound();
            Limpiar(modelo);
            empleado.Nombre = modelo.Nombre;
            empleado.Apellido = modelo.Apellido;
            empleado.Cargo = modelo.Cargo;
            empleado.Telefono = modelo.Telefono;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Empleado actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var empleado = await _context.Empleados.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ID_Empleado == id);
            if (empleado == null) return NotFound();
            ViewBag.TienePedidos = await _context.Pedidos.AnyAsync(p => p.ID_Empleado == id);
            return View(empleado);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return RedirectToAction(nameof(Index));
            if (await _context.Pedidos.AnyAsync(p => p.ID_Empleado == id))
            {
                TempData["Error"] = "No se puede eliminar un empleado asociado a pedidos.";
                return RedirectToAction(nameof(Index));
            }
            _context.Empleados.Remove(empleado);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Empleado eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private static void Limpiar(Empleado e)
        {
            e.Nombre = e.Nombre.Trim();
            e.Apellido = e.Apellido.Trim();
            e.Cargo = e.Cargo.Trim();
            e.Telefono = e.Telefono.Trim();
        }
    }
}
