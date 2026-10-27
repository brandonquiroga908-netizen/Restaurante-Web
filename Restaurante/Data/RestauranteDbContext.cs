using Microsoft.EntityFrameworkCore;
using Restaurante.Models;

namespace Restaurante.Data
{
    public class RestauranteDbContext : DbContext
    {
        public RestauranteDbContext(DbContextOptions<RestauranteDbContext> options) : base(options) { }

        public DbSet<Factura> Facturas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<Empleado> Empleados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Factura>().HasOne(f => f.Pedido).WithOne()
                .HasForeignKey<Factura>(f => f.ID_Pedido).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Factura>().HasIndex(f => f.ID_Pedido).IsUnique();
            modelBuilder.Entity<Factura>().Property(f => f.Estado).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<Factura>().Property(f => f.MetodoPago).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<Factura>().ToTable("Facturas", t => {
                t.HasCheckConstraint("CK_Facturas_Importes", "[Subtotal] >= 0 AND [Impuestos] >= 0 AND [Total] = [Subtotal] + [Impuestos]");
                t.HasCheckConstraint("CK_Facturas_Estado", "[Estado] IN ('Pendiente','Pagada','Anulada')");
                t.HasCheckConstraint("CK_Facturas_MetodoPago", "[MetodoPago] IN ('Efectivo','Tarjeta','Transferencia','QR')");
            });
            modelBuilder.Entity<Mesa>()
                .HasIndex(m => m.Numero).IsUnique();

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Mesa)
                .WithMany(m => m.Pedidos)
                .HasForeignKey(p => p.ID_Mesa)
                .OnDelete(DeleteBehavior.Restrict);


            // Relación entre empleados y pedidos
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Empleado)
                .WithMany(e => e.Pedidos)
                .HasForeignKey(p => p.ID_Empleado)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación entre clientes y pedidos
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.ID_Cliente)
                .OnDelete(DeleteBehavior.Restrict);

            // Al borrar un pedido se borran sus detalles

            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.ID_Pedido)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ID_Producto)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
