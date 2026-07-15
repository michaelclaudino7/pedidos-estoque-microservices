using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pedidos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrecaoColunasItemPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomeProduto",
                table: "ItemPedido",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Quantidade",
                table: "ItemPedido",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoUnitario",
                table: "ItemPedido",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "NomeProduto", table: "ItemPedido");
            migrationBuilder.DropColumn(name: "Quantidade", table: "ItemPedido");
            migrationBuilder.DropColumn(name: "PrecoUnitario", table: "ItemPedido");
        }
    }
}
