export type MetodoPago = 'Efectivo' | 'Transferencia';

export interface PuntoEntregaDto {
  id: number;
  nombre: string;
  tipo: string;
  diasSemana: string[];
  horaInicio: string;
  horaFin: string;
  diasAnticipacion: number;
  horaLimitePedido: string;
  costoEnvio: number;
  categoriaIds: number[];
}

export interface ItemCarritoRequest {
  productoId: number;
  presentacionId: number;
  cantidad: number;
  notas: string | null;
}

// Un checkout es un solo pedido (ya no se separa por vendedor): una entrega para todo el carrito.
export interface CheckoutRequest {
  nombreCliente: string;
  whatsApp: string;
  puntoEntregaId: number;
  fechaEntrega: string; // yyyy-MM-dd
  horaEntrega: string; // HH:mm:ss
  detalleEntrega: string | null;
  metodoPago: MetodoPago;
  items: ItemCarritoRequest[];
}

export interface PedidoDetalleDto {
  id: number;
  nombreProducto: string;
  categoriaId: number;
  precioUnitario: number;
  cantidad: number;
  notas: string | null;
}

export interface PedidoDto {
  id: number;
  puntoEntregaNombre: string;
  fechaEntrega: string;
  horaEntrega: string;
  detalleEntrega: string | null;
  total: number;
  estado: string;
  estadoPago: string;
  detalles: PedidoDetalleDto[];
}
