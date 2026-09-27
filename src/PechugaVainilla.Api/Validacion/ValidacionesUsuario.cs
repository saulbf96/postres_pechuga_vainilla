using System.Text.RegularExpressions;

namespace PechugaVainilla.Api.Validacion;

// El navegador ya valida esto (registro.ts), pero el servidor NUNCA debe confiar solo en eso -
// alguien podria mandar la peticion directo (curl, Postman) sin pasar por el formulario.
// Reglas: CLAUDE.md "Validar toda entrada (longitudes, rangos, formatos)".
public static partial class ValidacionesUsuario
{
    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex RegexCorreo();

    [GeneratedRegex(@"^\d{10}$")]
    private static partial Regex RegexDiezDigitos();

    public static List<string> ValidarDatosBasicos(string? nombre, string? email, string? whatsApp, bool whatsAppRequerido)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add("Escribe el nombre.");
        }
        else if (nombre.Any(char.IsDigit))
        {
            errores.Add("El nombre no debe tener números.");
        }

        if (string.IsNullOrWhiteSpace(email) || !RegexCorreo().IsMatch(email))
        {
            errores.Add("El correo no es válido.");
        }

        if (whatsAppRequerido && string.IsNullOrWhiteSpace(whatsApp))
        {
            errores.Add("Escribe el WhatsApp.");
        }
        else if (!string.IsNullOrWhiteSpace(whatsApp) && !RegexDiezDigitos().IsMatch(whatsApp))
        {
            errores.Add("El WhatsApp debe tener exactamente 10 números, sin espacios ni letras.");
        }

        return errores;
    }
}
