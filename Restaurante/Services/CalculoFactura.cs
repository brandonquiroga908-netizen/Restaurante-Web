using Restaurante.Models;
namespace Restaurante.Services;
public static class CalculoFactura
{
    public static (decimal Subtotal, decimal Impuestos, decimal Total) Calcular(IEnumerable<DetallePedido> detalles, decimal tasa)
    {
        if (tasa < 0 || tasa > 1) throw new InvalidOperationException("La tasa debe estar entre 0 y 1.");
        var lineas = detalles.ToList();
        if (lineas.Count == 0) throw new InvalidOperationException("El pedido no contiene productos.");
        if (lineas.Any(d => d.Cantidad <= 0 || d.PrecioUnitario < 0))
            throw new InvalidOperationException("El pedido contiene cantidades o precios inválidos.");
        var subtotal = lineas.Sum(d => decimal.Round(d.Cantidad * d.PrecioUnitario, 2, MidpointRounding.AwayFromZero));
        var impuestos = decimal.Round(subtotal * tasa, 2, MidpointRounding.AwayFromZero);
        var total = subtotal + impuestos;
        if (total > 9999999999999999.99m) throw new InvalidOperationException("El total supera el importe permitido.");
        return (subtotal, impuestos, total);
    }
}
