export type MetodoPago = 'Efectivo' | 'Transferencia';

export interface PuntoEntregaDto {
  id: number;
  vendedorId: number;
  nombre: string;
  tipo: string;
  diasSemana: string[];
  horaInicio: string;
  horaFin: string;
  diasAnticipacion: number;
  horaLimitePedido: string;
  costoEnvio: number;
}

export interface ItemCarritoRequest {
  productoId: number;
  presentacionId: number;
  cantidad: number;
  notas: string | null;
}

export interface GrupoCheckoutRequest {
  vendedorId: number;
  puntoEntregaId: number;
  fechaEntrega: string; // yyyy-MM-dd
  horaEntrega: string; // HH:mm:ss
  items: ItemCarritoRequest[];
}

export interface CheckoutRequest {
  nombreCliente: string;
  whatsApp: string;
  detalleEntrega: string | null;
  metodoPago: MetodoPago;
  grupos: GrupoCheckoutRequest[];
}

export interface PedidoDetalleDto {
  id: number;
  nombreProducto: string;
  precioUnitario: number;
  cantidad: number;
  notas: string | null;
}

export interface PedidoDto {
  id: number;
  checkoutId: string;
  vendedorNombre: string;
  puntoEntregaNombre: string;
  fechaEntrega: string;
  horaEntrega: string;
  detalleEntrega: string | null;
  total: number;
  estado: string;
  estadoPago: string;
  detalles: PedidoDetalleDto[];
}
