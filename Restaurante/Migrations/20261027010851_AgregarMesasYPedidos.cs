using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurante.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMesasYPedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mesas",
                columns: table => new
                {
                    ID_Mesa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Capacidad = table.Column<int>(type: "int", nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesas", x => x.ID_Mesa);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    ID_Pedido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ID_Mesa = table.Column<int>(type: "int", nullable: false),
                    Cliente = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.ID_Pedido);
                    table.ForeignKey(
                        name: "FK_Pedidos_Mesas_ID_Mesa",
                        column: x => x.ID_Mesa,
                        principalTable: "Mesas",
                        principalColumn: "ID_Mesa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesPedido",
                columns: table => new
                {
                    ID_Detalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_Pedido = table.Column<int>(type: "int", nullable: false),
                    ID_Producto = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPedido", x => x.ID_Detalle);
                    table.ForeignKey(
                        name: "FK_DetallesPedido_Pedidos_ID_Pedido",
                        column: x => x.ID_Pedido,
                        principalTable: "Pedidos",
                        principalColumn: "ID_Pedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesPedido_Productos_ID_Producto",
                        column: x => x.ID_Producto,
                        principalTable: "Productos",
                        principalColumn: "ID_Producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_ID_Pedido",
                table: "DetallesPedido",
                column: "ID_Pedido");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_ID_Producto",
                table: "DetallesPedido",
                column: "ID_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_Mesas_Numero",
                table: "Mesas",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ID_Mesa",
                table: "Pedidos",
                column: "ID_Mesa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesPedido");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Mesas");
        }
    }
}
