using Estoque.Domain;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Infrastructure;

public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options)
    {
    }

    public DbSet<ProdutoEstoque> Produtos => Set<ProdutoEstoque>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProdutoEstoque>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(p => p.NomeProduto)
                .IsRequired()
                .HasMaxLength(200);

            // Garante que não existam dois registros de estoque para o mesmo produto
            builder.HasIndex(p => p.NomeProduto).IsUnique();
        });

        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
