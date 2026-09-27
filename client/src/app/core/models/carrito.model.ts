export interface ItemCarrito {
  categoriaId: number;
  categoriaNombre: string;
  productoId: number;
  productoNombre: string;
  presentacionId: number;
  presentacionNombre: string;
  precioUnitario: number;
  cantidad: number;
  notas?: string;
}

export interface GrupoCarrito {
  categoriaId: number;
  categoriaNombre: string;
  items: ItemCarrito[];
}
