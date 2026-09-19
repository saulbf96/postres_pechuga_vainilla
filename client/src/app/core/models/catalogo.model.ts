// Estas interfaces reflejan exactamente los DTOs que devuelve la API
// (src/PechugaVainilla.Api/Dtos) - si cambia uno, hay que cambiar el otro.

export interface VendedorDto {
  id: number;
  nombre: string;
  slug: string;
  whatsApp: string;
  descripcion: string | null;
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
  vendedorId: number;
  vendedorNombre: string;
  presentaciones: PresentacionDto[];
}
