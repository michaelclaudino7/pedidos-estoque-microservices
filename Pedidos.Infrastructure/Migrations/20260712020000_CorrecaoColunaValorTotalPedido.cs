using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pedidos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrecaoColunaValorTotalPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ValorTotal",
                table: "Pedidos",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ValorTotal", table: "Pedidos");
        }
    }
}
