export interface PresentacionAdminDto {
  id: number;
  nombre: string;
  precio: number;
  activo: boolean;
}

export interface AdminProductoDto {
  id: number;
  categoriaId: number;
  categoriaNombre: string;
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
  categoriaId: number;
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
  nombre: string;
  tipo: string;
  diasSemana: string[];
  horaInicio: string;
  horaFin: string;
  diasAnticipacion: number;
  horaLimitePedido: string;
  costoEnvio: number;
  activo: boolean;
  categoriaIds: number[];
}

export interface GuardarPuntoEntregaRequest {
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

export interface AdminPedidoDto {
  id: number;
  nombreCliente: string;
  whatsAppCliente: string;
  puntoEntregaNombre: string;
  fechaEntrega: string;
  horaEntrega: string;
  detalleEntrega: string | null;
  total: number;
  estado: string;
  estadoPago: string;
  detalles: { id: number; nombreProducto: string; categoriaId: number; precioUnitario: number; cantidad: number; notas: string | null }[];
}

export const ESTADOS_PEDIDO = ['Nuevo', 'Preparando', 'Listo', 'Entregado', 'Cancelado'] as const;
export const ESTADOS_PAGO = ['Pendiente', 'Pagado', 'PorConfirmar', 'PorCobrar', 'Cobrado'] as const;
export const DIAS_SEMANA = ['Lunes', 'Martes', 'Miercoles', 'Jueves', 'Viernes', 'Sabado', 'Domingo'] as const;

export interface PerfilDto {
  id: string;
  nombre: string;
  email: string;
  whatsApp: string | null;
  roles: string[];
  debeCambiarPassword: boolean;
}

// Sin contraseña: el sistema genera una temporal y la regresa al crear.
export interface CrearUsuarioRequest {
  nombre: string;
  email: string;
  whatsApp: string | null;
  rol: 'Administrador' | 'Cliente';
}

// Correo y contraseña de un administrador para autorizar expirar/restablecer contraseñas.
export interface AutorizacionAdminRequest {
  emailAdmin: string;
  passwordAdmin: string;
}
