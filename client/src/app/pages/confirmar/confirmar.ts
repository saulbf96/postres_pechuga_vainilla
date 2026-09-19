import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CarritoService } from '../../core/services/carrito';
import { PedidosService } from '../../core/services/pedidos';
import type { ItemCarrito } from '../../core/models/carrito.model';
import type { CheckoutRequest, MetodoPago, PuntoEntregaDto } from '../../core/models/pedido.model';

interface SeleccionGrupo {
  vendedorId: number;
  vendedorNombre: string;
  items: ItemCarrito[];
  puntosEntrega: PuntoEntregaDto[];
  puntoEntregaId: number | null;
  fecha: string;
  hora: string;
}

@Component({
  imports: [FormsModule],
  selector: 'app-confirmar',
  styleUrl: './confirmar.css',
  templateUrl: './confirmar.html',
})
export class Confirmar implements OnInit {
  private readonly carrito = inject(CarritoService);
  private readonly pedidosService = inject(PedidosService);
  private readonly router = inject(Router);

  protected readonly nombreCliente = signal('');
  protected readonly whatsApp = signal('');
  protected readonly detalleEntrega = signal('');
  protected readonly metodoPago = signal<MetodoPago>('Efectivo');

  protected readonly selecciones = signal<SeleccionGrupo[]>([]);
  protected readonly enviando = signal(false);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    const grupos = this.carrito.grupos();

    if (grupos.length === 0) {
      this.router.navigateByUrl('/carrito');
      return;
    }

    for (const grupo of grupos) {
      // Cada grupo (vendedor) arranca sin puntos cargados todavia; se llenan al responder la API.
      this.selecciones.update((actual) => [
        ...actual,
        {
          vendedorId: grupo.vendedorId,
          vendedorNombre: grupo.vendedorNombre,
          items: grupo.items,
          puntosEntrega: [],
          puntoEntregaId: null,
          fecha: '',
          hora: '',
        },
      ]);

      this.pedidosService.obtenerPuntosEntrega(grupo.vendedorId).subscribe((puntos) => {
        this.selecciones.update((actual) =>
          actual.map((s) =>
            s.vendedorId === grupo.vendedorId
              ? { ...s, puntosEntrega: puntos, puntoEntregaId: puntos[0]?.id ?? null }
              : s,
          ),
        );
      });
    }
  }

  protected subtotalGrupo(items: ItemCarrito[]): number {
    return items.reduce((suma, i) => suma + i.precioUnitario * i.cantidad, 0);
  }

  protected puntoElegido(seleccion: SeleccionGrupo): PuntoEntregaDto | undefined {
    return seleccion.puntosEntrega.find((p) => p.id === seleccion.puntoEntregaId);
  }

  protected fechaMinima(seleccion: SeleccionGrupo): string {
    const punto = this.puntoElegido(seleccion);
    const dias = punto?.diasAnticipacion ?? 0;
    const fecha = new Date();
    fecha.setDate(fecha.getDate() + dias);
    return fecha.toISOString().slice(0, 10);
  }

  protected actualizarCampo(vendedorId: number, cambios: Partial<SeleccionGrupo>): void {
    this.selecciones.update((actual) =>
      actual.map((s) => (s.vendedorId === vendedorId ? { ...s, ...cambios } : s)),
    );
  }

  protected enviar(): void {
    this.error.set(null);

    if (!this.nombreCliente().trim() || !this.whatsApp().trim()) {
      this.error.set('Escribe tu nombre y tu WhatsApp.');
      return;
    }

    for (const s of this.selecciones()) {
      if (!s.puntoEntregaId || !s.fecha || !s.hora) {
        this.error.set(`Falta elegir punto de entrega, fecha y hora para ${s.vendedorNombre}.`);
        return;
      }
    }

    const request: CheckoutRequest = {
      nombreCliente: this.nombreCliente(),
      whatsApp: this.whatsApp(),
      detalleEntrega: this.detalleEntrega() || null,
      metodoPago: this.metodoPago(),
      grupos: this.selecciones().map((s) => ({
        vendedorId: s.vendedorId,
        puntoEntregaId: s.puntoEntregaId!,
        fechaEntrega: s.fecha,
        horaEntrega: `${s.hora}:00`,
        items: s.items.map((i) => ({
          productoId: i.productoId,
          presentacionId: i.presentacionId,
          cantidad: i.cantidad,
          notas: i.notas ?? null,
        })),
      })),
    };

    this.enviando.set(true);
    this.pedidosService.crearPedido(request).subscribe({
      next: () => {
        this.carrito.vaciar();
        this.router.navigateByUrl('/mis-pedidos');
      },
      error: (err) => {
        this.enviando.set(false);
        this.error.set(err.error?.mensaje ?? 'No se pudo crear el pedido. Intenta de nuevo.');
      },
    });
  }
}
