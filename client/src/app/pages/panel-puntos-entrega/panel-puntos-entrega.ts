import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PanelService } from '../../core/services/panel';
import { CatalogoService } from '../../core/services/catalogo';
import { PanelNav } from '../../shared/panel-nav/panel-nav';
import { DIAS_SEMANA } from '../../core/models/panel.model';
import type { AdminPuntoEntregaDto } from '../../core/models/panel.model';
import type { CategoriaDto } from '../../core/models/catalogo.model';

@Component({
  imports: [FormsModule, PanelNav],
  selector: 'app-panel-puntos-entrega',
  styleUrl: './panel-puntos-entrega.css',
  templateUrl: './panel-puntos-entrega.html',
})
export class PanelPuntosEntrega implements OnInit {
  private readonly panelService = inject(PanelService);
  private readonly catalogoService = inject(CatalogoService);

  protected readonly puntos = signal<AdminPuntoEntregaDto[]>([]);
  protected readonly categorias = signal<CategoriaDto[]>([]);
  protected readonly cargando = signal(true);
  protected readonly mostrarFormulario = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly diasSemana = DIAS_SEMANA;
  protected readonly tipos = ['Domicilio', 'Recoger', 'Campus'];

  // Formulario
  protected readonly nuevasCategorias = signal<Set<number>>(new Set());
  protected readonly nuevoNombre = signal('');
  protected readonly nuevoTipo = signal('Recoger');
  protected readonly nuevosDias = signal<Set<string>>(new Set());
  protected readonly nuevaHoraInicio = signal('10:00');
  protected readonly nuevaHoraFin = signal('18:00');
  protected readonly nuevosDiasAnticipacion = signal(1);
  protected readonly nuevaHoraLimite = signal('20:00');
  protected readonly nuevoCostoEnvio = signal(0);

  ngOnInit(): void {
    this.catalogoService.obtenerCategorias().subscribe((categorias) => this.categorias.set(categorias));
    this.cargar();
  }

  private cargar(): void {
    this.cargando.set(true);
    this.panelService.obtenerPuntosEntrega().subscribe({
      next: (puntos) => {
        this.puntos.set(puntos);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false),
    });
  }

  protected alternarDia(dia: string): void {
    this.nuevosDias.update((actual) => {
      const copia = new Set(actual);
      if (copia.has(dia)) {
        copia.delete(dia);
      } else {
        copia.add(dia);
      }
      return copia;
    });
  }

  protected alternarCategoria(categoriaId: number): void {
    this.nuevasCategorias.update((actual) => {
      const copia = new Set(actual);
      if (copia.has(categoriaId)) {
        copia.delete(categoriaId);
      } else {
        copia.add(categoriaId);
      }
      return copia;
    });
  }

  protected nombresCategorias(categoriaIds: number[]): string {
    return categoriaIds
      .map((id) => this.categorias().find((c) => c.id === id)?.nombre)
      .filter((nombre): nombre is string => !!nombre)
      .join(', ');
  }

  protected crear(): void {
    this.error.set(null);

    if (this.nuevasCategorias().size === 0 || !this.nuevoNombre().trim() || this.nuevosDias().size === 0) {
      this.error.set('Elige al menos una categoría, un nombre y al menos un día.');
      return;
    }

    this.panelService
      .crearPuntoEntrega({
        nombre: this.nuevoNombre(),
        tipo: this.nuevoTipo(),
        diasSemana: Array.from(this.nuevosDias()),
        horaInicio: this.nuevaHoraInicio(),
        horaFin: this.nuevaHoraFin(),
        diasAnticipacion: this.nuevosDiasAnticipacion(),
        horaLimitePedido: this.nuevaHoraLimite(),
        costoEnvio: this.nuevoCostoEnvio(),
        categoriaIds: Array.from(this.nuevasCategorias()),
      })
      .subscribe({
        next: () => {
          this.nuevoNombre.set('');
          this.nuevosDias.set(new Set());
          this.nuevasCategorias.set(new Set());
          this.mostrarFormulario.set(false);
          this.cargar();
        },
        error: (err) => this.error.set(err.error?.mensaje ?? 'No se pudo crear el punto de entrega.'),
      });
  }

  protected cambiarActivo(punto: AdminPuntoEntregaDto): void {
    this.panelService.cambiarActivoPuntoEntrega(punto.id, !punto.activo).subscribe(() => this.cargar());
  }
}
