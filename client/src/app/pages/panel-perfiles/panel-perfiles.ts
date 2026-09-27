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
  protected readonly nuevoRol = signal<'Administrador' | 'Cliente'>('Cliente');

  // Aviso verde de exito (ej. la contraseña temporal del usuario recien creado).
  protected readonly aviso = signal<string | null>(null);

  // Cuadro para expirar/restablecer contraseña: pide correo y contraseña de un administrador.
  protected readonly accionPassword = signal<{ tipo: 'expirar' | 'restablecer'; perfil: PerfilDto } | null>(null);
  protected readonly emailAdmin = signal('');
  protected readonly passwordAdmin = signal('');
  protected readonly errorAccion = signal<string | null>(null);
  protected readonly procesandoAccion = signal(false);

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
    this.aviso.set(null);

    // Junta todos los problemas de una vez y dice exactamente que falta.
    const problemas: string[] = [];
    if (!this.nuevoNombre().trim()) {
      problemas.push('Escribe el nombre.');
    }
    if (!this.nuevoEmail().trim()) {
      problemas.push('Escribe el correo.');
    }
    // WhatsApp: obligatorio para clientes, opcional para administradores.
    const whatsApp = this.nuevoWhatsApp();
    if (this.nuevoRol() === 'Cliente' && !whatsApp) {
      problemas.push('El WhatsApp es obligatorio para clientes.');
    } else if (whatsApp && whatsApp.length !== 10) {
      problemas.push('El WhatsApp debe tener exactamente 10 números.');
    }
    if (problemas.length > 0) {
      this.error.set(problemas.join(' '));
      return;
    }

    const nombre = this.nuevoNombre();
    this.panelService
      .crearUsuario({
        nombre,
        email: this.nuevoEmail(),
        whatsApp: whatsApp || null,
        rol: this.nuevoRol(),
      })
      .subscribe({
        next: (respuesta) => {
          this.aviso.set(`Usuario "${nombre}" creado. Su contraseña temporal es: ${respuesta.passwordTemporal} — pásasela; al entrar se le pedirá una nueva.`);
          this.nuevoNombre.set('');
          this.nuevoEmail.set('');
          this.nuevoWhatsApp.set('');
          this.nuevoRol.set('Cliente');
          this.mostrarFormulario.set(false);
          this.cargar();
        },
        error: (err) => this.error.set(err.error?.mensaje ?? 'No se pudo crear el usuario.'),
      });
  }

  protected abrirAccionPassword(tipo: 'expirar' | 'restablecer', perfil: PerfilDto): void {
    this.accionPassword.set({ tipo, perfil });
    this.emailAdmin.set('');
    this.passwordAdmin.set('');
    this.errorAccion.set(null);
    this.aviso.set(null);
  }

  protected cerrarAccionPassword(): void {
    this.accionPassword.set(null);
  }

  protected confirmarAccionPassword(): void {
    const accion = this.accionPassword();
    if (!accion) {
      return;
    }

    const problemas: string[] = [];
    if (!this.emailAdmin().trim()) {
      problemas.push('Escribe el correo del administrador.');
    }
    if (!this.passwordAdmin()) {
      problemas.push('Escribe la contraseña del administrador.');
    }
    if (problemas.length > 0) {
      this.errorAccion.set(problemas.join(' '));
      return;
    }

    const autorizacion = { emailAdmin: this.emailAdmin(), passwordAdmin: this.passwordAdmin() };
    const alTerminar = (mensaje: string) => {
      this.procesandoAccion.set(false);
      this.accionPassword.set(null);
      this.aviso.set(mensaje);
      this.cargar();
    };
    const alFallar = (err: { error?: { mensaje?: string } }) => {
      this.procesandoAccion.set(false);
      this.errorAccion.set(err.error?.mensaje ?? 'No se pudo completar la acción.');
    };

    this.procesandoAccion.set(true);
    if (accion.tipo === 'expirar') {
      this.panelService.expirarPassword(accion.perfil.id, autorizacion).subscribe({
        next: () => alTerminar(`Listo. A "${accion.perfil.nombre}" se le pedirá una contraseña nueva la próxima vez que entre.`),
        error: alFallar,
      });
    } else {
      this.panelService.restablecerPassword(accion.perfil.id, autorizacion).subscribe({
        next: (respuesta) =>
          alTerminar(`Contraseña de "${accion.perfil.nombre}" restablecida. La temporal es: ${respuesta.passwordTemporal} — pásasela; al entrar se le pedirá una nueva.`),
        error: alFallar,
      });
    }
  }
}
