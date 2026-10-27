using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.Data;
using Restaurante.Models;
using Restaurante.ViewModels;

namespace Restaurante.Controllers
{
    public class ClientesController : Controller
    {
        private readonly RestauranteDbContext _context;

        public ClientesController(RestauranteDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? busqueda, int pagina = 1)
        {
            string filtro = busqueda?.Trim() ?? string.Empty;

            IQueryable<Cliente> consulta = _context.Clientes;

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                consulta = consulta.Where(c =>
                    c.Nombre.Contains(filtro) ||
                    c.Apellido.Contains(filtro) ||
                    c.Direccion.Contains(filtro));
            }

            var modelo = new ClientesViewModel
            {
                Busqueda = filtro,
                TotalClientes = await consulta.CountAsync()
            };

            modelo.PaginaActual = Math.Clamp(pagina, 1, modelo.TotalPaginas);

            modelo.Clientes = await consulta
                .OrderBy(c => c.ID_Cliente)
                .Skip((modelo.PaginaActual - 1) * modelo.ClientesPorPagina)
                .Take(modelo.ClientesPorPagina)
                .ToListAsync();

            return View(modelo);
        }

        public async Task<IActionResult> Detalles(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Pedidos)
                    .ThenInclude(p => p.Mesa)
                .FirstOrDefaultAsync(c => c.ID_Cliente == id);

            if (cliente == null)
            {
                return NotFound();
            }

            cliente.Pedidos = cliente.Pedidos
                .OrderByDescending(p => p.Fecha)
                .ToList();

            return View(cliente);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Cliente());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            [Bind("Nombre,Apellido,Telefono,Correo,Direccion")] Cliente modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            modelo.Nombre = modelo.Nombre.Trim();
            modelo.Apellido = modelo.Apellido.Trim();
            modelo.Telefono = modelo.Telefono.Trim();
            modelo.Correo = modelo.Correo.Trim();
            modelo.Direccion = modelo.Direccion.Trim();

            _context.Clientes.Add(modelo);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Cliente registrado correctamente.";

            return RedirectToAction(nameof(Detalles), new { id = modelo.ID_Cliente });
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            [Bind("ID_Cliente,Nombre,Apellido,Telefono,Correo,Direccion")] Cliente modelo)
        {
            if (id != modelo.ID_Cliente)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            cliente.Nombre = modelo.Nombre.Trim();
            cliente.Apellido = modelo.Apellido.Trim();
            cliente.Telefono = modelo.Telefono.Trim();
            cliente.Correo = modelo.Correo.Trim();
            cliente.Direccion = modelo.Direccion.Trim();

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Cliente actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        [HttpPost]
        [ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            bool tienePedidos = await _context.Pedidos.AnyAsync(p => p.ID_Cliente == id);

            if (tienePedidos)
            {
                TempData["Error"] = "No se puede eliminar el cliente porque tiene pedidos.";
                return RedirectToAction(nameof(Detalles), new { id });
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Cliente eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
