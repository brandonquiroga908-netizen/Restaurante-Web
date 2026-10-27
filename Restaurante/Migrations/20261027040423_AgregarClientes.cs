using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurante.Migrations
{
    /// <inheritdoc />
    public partial class AgregarClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "Pedidos");

            migrationBuilder.AddColumn<int>(
                name: "ID_Cliente",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    ID_Cliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.ID_Cliente);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ID_Cliente",
                table: "Pedidos",
                column: "ID_Cliente");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Clientes_ID_Cliente",
                table: "Pedidos",
                column: "ID_Cliente",
                principalTable: "Clientes",
                principalColumn: "ID_Cliente",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Clientes_ID_Cliente",
                table: "Pedidos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_ID_Cliente",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ID_Cliente",
                table: "Pedidos");

            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "Pedidos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }
    }
}
