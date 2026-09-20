using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PechugaVainilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AsignacionesVendedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsignacionesVendedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    DiasSemana = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesVendedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionesVendedor_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsignacionesVendedor_Vendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesVendedor_UsuarioId",
                table: "AsignacionesVendedor",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesVendedor_VendedorId",
                table: "AsignacionesVendedor",
                column: "VendedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionesVendedor");
        }
    }
}
