using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurante.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaEmpleados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ID_Empleado",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    ID_Empleado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.ID_Empleado);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ID_Empleado",
                table: "Pedidos",
                column: "ID_Empleado");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Empleados_ID_Empleado",
                table: "Pedidos",
                column: "ID_Empleado",
                principalTable: "Empleados",
                principalColumn: "ID_Empleado",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Empleados_ID_Empleado",
                table: "Pedidos");

            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_ID_Empleado",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ID_Empleado",
                table: "Pedidos");
        }
    }
}
