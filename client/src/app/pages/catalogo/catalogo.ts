import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CatalogoService } from '../../core/services/catalogo';
import type { ProductoDto, VendedorDto } from '../../core/models/catalogo.model';

type FiltroVendedor = 'todo' | string; // 'todo' o el slug del vendedor (ej. 'vainilla')

@Component({
  imports: [],
  selector: 'app-catalogo',
  styleUrl: './catalogo.css',
  templateUrl: './catalogo.html',
})
export class Catalogo implements OnInit {
  private readonly catalogoService = inject(CatalogoService);

  protected readonly vendedores = signal<VendedorDto[]>([]);
  protected readonly productos = signal<ProductoDto[]>([]);
  protected readonly cargando = signal(true);
  protected readonly error = signal(false);
  protected readonly filtro = signal<FiltroVendedor>('todo');

  // Que presentacion (Chico/Grande/Pieza) esta elegida por producto, antes de pedir por WhatsApp.
  protected readonly presentacionElegida = signal<Record<number, number>>({});

  protected readonly productosFiltrados = computed(() => {
    const filtro = this.filtro();
    if (filtro === 'todo') {
      return this.productos();
    }
    const vendedor = this.vendedores().find((v) => v.slug === filtro);
    return vendedor ? this.productos().filter((p) => p.vendedorId === vendedor.id) : this.productos();
  });

  ngOnInit(): void {
    this.catalogoService.obtenerVendedores().subscribe({
      next: (vendedores) => this.vendedores.set(vendedores),
      error: () => this.error.set(true),
    });

    this.catalogoService.obtenerProductos().subscribe({
      next: (productos) => {
        this.productos.set(productos);

        // Por defecto, cada producto arranca con su primera presentacion elegida.
        const seleccion: Record<number, number> = {};
        for (const producto of productos) {
          if (producto.presentaciones.length > 0) {
            seleccion[producto.id] = producto.presentaciones[0].id;
          }
        }
        this.presentacionElegida.set(seleccion);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set(true);
        this.cargando.set(false);
      },
    });
  }

  protected cambiarFiltro(filtro: FiltroVendedor): void {
    this.filtro.set(filtro);
  }

  protected elegirPresentacion(productoId: number, presentacionId: number): void {
    this.presentacionElegida.update((actual) => ({ ...actual, [productoId]: presentacionId }));
  }

  protected presentacionActual(producto: ProductoDto) {
    const id = this.presentacionElegida()[producto.id];
    return producto.presentaciones.find((p) => p.id === id) ?? producto.presentaciones[0];
  }

  protected precioDesde(producto: ProductoDto): string {
    const min = Math.min(...producto.presentaciones.map((p) => p.precio));
    return min.toFixed(2);
  }

  // Colores de linea segun docs/PROYECTO.md: Vainilla = rosa suave, Pechuga = verde suave.
  protected tintePara(vendedorId: number): { fondo: string; texto: string } {
    const vendedor = this.vendedores().find((v) => v.id === vendedorId);
    return vendedor?.slug === 'pechuga'
      ? { fondo: '#E3EADC', texto: '#3F5E3A' }
      : { fondo: '#F3DDD6', texto: '#8F2A3F' };
  }

  // Arma el link wa.me con el mensaje ya escrito y codificado (nunca se manda sin codificar,
  // porque el texto tiene espacios y simbolos que romperian la URL).
  protected enlaceWhatsApp(producto: ProductoDto): string {
    const vendedor = this.vendedores().find((v) => v.id === producto.vendedorId);
    const presentacion = this.presentacionActual(producto);
    if (!vendedor || !presentacion) {
      return '#';
    }

    const mensaje =
      `Hola! Quiero pedir: ${producto.nombre} (${presentacion.nombre}) - $${presentacion.precio.toFixed(2)} - Cantidad: 1`;

    return `https://wa.me/${vendedor.whatsApp}?text=${encodeURIComponent(mensaje)}`;
  }
}
