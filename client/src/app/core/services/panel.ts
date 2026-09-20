import { HttpClient } from '@angular/common/http';
import { Service, inject } from '@angular/core';
import type {
  AdminPedidoDto,
  AdminProductoDto,
  AdminPuntoEntregaDto,
  CrearProductoRequest,
  EditarProductoRequest,
  GuardarPuntoEntregaRequest,
  PresentacionAdminDto,
  PresentacionRequest,
} from '../models/panel.model';

@Service()
export class PanelService {
  private readonly http = inject(HttpClient);

  // Productos
  obtenerProductos() {
    return this.http.get<AdminProductoDto[]>('/api/v1/admin/productos');
  }

  crearProducto(request: CrearProductoRequest) {
    return this.http.post<AdminProductoDto>('/api/v1/admin/productos', request);
  }

  editarProducto(id: number, request: EditarProductoRequest) {
    return this.http.put<void>(`/api/v1/admin/productos/${id}`, request);
  }

  cambiarActivoProducto(id: number, activo: boolean) {
    return this.http.patch<void>(`/api/v1/admin/productos/${id}/activo`, { activo });
  }

  agregarPresentacion(productoId: number, request: PresentacionRequest) {
    return this.http.post<PresentacionAdminDto>(`/api/v1/admin/productos/${productoId}/presentaciones`, request);
  }

  editarPresentacion(presentacionId: number, request: PresentacionRequest) {
    return this.http.put<void>(`/api/v1/admin/productos/presentaciones/${presentacionId}`, request);
  }

  cambiarActivaPresentacion(presentacionId: number, activo: boolean) {
    return this.http.patch<void>(`/api/v1/admin/productos/presentaciones/${presentacionId}/activo`, { activo });
  }

  // Puntos de entrega
  obtenerPuntosEntrega() {
    return this.http.get<AdminPuntoEntregaDto[]>('/api/v1/admin/puntos-entrega');
  }

  crearPuntoEntrega(request: GuardarPuntoEntregaRequest) {
    return this.http.post<AdminPuntoEntregaDto>('/api/v1/admin/puntos-entrega', request);
  }

  editarPuntoEntrega(id: number, request: GuardarPuntoEntregaRequest) {
    return this.http.put<void>(`/api/v1/admin/puntos-entrega/${id}`, request);
  }

  cambiarActivoPuntoEntrega(id: number, activo: boolean) {
    return this.http.patch<void>(`/api/v1/admin/puntos-entrega/${id}/activo`, { activo });
  }

  // Pedidos
  obtenerPedidos() {
    return this.http.get<AdminPedidoDto[]>('/api/v1/admin/pedidos');
  }

  cambiarEstadoPedido(id: number, estado: string) {
    return this.http.patch<void>(`/api/v1/admin/pedidos/${id}/estado`, { estado });
  }

  cambiarEstadoPago(id: number, estado: string) {
    return this.http.patch<void>(`/api/v1/admin/pedidos/${id}/pago`, { estado });
  }
}
