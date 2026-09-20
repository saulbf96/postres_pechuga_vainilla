import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PanelService } from '../../core/services/panel';
import { CatalogoService } from '../../core/services/catalogo';
import type { AdminProductoDto } from '../../core/models/panel.model';
import type { VendedorDto } from '../../core/models/catalogo.model';

interface FilaPresentacion {
  nombre: string;
  precio: number;
}

@Component({
  imports: [FormsModule, RouterLink],
  selector: 'app-panel-productos',
  styleUrl: './panel-productos.css',
  templateUrl: './panel-productos.html',
})
export class PanelProductos implements OnInit {
  private readonly panelService = inject(PanelService);
  private readonly catalogoService = inject(CatalogoService);

  protected readonly productos = signal<AdminProductoDto[]>([]);
  protected readonly vendedores = signal<VendedorDto[]>([]);
  protected readonly cargando = signal(true);
  protected readonly mostrarFormulario = signal(false);
  protected readonly error = signal<string | null>(null);

  // Formulario "crear producto"
  protected readonly nuevoVendedorId = signal<number | null>(null);
  protected readonly nuevoNombre = signal('');
  protected readonly nuevaDescripcion = signal('');
  protected readonly nuevoMaxPorDia = signal<number | null>(null);
  protected readonly nuevasPresentaciones = signal<FilaPresentacion[]>([{ nombre: '', precio: 0 }]);

  // Presentacion nueva por producto ya existente (id del producto -> fila en edicion)
  protected readonly nuevaPresentacionPorProducto = signal<Record<number, FilaPresentacion>>({});

  ngOnInit(): void {
    this.catalogoService.obtenerVendedores().subscribe((vendedores) => this.vendedores.set(vendedores));
    this.cargar();
  }

  private cargar(): void {
    this.cargando.set(true);
    this.panelService.obtenerProductos().subscribe({
      next: (productos) => {
        this.productos.set(productos);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false),
    });
  }

  protected agregarFilaPresentacion(): void {
    this.nuevasPresentaciones.update((actual) => [...actual, { nombre: '', precio: 0 }]);
  }

  protected quitarFilaPresentacion(index: number): void {
    this.nuevasPresentaciones.update((actual) => actual.filter((_, i) => i !== index));
  }

  protected actualizarFilaPresentacion(index: number, cambios: Partial<FilaPresentacion>): void {
    this.nuevasPresentaciones.update((actual) =>
      actual.map((fila, i) => (i === index ? { ...fila, ...cambios } : fila)),
    );
  }

  protected crearProducto(): void {
    this.error.set(null);

    if (!this.nuevoVendedorId() || !this.nuevoNombre().trim()) {
      this.error.set('Elige el vendedor y escribe un nombre.');
      return;
    }

    const presentaciones = this.nuevasPresentaciones().filter((p) => p.nombre.trim() && p.precio > 0);
    if (presentaciones.length === 0) {
      this.error.set('Agrega al menos una presentación con precio mayor a cero.');
      return;
    }

    this.panelService
      .crearProducto({
        vendedorId: this.nuevoVendedorId(),
        nombre: this.nuevoNombre(),
        descripcion: this.nuevaDescripcion() || null,
        alergenos: null,
        maxPorDia: this.nuevoMaxPorDia(),
        presentaciones,
      })
      .subscribe({
        next: () => {
          this.nuevoNombre.set('');
          this.nuevaDescripcion.set('');
          this.nuevoMaxPorDia.set(null);
          this.nuevasPresentaciones.set([{ nombre: '', precio: 0 }]);
          this.mostrarFormulario.set(false);
          this.cargar();
        },
        error: (err) => this.error.set(err.error?.mensaje ?? 'No se pudo crear el producto.'),
      });
  }

  protected cambiarActivo(producto: AdminProductoDto): void {
    this.panelService.cambiarActivoProducto(producto.id, !producto.activo).subscribe(() => this.cargar());
  }

  protected cambiarActivaPresentacion(presentacionId: number, activoActual: boolean): void {
    this.panelService.cambiarActivaPresentacion(presentacionId, !activoActual).subscribe(() => this.cargar());
  }

  protected filaNuevaPresentacion(productoId: number): FilaPresentacion {
    return this.nuevaPresentacionPorProducto()[productoId] ?? { nombre: '', precio: 0 };
  }

  protected actualizarNuevaPresentacion(productoId: number, cambios: Partial<FilaPresentacion>): void {
    this.nuevaPresentacionPorProducto.update((actual) => ({
      ...actual,
      [productoId]: { ...this.filaNuevaPresentacion(productoId), ...cambios },
    }));
  }

  protected agregarPresentacion(producto: AdminProductoDto): void {
    const fila = this.filaNuevaPresentacion(producto.id);
    if (!fila.nombre.trim() || fila.precio <= 0) {
      return;
    }

    this.panelService.agregarPresentacion(producto.id, fila).subscribe(() => {
      this.nuevaPresentacionPorProducto.update((actual) => {
        const copia = { ...actual };
        delete copia[producto.id];
        return copia;
      });
      this.cargar();
    });
  }
}
