import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PedidosService } from '../../core/services/pedidos';
import type { PedidoDto } from '../../core/models/pedido.model';

@Component({
  imports: [RouterLink],
  selector: 'app-mis-pedidos',
  styleUrl: './mis-pedidos.css',
  templateUrl: './mis-pedidos.html',
})
export class MisPedidos implements OnInit {
  private readonly pedidosService = inject(PedidosService);

  protected readonly pedidos = signal<PedidoDto[]>([]);
  protected readonly cargando = signal(true);

  ngOnInit(): void {
    this.pedidosService.obtenerMisPedidos().subscribe({
      next: (pedidos) => {
        this.pedidos.set(pedidos);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false),
    });
  }

  // Colores del chip de estado, para que se distingan de un vistazo.
  protected colorEstado(estado: string): { fondo: string; texto: string } {
    switch (estado) {
      case 'Entregado':
        return { fondo: '#E3EADC', texto: '#3F5E3A' };
      case 'Cancelado':
        return { fondo: '#F3DDD6', texto: '#8F2A3F' };
      default:
        return { fondo: '#FBF5EA', texto: '#6E5A52' };
    }
  }
}
