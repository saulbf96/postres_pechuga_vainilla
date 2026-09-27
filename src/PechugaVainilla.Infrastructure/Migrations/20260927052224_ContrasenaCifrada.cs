using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PechugaVainilla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ContrasenaCifrada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordCifrada",
                table: "Usuarios",
                type: "varbinary(max)",
                nullable: true);

            // Llaves de SQL Server para cifrar/descifrar PasswordCifrada sin escribir frases:
            // la master key queda protegida por el servidor, asi DECRYPTBYKEYAUTOCERT la abre sola.
            // Su contraseña se genera al azar (NEWID) porque nunca se vuelve a necesitar.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.symmetric_keys WHERE name = '##MS_DatabaseMasterKey##')
                BEGIN
                    DECLARE @clave nvarchar(100) = CONVERT(nvarchar(36), NEWID()) + 'Aa1!';
                    EXEC('CREATE MASTER KEY ENCRYPTION BY PASSWORD = ''' + @clave + '''');
                END");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.certificates WHERE name = 'CertContrasenas')
                    CREATE CERTIFICATE CertContrasenas WITH SUBJECT = 'Cifrado de contrasenas Pechuga y Vainilla';");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.symmetric_keys WHERE name = 'ClaveContrasenas')
                    CREATE SYMMETRIC KEY ClaveContrasenas WITH ALGORITHM = AES_256
                    ENCRYPTION BY CERTIFICATE CertContrasenas;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // La master key se deja: puede usarla otra cosa de la base.
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.symmetric_keys WHERE name = 'ClaveContrasenas') DROP SYMMETRIC KEY ClaveContrasenas;");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.certificates WHERE name = 'CertContrasenas') DROP CERTIFICATE CertContrasenas;");

            migrationBuilder.DropColumn(
                name: "PasswordCifrada",
                table: "Usuarios");
        }
    }
}
