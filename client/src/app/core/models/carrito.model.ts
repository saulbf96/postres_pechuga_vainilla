export interface ItemCarrito {
  vendedorId: number;
  vendedorNombre: string;
  productoId: number;
  productoNombre: string;
  presentacionId: number;
  presentacionNombre: string;
  precioUnitario: number;
  cantidad: number;
  notas?: string;
}

export interface GrupoCarrito {
  vendedorId: number;
  vendedorNombre: string;
  items: ItemCarrito[];
}
