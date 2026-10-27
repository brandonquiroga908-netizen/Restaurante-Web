using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Restaurante.Models;
namespace Restaurante.ViewModels;
public class FacturasIndexViewModel
{
    public string? Buscar { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public EstadoFactura? Estado { get; set; }
    public int Pagina { get; set; } = 1;
    public int Paginas { get; set; } = 1;
    public int Cantidad { get; set; }
    public decimal TotalFacturado { get; set; }
    public int Pagadas { get; set; }
    public int Pendientes { get; set; }
    public int Anuladas { get; set; }
    public List<Factura> Facturas { get; set; } = new();
}
public class NuevaFacturaViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un pedido.")]
    public int ID_Pedido { get; set; }
    [Required, EnumDataType(typeof(MetodoPagoFactura))]
    public MetodoPagoFactura? MetodoPago { get; set; }
    [ValidateNever] public List<SelectListItem> Pedidos { get; set; } = new();
    [ValidateNever] public Pedido? Pedido { get; set; }
    [ValidateNever] public decimal? Tasa { get; set; }
    [ValidateNever] public decimal Subtotal { get; set; }
    [ValidateNever] public decimal Impuestos { get; set; }
    [ValidateNever] public decimal Total { get; set; }
}
public record LineaFactura(string Producto, int Cantidad, decimal PrecioUnitario, decimal Subtotal);
public record DatosFactura(string Cliente, string Empleado, List<LineaFactura> Lineas);
public record FacturaDetalleViewModel(Factura Factura, DatosFactura Datos);
