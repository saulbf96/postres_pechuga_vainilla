namespace PechugaVainilla.Api.Dtos;

public record PresentacionAdminDto(int Id, string Nombre, decimal Precio, bool Activo);

public record AdminProductoDto(
    int Id,
    int VendedorId,
    string VendedorNombre,
    string Nombre,
    string? Descripcion,
    string? Alergenos,
    string? FotoRuta,
    int? MaxPorDia,
    bool Activo,
    List<PresentacionAdminDto> Presentaciones);

public record PresentacionRequest(string Nombre, decimal Precio);

public record CrearProductoRequest(
    int? VendedorId,
    string Nombre,
    string? Descripcion,
    string? Alergenos,
    int? MaxPorDia,
    List<PresentacionRequest> Presentaciones);

public record EditarProductoRequest(string Nombre, string? Descripcion, string? Alergenos, int? MaxPorDia);

public record CambiarActivoRequest(bool Activo);
