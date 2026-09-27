using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Infrastructure.Data;

namespace PechugaVainilla.Infrastructure.Identity;

// Guarda una copia cifrada de la contraseña en Usuarios.PasswordCifrada con la llave
// simetrica ClaveContrasenas (se crea en la migracion ContrasenaCifrada), para poder
// leerla desde SQL con DECRYPTBYKEYAUTOCERT. El login no la usa: sigue con PasswordHash.
public class CifradoContrasena
{
    private readonly PechugaVainillaDbContext _db;

    public CifradoContrasena(PechugaVainillaDbContext db)
    {
        _db = db;
    }

    public async Task GuardarAsync(string usuarioId, string password, CancellationToken cancellationToken = default)
    {
        // Los {} se mandan como parametros de SQL, no se pegan al texto (no hay inyeccion SQL).
        await _db.Database.ExecuteSqlInterpolatedAsync($@"
            OPEN SYMMETRIC KEY ClaveContrasenas DECRYPTION BY CERTIFICATE CertContrasenas;
            UPDATE Usuarios SET PasswordCifrada = ENCRYPTBYKEY(KEY_GUID('ClaveContrasenas'), {password}) WHERE Id = {usuarioId};
            CLOSE SYMMETRIC KEY ClaveContrasenas;",
            cancellationToken);
    }
}
