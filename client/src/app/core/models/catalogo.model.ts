// Estas interfaces reflejan exactamente los DTOs que devuelve la API
// (src/PechugaVainilla.Api/Dtos) - si cambia uno, hay que cambiar el otro.

export interface CategoriaDto {
  id: number;
  nombre: string;
  slug: string;
  descripcion: string | null;
  color: string;
  colorSuave: string;
  fotoRuta: string | null;
  orden: number;
}

export interface PresentacionDto {
  id: number;
  nombre: string;
  precio: number;
}

export interface ProductoDto {
  id: number;
  nombre: string;
  descripcion: string | null;
  alergenos: string | null;
  fotoRuta: string | null;
  categoriaId: number;
  categoriaNombre: string;
  presentaciones: PresentacionDto[];
}
