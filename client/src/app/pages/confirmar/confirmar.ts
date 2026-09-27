import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CarritoService } from '../../core/services/carrito';
import { PedidosService } from '../../core/services/pedidos';
import type { CheckoutRequest, MetodoPago, PuntoEntregaDto } from '../../core/models/pedido.model';

@Component({
  imports: [FormsModule],
  selector: 'app-confirmar',
  styleUrl: './confirmar.css',
  templateUrl: './confirmar.html',
})
export class Confirmar implements OnInit {
  protected readonly carrito = inject(CarritoService);
  private readonly pedidosService = inject(PedidosService);
  private readonly router = inject(Router);

  protected readonly nombreCliente = signal('');
  protected readonly whatsApp = signal('');
  protected readonly detalleEntrega = signal('');
  protected readonly metodoPago = signal<MetodoPago>('Efectivo');

  protected readonly puntosEntrega = signal<PuntoEntregaDto[]>([]);
  protected readonly puntoEntregaId = signal<number | null>(null);
  protected readonly fecha = signal('');
  protected readonly hora = signal('');

  protected readonly enviando = signal(false);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    const items = this.carrito.items();

    if (items.length === 0) {
      this.router.navigateByUrl('/carrito');
      return;
    }

    // Solo se pueden mostrar puntos donde esten disponibles TODAS las categorias del carrito.
    const categoriaIds = [...new Set(items.map((i) => i.categoriaId))];

    this.pedidosService.obtenerPuntosEntrega().subscribe((puntos) => {
      const validos = puntos.filter((p) => categoriaIds.every((id) => p.categoriaIds.includes(id)));
      this.puntosEntrega.set(validos);
      this.puntoEntregaId.set(validos[0]?.id ?? null);
    });
  }

  protected puntoElegido(): PuntoEntregaDto | undefined {
    return this.puntosEntrega().find((p) => p.id === this.puntoEntregaId());
  }

  protected fechaMinima(): string {
    const punto = this.puntoElegido();
    const dias = punto?.diasAnticipacion ?? 0;
    const fecha = new Date();
    fecha.setDate(fecha.getDate() + dias);
    return fecha.toISOString().slice(0, 10);
  }

  // Filtra mientras escribes: solo digitos, maximo 10 - una letra simplemente no aparece.
  protected onWhatsAppChange(valor: string): void {
    this.whatsApp.set(valor.replace(/\D/g, '').slice(0, 10));
  }

  protected enviar(): void {
    this.error.set(null);

    if (!this.nombreCliente().trim()) {
      this.error.set('Escribe tu nombre.');
      return;
    }
    if (!/^\d{10}$/.test(this.whatsApp())) {
      this.error.set('Escribe tu WhatsApp con exactamente 10 números.');
      return;
    }
    if (!this.puntoEntregaId() || !this.fecha() || !this.hora()) {
      this.error.set('Falta elegir punto de entrega, fecha y hora.');
      return;
    }

    const request: CheckoutRequest = {
      nombreCliente: this.nombreCliente(),
      whatsApp: this.whatsApp(),
      puntoEntregaId: this.puntoEntregaId()!,
      fechaEntrega: this.fecha(),
      horaEntrega: `${this.hora()}:00`,
      detalleEntrega: this.detalleEntrega() || null,
      metodoPago: this.metodoPago(),
      items: this.carrito.items().map((i) => ({
        productoId: i.productoId,
        presentacionId: i.presentacionId,
        cantidad: i.cantidad,
        notas: i.notas ?? null,
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
