import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PanelService } from '../../core/services/panel';
import { PanelNav } from '../../shared/panel-nav/panel-nav';
import { ESTADOS_PAGO, ESTADOS_PEDIDO } from '../../core/models/panel.model';
import type { AdminPedidoDto } from '../../core/models/panel.model';

@Component({
  imports: [FormsModule, PanelNav],
  selector: 'app-panel-pedidos',
  styleUrl: './panel-pedidos.css',
  templateUrl: './panel-pedidos.html',
})
export class PanelPedidos implements OnInit {
  private readonly panelService = inject(PanelService);

  protected readonly pedidos = signal<AdminPedidoDto[]>([]);
  protected readonly cargando = signal(true);
  protected readonly estadosPedido = ESTADOS_PEDIDO;
  protected readonly estadosPago = ESTADOS_PAGO;

  ngOnInit(): void {
    this.cargar();
  }

  private cargar(): void {
    this.cargando.set(true);
    this.panelService.obtenerPedidos().subscribe({
      next: (pedidos) => {
        this.pedidos.set(pedidos);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false),
    });
  }

  protected cambiarEstado(pedido: AdminPedidoDto, estado: string): void {
    this.panelService.cambiarEstadoPedido(pedido.id, estado).subscribe(() => this.cargar());
  }

  protected cambiarEstadoPago(pedido: AdminPedidoDto, estado: string): void {
    this.panelService.cambiarEstadoPago(pedido.id, estado).subscribe(() => this.cargar());
  }
}
