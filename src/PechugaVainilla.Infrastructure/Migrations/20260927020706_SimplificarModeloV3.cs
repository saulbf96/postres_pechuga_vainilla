using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PechugaVainilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplificarModeloV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Vendedores_VendedorId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Vendedores_VendedorId",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_PuntosEntrega_Vendedores_VendedorId",
                table: "PuntosEntrega");

            migrationBuilder.DropTable(
                name: "AsignacionesVendedor");

            migrationBuilder.DropTable(
                name: "Vendedores");

            migrationBuilder.DropIndex(
                name: "IX_PuntosEntrega_VendedorId",
                table: "PuntosEntrega");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_CheckoutId",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_VendedorId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "VendedorId",
                table: "PuntosEntrega");

            migrationBuilder.DropColumn(
                name: "CheckoutId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "VendedorId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "CobradoPorVendedorId",
                table: "Pagos");

            migrationBuilder.RenameColumn(
                name: "VendedorId",
                table: "Productos",
                newName: "CategoriaId");

            migrationBuilder.RenameIndex(
                name: "IX_Productos_VendedorId",
                table: "Productos",
                newName: "IX_Productos_CategoriaId");

            migrationBuilder.AddColumn<int>(
                name: "CategoriaId",
                table: "PedidoDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ColorSuave = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FotoRuta = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PuntosEntregaCategorias",
                columns: table => new
                {
                    PuntoEntregaId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuntosEntregaCategorias", x => new { x.PuntoEntregaId, x.CategoriaId });
                    table.ForeignKey(
                        name: "FK_PuntosEntregaCategorias_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PuntosEntregaCategorias_PuntosEntrega_PuntoEntregaId",
                        column: x => x.PuntoEntregaId,
                        principalTable: "PuntosEntrega",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Slug",
                table: "Categorias",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PuntosEntregaCategorias_CategoriaId",
                table: "PuntosEntregaCategorias",
                column: "CategoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Categorias_CategoriaId",
                table: "Productos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Categorias_CategoriaId",
                table: "Productos");

            migrationBuilder.DropTable(
                name: "PuntosEntregaCategorias");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "PedidoDetalles");

            migrationBuilder.RenameColumn(
                name: "CategoriaId",
                table: "Productos",
                newName: "VendedorId");

            migrationBuilder.RenameIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                newName: "IX_Productos_VendedorId");

            migrationBuilder.AddColumn<int>(
                name: "VendedorId",
                table: "PuntosEntrega",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutId",
                table: "Pedidos",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VendedorId",
                table: "Pedidos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CobradoPorVendedorId",
                table: "Pagos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Vendedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    WhatsApp = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendedores_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionesVendedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    DiasSemana = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesVendedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionesVendedor_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
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
                name: "IX_PuntosEntrega_VendedorId",
                table: "PuntosEntrega",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_CheckoutId",
                table: "Pedidos",
                column: "CheckoutId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_VendedorId",
                table: "Pedidos",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesVendedor_UsuarioId",
                table: "AsignacionesVendedor",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesVendedor_VendedorId",
                table: "AsignacionesVendedor",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendedores_Slug",
                table: "Vendedores",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendedores_UsuarioId",
                table: "Vendedores",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Vendedores_VendedorId",
                table: "Pedidos",
                column: "VendedorId",
                principalTable: "Vendedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Vendedores_VendedorId",
                table: "Productos",
                column: "VendedorId",
                principalTable: "Vendedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PuntosEntrega_Vendedores_VendedorId",
                table: "PuntosEntrega",
                column: "VendedorId",
                principalTable: "Vendedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
