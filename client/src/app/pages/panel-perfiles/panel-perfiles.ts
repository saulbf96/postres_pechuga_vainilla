import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PanelService } from '../../core/services/panel';
import { PanelNav } from '../../shared/panel-nav/panel-nav';
import type { PerfilDto } from '../../core/models/panel.model';

@Component({
  imports: [FormsModule, PanelNav],
  selector: 'app-panel-perfiles',
  styleUrl: './panel-perfiles.css',
  templateUrl: './panel-perfiles.html',
})
export class PanelPerfiles implements OnInit {
  private readonly panelService = inject(PanelService);

  protected readonly perfiles = signal<PerfilDto[]>([]);
  protected readonly cargando = signal(true);
  protected readonly mostrarFormulario = signal(false);
  protected readonly error = signal<string | null>(null);

  // Formulario "nuevo usuario"
  protected readonly nuevoNombre = signal('');
  protected readonly nuevoEmail = signal('');
  protected readonly nuevoWhatsApp = signal('');
  protected readonly nuevoPassword = signal('');
  protected readonly nuevoRol = signal<'Administrador' | 'Cliente'>('Cliente');

  // Buscador y filtro de la lista
  protected readonly busqueda = signal('');
  protected readonly filtroRol = signal<'Todos' | 'Administrador' | 'Cliente'>('Todos');

  // Se recalcula sola cada vez que cambian perfiles, busqueda o filtroRol.
  protected readonly perfilesFiltrados = computed(() => {
    const texto = this.busqueda().trim().toLowerCase();
    const rol = this.filtroRol();

    return this.perfiles().filter((perfil) => {
      const coincideRol = rol === 'Todos' || perfil.roles.includes(rol);
      const coincideTexto =
        !texto ||
        perfil.nombre.toLowerCase().includes(texto) ||
        perfil.email.toLowerCase().includes(texto) ||
        (perfil.whatsApp ?? '').includes(texto);
      return coincideRol && coincideTexto;
    });
  });

  ngOnInit(): void {
    this.cargar();
  }

  // Filtra mientras escribes: solo digitos, maximo 10 - una letra simplemente no aparece.
  protected onWhatsAppChange(valor: string): void {
    this.nuevoWhatsApp.set(valor.replace(/\D/g, '').slice(0, 10));
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

  protected crearUsuario(): void {
    this.error.set(null);

    if (!this.nuevoNombre().trim() || !this.nuevoEmail().trim() || this.nuevoPassword().length < 8) {
      this.error.set('Llena nombre, correo y una contraseña de al menos 8 caracteres.');
      return;
    }

    this.panelService
      .crearUsuario({
        nombre: this.nuevoNombre(),
        email: this.nuevoEmail(),
        whatsApp: this.nuevoWhatsApp() || null,
        password: this.nuevoPassword(),
        rol: this.nuevoRol(),
      })
      .subscribe({
        next: () => {
          this.nuevoNombre.set('');
          this.nuevoEmail.set('');
          this.nuevoWhatsApp.set('');
          this.nuevoPassword.set('');
          this.nuevoRol.set('Cliente');
          this.mostrarFormulario.set(false);
          this.cargar();
        },
        error: (err) => this.error.set(err.error?.mensaje ?? 'No se pudo crear el usuario.'),
      });
  }
}
