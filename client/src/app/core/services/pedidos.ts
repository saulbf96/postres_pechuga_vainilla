import { HttpClient, HttpParams } from '@angular/common/http';
import { Service, inject } from '@angular/core';
import type { CheckoutRequest, PedidoDto, PuntoEntregaDto } from '../models/pedido.model';

@Service()
export class PedidosService {
  private readonly http = inject(HttpClient);

  // categoriaId opcional: sin el, trae todos los puntos activos.
  obtenerPuntosEntrega(categoriaId?: number) {
    let params = new HttpParams();
    if (categoriaId !== undefined) {
      params = params.set('categoriaId', categoriaId);
    }
    return this.http.get<PuntoEntregaDto[]>('/api/v1/puntos-entrega', { params });
  }

  crearPedido(request: CheckoutRequest) {
    return this.http.post<PedidoDto>('/api/v1/pedidos', request);
  }

  obtenerMisPedidos() {
    return this.http.get<PedidoDto[]>('/api/v1/pedidos/mis');
  }

  obtenerPedido(id: number) {
    return this.http.get<PedidoDto>(`/api/v1/pedidos/${id}`);
  }
}
