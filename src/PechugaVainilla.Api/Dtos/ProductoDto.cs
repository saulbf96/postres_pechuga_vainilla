namespace PechugaVainilla.Api.Dtos;

public record PresentacionDto(int Id, string Nombre, decimal Precio);

public record ProductoDto(
    int Id,
    string Nombre,
    string? Descripcion,
    string? Alergenos,
    string? FotoRuta,
    int VendedorId,
    string VendedorNombre,
    List<PresentacionDto> Presentaciones);
