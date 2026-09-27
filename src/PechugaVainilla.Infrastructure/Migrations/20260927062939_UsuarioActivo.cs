using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PechugaVainilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioActivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                // true a mano: EF genero false, y eso habria desactivado a todos los usuarios existentes.
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Usuarios");
        }
    }
}
