using Microsoft.EntityFrameworkCore;
using ApiPagos.Models;

namespace ApiPagos.Data;

public class PagosDbContext(DbContextOptions<PagosDbContext> options) : DbContext(options)
{
    public DbSet<Pago> Pagos => Set<Pago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pago>(e =>
        {
            e.HasKey(x => x.IdPago);
            e.Property(x => x.MontoPago).HasColumnType("decimal(9,2)");
            e.Property(x => x.FechaPago).IsRequired();
            e.Property(x => x.FormaPago).IsRequired();
        });
    }
}
