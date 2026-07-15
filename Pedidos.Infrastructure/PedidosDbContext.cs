using Microsoft.EntityFrameworkCore;
using Pedidos.Domain;

namespace Pedidos.Infrastructure;

public class PedidosDbContext : DbContext
{
    public PedidosDbContext(DbContextOptions<PedidosDbContext> options) : base(options)
    {
    }

    public DbSet<Pedido> Pedidos => Set<Pedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pedido>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(p => p.ValorTotal).HasColumnType("numeric(18,2)");

            builder.OwnsMany(p => p.Itens, item =>
            {
                item.WithOwner().HasForeignKey("PedidoId");
                item.Property<int>("Id");
                item.HasKey("Id");

                item.Property(i => i.NomeProduto).IsRequired().HasMaxLength(200);
                item.Property(i => i.Quantidade).IsRequired();
                item.Property(i => i.PrecoUnitario).HasColumnType("numeric(18,2)");
            });
        });

        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}