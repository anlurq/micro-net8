using Microsoft.EntityFrameworkCore;
using ApiPedidos.Models;

namespace ApiPedidos.Data;

public class PedidosDbContext(DbContextOptions<PedidosDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(e =>
        {
            e.HasKey(x => x.IdCliente);
            e.Property(x => x.NombreCliente).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Pedido>(e =>
        {
            e.HasKey(x => x.IdPedido);
            e.Property(x => x.MontoPedido).HasColumnType("decimal(9,2)");
            e.Property(x => x.FechaPedido).IsRequired();
            e.Property(x => x.FormaPago).IsRequired(); // 1..3 (validas con FluentValidation en el request)
            e.HasOne(x => x.Cliente)
             .WithMany(c => c.Pedidos)
             .HasForeignKey(x => x.IdCliente)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
