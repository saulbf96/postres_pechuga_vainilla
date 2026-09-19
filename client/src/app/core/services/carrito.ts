import { Service, computed, effect, signal } from '@angular/core';
import type { GrupoCarrito, ItemCarrito } from '../models/carrito.model';

const CLAVE_STORAGE = 'pyv-carrito';

// Carrito 100% del lado del cliente (no llama a la API) - solo se convierte en un
// pedido real cuando el usuario confirma en /confirmar. Se guarda en localStorage
// para que no se pierda si recargas la pagina.
@Service()
export class CarritoService {
  readonly items = signal<ItemCarrito[]>(cargarDeStorage());

  readonly totalItems = computed(() => this.items().reduce((suma, i) => suma + i.cantidad, 0));
  readonly totalPrecio = computed(() => this.items().reduce((suma, i) => suma + i.precioUnitario * i.cantidad, 0));

  // Agrupado por vendedor: el checkout siempre manda un grupo por vendedor (regla de negocio).
  readonly grupos = computed<GrupoCarrito[]>(() => {
    const mapa = new Map<number, GrupoCarrito>();
    for (const item of this.items()) {
      if (!mapa.has(item.vendedorId)) {
        mapa.set(item.vendedorId, { vendedorId: item.vendedorId, vendedorNombre: item.vendedorNombre, items: [] });
      }
      mapa.get(item.vendedorId)!.items.push(item);
    }
    return Array.from(mapa.values());
  });

  constructor() {
    effect(() => guardarEnStorage(this.items()));
  }

  agregar(nuevo: ItemCarrito): void {
    this.items.update((actuales) => {
      const existente = actuales.find((i) => i.presentacionId === nuevo.presentacionId);
      if (existente) {
        return actuales.map((i) =>
          i.presentacionId === nuevo.presentacionId ? { ...i, cantidad: i.cantidad + nuevo.cantidad } : i,
        );
      }
      return [...actuales, nuevo];
    });
  }

  actualizarCantidad(presentacionId: number, cantidad: number): void {
    if (cantidad <= 0) {
      this.quitar(presentacionId);
      return;
    }
    this.items.update((actuales) =>
      actuales.map((i) => (i.presentacionId === presentacionId ? { ...i, cantidad } : i)),
    );
  }

  quitar(presentacionId: number): void {
    this.items.update((actuales) => actuales.filter((i) => i.presentacionId !== presentacionId));
  }

  vaciar(): void {
    this.items.set([]);
  }
}

function cargarDeStorage(): ItemCarrito[] {
  try {
    const datos = localStorage.getItem(CLAVE_STORAGE);
    return datos ? JSON.parse(datos) : [];
  } catch {
    return [];
  }
}

function guardarEnStorage(items: ItemCarrito[]): void {
  try {
    localStorage.setItem(CLAVE_STORAGE, JSON.stringify(items));
  } catch {
    // Si falla (modo privado, storage lleno) simplemente no persiste - no rompe la app.
  }
}
