using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Restaurante.Models;
public enum EstadoFactura { Pendiente, Pagada, Anulada }
public enum MetodoPagoFactura { Efectivo, Tarjeta, Transferencia, QR }
public class Factura
{
    [Key] public int ID_Factura { get; set; }
    public int ID_Pedido { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public DateTime Fecha { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Subtotal { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Impuestos { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Total { get; set; }
    public MetodoPagoFactura MetodoPago { get; set; }
    public EstadoFactura Estado { get; set; }
    public string DatosPedidoJson { get; set; } = string.Empty;
}
