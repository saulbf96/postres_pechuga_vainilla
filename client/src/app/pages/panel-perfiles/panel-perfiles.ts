import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PanelService } from '../../core/services/panel';
import { CatalogoService } from '../../core/services/catalogo';
import { PanelNav } from '../../shared/panel-nav/panel-nav';
import { DIAS_SEMANA } from '../../core/models/panel.model';
import type { NuevaAsignacionRequest, PerfilDto } from '../../core/models/panel.model';
import type { VendedorDto } from '../../core/models/catalogo.model';

interface FilaAsignacion {
  vendedorId: number | null;
  dias: Set<string>;
  horaInicio: string;
  horaFin: string;
}

@Component({
  imports: [FormsModule, PanelNav],
  selector: 'app-panel-perfiles',
  styleUrl: './panel-perfiles.css',
  templateUrl: './panel-perfiles.html',
})
export class PanelPerfiles implements OnInit {
  private readonly panelService = inject(PanelService);
  private readonly catalogoService = inject(CatalogoService);

  protected readonly perfiles = signal<PerfilDto[]>([]);
  protected readonly vendedores = signal<VendedorDto[]>([]);
  protected readonly cargando = signal(true);
  protected readonly mostrarFormulario = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly diasSemana = DIAS_SEMANA;

  // Formulario "nuevo usuario"
  protected readonly nuevoNombre = signal('');
  protected readonly nuevoEmail = signal('');
  protected readonly nuevoWhatsApp = signal('');
  protected readonly nuevoPassword = signal('');
  protected readonly nuevoRol = signal<'Administrador' | 'Vendedor'>('Vendedor');
  protected readonly nuevasAsignaciones = signal<FilaAsignacion[]>([]);

  ngOnInit(): void {
    this.catalogoService.obtenerVendedores().subscribe((vendedores) => this.vendedores.set(vendedores));
    this.cargar();
  }

  private cargar(): void {
    this.cargando.set(true);
    this.panelService.obtenerPerfiles().subscribe({
      next: (perfiles) => {
        this.perfiles.set(perfiles);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false),
    });
  }

  protected agregarFilaAsignacion(): void {
    this.nuevasAsignaciones.update((actual) => [
      ...actual,
      { vendedorId: null, dias: new Set(), horaInicio: '09:00', horaFin: '18:00' },
    ]);
  }

  protected quitarFilaAsignacion(index: number): void {
    this.nuevasAsignaciones.update((actual) => actual.filter((_, i) => i !== index));
  }

  protected alternarDiaFila(index: number, dia: string): void {
    this.nuevasAsignaciones.update((actual) =>
      actual.map((fila, i) => {
        if (i !== index) return fila;
        const dias = new Set(fila.dias);
        dias.has(dia) ? dias.delete(dia) : dias.add(dia);
        return { ...fila, dias };
      }),
    );
  }

  protected actualizarFilaAsignacion(index: number, cambios: Partial<FilaAsignacion>): void {
    this.nuevasAsignaciones.update((actual) => actual.map((fila, i) => (i === index ? { ...fila, ...cambios } : fila)));
  }

  protected crearUsuario(): void {
    this.error.set(null);

    if (!this.nuevoNombre().trim() || !this.nuevoEmail().trim() || this.nuevoPassword().length < 8) {
      this.error.set('Llena nombre, correo y una contraseña de al menos 8 caracteres.');
      return;
    }

    const asignaciones: NuevaAsignacionRequest[] = [];
    if (this.nuevoRol() === 'Vendedor') {
      for (const fila of this.nuevasAsignaciones()) {
        if (fila.vendedorId && fila.dias.size > 0) {
          asignaciones.push({
            vendedorId: fila.vendedorId,
            diasSemana: Array.from(fila.dias),
            horaInicio: fila.horaInicio,
            horaFin: fila.horaFin,
          });
        }
      }
    }

    this.panelService
      .crearUsuario({
        nombre: this.nuevoNombre(),
        email: this.nuevoEmail(),
        whatsApp: this.nuevoWhatsApp() || null,
        password: this.nuevoPassword(),
        rol: this.nuevoRol(),
        asignaciones,
      })
      .subscribe({
        next: () => {
          this.nuevoNombre.set('');
          this.nuevoEmail.set('');
          this.nuevoWhatsApp.set('');
          this.nuevoPassword.set('');
          this.nuevasAsignaciones.set([]);
          this.mostrarFormulario.set(false);
          this.cargar();
        },
        error: (err) => this.error.set(err.error?.mensaje ?? 'No se pudo crear el usuario.'),
      });
  }

  protected cambiarActivaAsignacion(asignacionId: number, activoActual: boolean): void {
    this.panelService.cambiarActivaAsignacion(asignacionId, !activoActual).subscribe(() => this.cargar());
  }
}
