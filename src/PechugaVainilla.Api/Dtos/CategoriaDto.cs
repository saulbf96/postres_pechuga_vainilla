namespace PechugaVainilla.Api.Dtos;

public record CategoriaDto(int Id, string Nombre, string Slug, string? Descripcion, string Color, string ColorSuave, string? FotoRuta, int Orden);
