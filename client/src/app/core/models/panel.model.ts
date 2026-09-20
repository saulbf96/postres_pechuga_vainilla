export interface PresentacionAdminDto {
  id: number;
  nombre: string;
  precio: number;
  activo: boolean;
}

export interface AdminProductoDto {
  id: number;
  vendedorId: number;
  vendedorNombre: string;
  nombre: string;
  descripcion: string | null;
  alergenos: string | null;
  fotoRuta: string | null;
  maxPorDia: number | null;
  activo: boolean;
  presentaciones: PresentacionAdminDto[];
}

export interface PresentacionRequest {
  nombre: string;
  precio: number;
}

export interface CrearProductoRequest {
  vendedorId: number | null;
  nombre: string;
  descripcion: string | null;
  alergenos: string | null;
  maxPorDia: number | null;
  presentaciones: PresentacionRequest[];
}

export interface EditarProductoRequest {
  nombre: string;
  descripcion: string | null;
  alergenos: string | null;
  maxPorDia: number | null;
}

export interface AdminPuntoEntregaDto {
  id: number;
  vendedorId: number;
  vendedorNombre: string;
  nombre: string;
  tipo: string;
  diasSemana: string[];
  horaInicio: string;
  horaFin: string;
  diasAnticipacion: number;
  horaLimitePedido: string;
  costoEnvio: number;
  activo: boolean;
}

export interface GuardarPuntoEntregaRequest {
  vendedorId: number | null;
  nombre: string;
  tipo: string;
  diasSemana: string[];
  horaInicio: string;
  horaFin: string;
  diasAnticipacion: number;
  horaLimitePedido: string;
  costoEnvio: number;
}

export interface AdminPedidoDto {
  id: number;
  checkoutId: string;
  vendedorNombre: string;
  nombreCliente: string;
  whatsAppCliente: string;
  puntoEntregaNombre: string;
  fechaEntrega: string;
  horaEntrega: string;
  detalleEntrega: string | null;
  total: number;
  estado: string;
  estadoPago: string;
  detalles: { id: number; nombreProducto: string; precioUnitario: number; cantidad: number; notas: string | null }[];
}

export const ESTADOS_PEDIDO = ['Nuevo', 'Preparando', 'Listo', 'Entregado', 'Cancelado'] as const;
export const ESTADOS_PAGO = ['Pendiente', 'Pagado', 'PorConfirmar', 'PorCobrar', 'Cobrado'] as const;
export const DIAS_SEMANA = ['Lunes', 'Martes', 'Miercoles', 'Jueves', 'Viernes', 'Sabado', 'Domingo'] as const;

export interface AsignacionDto {
  id: number;
  vendedorId: number;
  vendedorNombre: string;
  diasSemana: string[];
  horaInicio: string;
  horaFin: string;
  activo: boolean;
}

export interface PerfilDto {
  id: string;
  nombre: string;
  email: string;
  whatsApp: string | null;
  roles: string[];
  asignaciones: AsignacionDto[];
}

export interface NuevaAsignacionRequest {
  vendedorId: number;
  diasSemana: string[];
  horaInicio: string;
  horaFin: string;
}

export interface CrearUsuarioRequest {
  nombre: string;
  email: string;
  whatsApp: string | null;
  password: string;
  rol: 'Administrador' | 'Vendedor';
  asignaciones: NuevaAsignacionRequest[];
}
