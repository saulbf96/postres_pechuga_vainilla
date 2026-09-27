import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CatalogoService } from '../../core/services/catalogo';
import { CarritoService } from '../../core/services/carrito';
import type { ProductoDto, CategoriaDto } from '../../core/models/catalogo.model';

type FiltroCategoria = 'todo' | string; // 'todo' o el slug de la categoria (ej. 'vainilla')

// Numero de WhatsApp del negocio (dato pendiente de llenar, ver docs/PROYECTO.md).
const NUMERO_WHATSAPP_NEGOCIO = '5215500000000';

@Component({
  imports: [],
  selector: 'app-catalogo',
  styleUrl: './catalogo.css',
  templateUrl: './catalogo.html',
})
export class Catalogo implements OnInit {
  private readonly catalogoService = inject(CatalogoService);
  private readonly carritoService = inject(CarritoService);

  // Muestra un "Agregado" temporal en el boton cuando se agrega un producto al carrito.
  protected readonly agregadoRecien = signal<number | null>(null);

  protected readonly categorias = signal<CategoriaDto[]>([]);
  protected readonly productos = signal<ProductoDto[]>([]);
  protected readonly cargando = signal(true);
  protected readonly error = signal(false);
  protected readonly filtro = signal<FiltroCategoria>('todo');

  // Que presentacion (Chico/Grande/Pieza) esta elegida por producto, antes de pedir por WhatsApp.
  protected readonly presentacionElegida = signal<Record<number, number>>({});

  protected readonly productosFiltrados = computed(() => {
    const filtro = this.filtro();
    if (filtro === 'todo') {
      return this.productos();
    }
    const categoria = this.categorias().find((c) => c.slug === filtro);
    return categoria ? this.productos().filter((p) => p.categoriaId === categoria.id) : this.productos();
  });

  ngOnInit(): void {
    this.catalogoService.obtenerCategorias().subscribe({
      next: (categorias) => this.categorias.set(categorias),
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

  protected cambiarFiltro(filtro: FiltroCategoria): void {
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

  // Colores segun la categoria del producto (salen de la tabla Categorias, ver docs/DISENO.md).
  protected tintePara(categoriaId: number): { fondo: string; texto: string } {
    const categoria = this.categorias().find((c) => c.id === categoriaId);
    return categoria
      ? { fondo: categoria.colorSuave, texto: categoria.color }
      : { fondo: '#F3DDD6', texto: '#8F2A3F' };
  }

  // Arma el link wa.me con el mensaje ya escrito y codificado (nunca se manda sin codificar,
  // porque el texto tiene espacios y simbolos que romperian la URL).
  protected enlaceWhatsApp(producto: ProductoDto): string {
    const presentacion = this.presentacionActual(producto);
    if (!presentacion) {
      return '#';
    }

    const mensaje =
      `Hola! Quiero pedir: ${producto.nombre} (${presentacion.nombre}) - $${presentacion.precio.toFixed(2)} - Cantidad: 1`;

    return `https://wa.me/${NUMERO_WHATSAPP_NEGOCIO}?text=${encodeURIComponent(mensaje)}`;
  }

  protected agregarAlCarrito(producto: ProductoDto): void {
    const presentacion = this.presentacionActual(producto);
    if (!presentacion) {
      return;
    }

    this.carritoService.agregar({
      categoriaId: producto.categoriaId,
      categoriaNombre: producto.categoriaNombre,
      productoId: producto.id,
      productoNombre: producto.nombre,
      presentacionId: presentacion.id,
      presentacionNombre: presentacion.nombre,
      precioUnitario: presentacion.precio,
      cantidad: 1,
    });

    this.agregadoRecien.set(producto.id);
    setTimeout(() => this.agregadoRecien.set(null), 1500);
  }
}
