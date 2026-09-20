using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PechugaVainilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PanelVendedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Vendedores",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendedores_UsuarioId",
                table: "Vendedores",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vendedores_AspNetUsers_UsuarioId",
                table: "Vendedores",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vendedores_AspNetUsers_UsuarioId",
                table: "Vendedores");

            migrationBuilder.DropIndex(
                name: "IX_Vendedores_UsuarioId",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Vendedores");
        }
    }
}
