import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CarritoService } from '../../core/services/carrito';
import type { ItemCarrito } from '../../core/models/carrito.model';

@Component({
  imports: [RouterLink],
  selector: 'app-carrito',
  styleUrl: './carrito.css',
  templateUrl: './carrito.html',
})
export class Carrito {
  protected readonly carrito = inject(CarritoService);
  private readonly router = inject(Router);

  protected subtotalGrupo(items: ItemCarrito[]): number {
    return items.reduce((suma, i) => suma + i.precioUnitario * i.cantidad, 0);
  }

  protected cambiarCantidad(item: ItemCarrito, delta: number): void {
    this.carrito.actualizarCantidad(item.presentacionId, item.cantidad + delta);
  }

  protected quitar(item: ItemCarrito): void {
    this.carrito.quitar(item.presentacionId);
  }

  protected continuar(): void {
    this.router.navigateByUrl('/confirmar');
  }
}
