import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PanelService } from '../../core/services/panel';
import { PanelNav } from '../../shared/panel-nav/panel-nav';
import type { AutorizacionAdminRequest, PerfilDto } from '../../core/models/panel.model';

type Rol = 'Administrador' | 'Cliente';
type TipoAccion = 'expirar' | 'restablecer' | 'desactivar' | 'reactivar';

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
  protected readonly nuevoRol = signal<Rol>('Cliente');

  // Cuadro de resultado al centro de la pantalla (ej. la contraseña temporal): no se queda
  // en la pagina, se cierra con "Entendido".
  protected readonly resultado = signal<{ titulo: string; mensaje: string; passwordTemporal?: string } | null>(null);
  protected readonly copiado = signal(false);

  // Cuadro para acciones delicadas: pide correo y contraseña de un administrador.
  protected readonly accionAdmin = signal<{ tipo: TipoAccion; perfil: PerfilDto } | null>(null);
  protected readonly emailAdmin = signal('');
  protected readonly passwordAdmin = signal('');
  protected readonly errorAccion = signal<string | null>(null);
  protected readonly procesandoAccion = signal(false);

  // Cuadro para editar usuario
  protected readonly edicion = signal<PerfilDto | null>(null);
  protected readonly editNombre = signal('');
  protected readonly editEmail = signal('');
  protected readonly editWhatsApp = signal('');
  protected readonly editRol = signal<Rol>('Cliente');
  protected readonly errorEdicion = signal<string | null>(null);
  protected readonly guardandoEdicion = signal(false);

  // Si la edicion cambia el rol, hay que pedir los datos de un administrador.
  protected readonly edicionCambiaRol = computed(() => {
    const perfil = this.edicion();
    return !!perfil && !perfil.roles.includes(this.editRol());
  });

  // Buscador y filtros de la lista
  protected readonly busqueda = signal('');
  protected readonly filtroRol = signal<'Todos' | Rol>('Todos');
  protected readonly mostrarInactivos = signal(false);

  // Se recalcula sola cada vez que cambian perfiles, busqueda o los filtros.
  protected readonly perfilesFiltrados = computed(() => {
    const texto = this.busqueda().trim().toLowerCase();
    const rol = this.filtroRol();
    const mostrarInactivos = this.mostrarInactivos();

    return this.perfiles().filter((perfil) => {
      const coincideActivo = mostrarInactivos || perfil.activo;
      const coincideRol = rol === 'Todos' || perfil.roles.includes(rol);
      const coincideTexto =
        !texto ||
        perfil.nombre.toLowerCase().includes(texto) ||
        perfil.email.toLowerCase().includes(texto) ||
        (perfil.whatsApp ?? '').includes(texto);
      return coincideActivo && coincideRol && coincideTexto;
    });
  });

  ngOnInit(): void {
    this.cargar();
  }

  // Filtra mientras escribes: solo digitos, maximo 10 - una letra simplemente no aparece.
  protected soloDiezDigitos(valor: string): string {
    return valor.replace(/\D/g, '').slice(0, 10);
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

  // Junta todos los problemas de una vez y dice exactamente que falta.
  private validarDatos(nombre: string, email: string, whatsApp: string, rol: Rol): string[] {
    const problemas: string[] = [];
    if (!nombre.trim()) {
      problemas.push('Escribe el nombre.');
    }
    if (!email.trim()) {
      problemas.push('Escribe el correo.');
    }
    // WhatsApp: obligatorio para clientes, opcional para administradores.
    if (rol === 'Cliente' && !whatsApp) {
      problemas.push('El WhatsApp es obligatorio para clientes.');
    } else if (whatsApp && whatsApp.length !== 10) {
      problemas.push('El WhatsApp debe tener exactamente 10 números.');
    }
    return problemas;
  }

  protected crearUsuario(): void {
    this.error.set(null);

    const whatsApp = this.nuevoWhatsApp();
    const problemas = this.validarDatos(this.nuevoNombre(), this.nuevoEmail(), whatsApp, this.nuevoRol());
    if (problemas.length > 0) {
      this.error.set(problemas.join(' '));
      return;
    }

    const nombre = this.nuevoNombre();
    this.panelService
      .crearUsuario({ nombre, email: this.nuevoEmail(), whatsApp: whatsApp || null, rol: this.nuevoRol() })
      .subscribe({
        next: (respuesta) => {
          this.mostrarResultado(
            'Usuario creado',
            `"${nombre}" ya puede entrar con esta contraseña temporal. Pásasela; al entrar se le pedirá una nueva.`,
            respuesta.passwordTemporal,
          );
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

  // ----- Resultado -----

  private mostrarResultado(titulo: string, mensaje: string, passwordTemporal?: string): void {
    this.copiado.set(false);
    this.resultado.set({ titulo, mensaje, passwordTemporal });
  }

  protected copiarPassword(password: string): void {
    navigator.clipboard?.writeText(password).then(() => this.copiado.set(true));
  }

  // ----- Editar -----

  protected abrirEdicion(perfil: PerfilDto): void {
    this.edicion.set(perfil);
    this.editNombre.set(perfil.nombre);
    this.editEmail.set(perfil.email);
    this.editWhatsApp.set(perfil.whatsApp ?? '');
    this.editRol.set(perfil.roles.includes('Administrador') ? 'Administrador' : 'Cliente');
    this.emailAdmin.set('');
    this.passwordAdmin.set('');
    this.errorEdicion.set(null);
  }

  protected guardarEdicion(): void {
    const perfil = this.edicion();
    if (!perfil) {
      return;
    }

    const whatsApp = this.editWhatsApp();
    const problemas = this.validarDatos(this.editNombre(), this.editEmail(), whatsApp, this.editRol());
    if (this.edicionCambiaRol()) {
      if (!this.emailAdmin().trim()) {
        problemas.push('Para cambiar el rol, escribe el correo del administrador.');
      }
      if (!this.passwordAdmin()) {
        problemas.push('Para cambiar el rol, escribe la contraseña del administrador.');
      }
    }
    if (problemas.length > 0) {
      this.errorEdicion.set(problemas.join(' '));
      return;
    }

    this.guardandoEdicion.set(true);
    this.panelService
      .editarUsuario(perfil.id, {
        nombre: this.editNombre(),
        email: this.editEmail(),
        whatsApp: whatsApp || null,
        rol: this.editRol(),
        emailAdmin: this.edicionCambiaRol() ? this.emailAdmin() : null,
        passwordAdmin: this.edicionCambiaRol() ? this.passwordAdmin() : null,
      })
      .subscribe({
        next: () => {
          this.guardandoEdicion.set(false);
          this.edicion.set(null);
          const cambioCorreo = this.editEmail().trim().toLowerCase() !== perfil.email.toLowerCase();
          this.mostrarResultado(
            'Cambios guardados',
            cambioCorreo
              ? `Se actualizó "${this.editNombre()}". Ojo: cambió su correo, avísale que ahora entra con ${this.editEmail()}.`
              : `Se actualizó "${this.editNombre()}".`,
          );
          this.cargar();
        },
        error: (err) => {
          this.guardandoEdicion.set(false);
          this.errorEdicion.set(err.error?.mensaje ?? 'No se pudieron guardar los cambios.');
        },
      });
  }

  // ----- Acciones con autorizacion de administrador -----

  protected abrirAccion(tipo: TipoAccion, perfil: PerfilDto): void {
    this.accionAdmin.set({ tipo, perfil });
    this.emailAdmin.set('');
    this.passwordAdmin.set('');
    this.errorAccion.set(null);
  }

  protected tituloAccion(tipo: TipoAccion): string {
    return {
      expirar: 'Expirar contraseña',
      restablecer: 'Restablecer contraseña',
      desactivar: 'Desactivar usuario',
      reactivar: 'Reactivar usuario',
    }[tipo];
  }

  protected confirmarAccion(): void {
    const accion = this.accionAdmin();
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

    const autorizacion: AutorizacionAdminRequest = { emailAdmin: this.emailAdmin(), passwordAdmin: this.passwordAdmin() };
    const nombre = accion.perfil.nombre;
    const alTerminar = (titulo: string, mensaje: string, passwordTemporal?: string) => {
      this.procesandoAccion.set(false);
      this.accionAdmin.set(null);
      this.mostrarResultado(titulo, mensaje, passwordTemporal);
      this.cargar();
    };
    const alFallar = (err: { error?: { mensaje?: string } }) => {
      this.procesandoAccion.set(false);
      this.errorAccion.set(err.error?.mensaje ?? 'No se pudo completar la acción.');
    };

    this.procesandoAccion.set(true);
    const id = accion.perfil.id;
    switch (accion.tipo) {
      case 'expirar':
        this.panelService.expirarPassword(id, autorizacion).subscribe({
          next: () => alTerminar('Contraseña expirada', `A "${nombre}" se le pedirá una contraseña nueva la próxima vez que entre.`),
          error: alFallar,
        });
        break;
      case 'restablecer':
        this.panelService.restablecerPassword(id, autorizacion).subscribe({
          next: (r) =>
            alTerminar('Contraseña restablecida', `Pásale esta contraseña temporal a "${nombre}"; al entrar se le pedirá una nueva.`, r.passwordTemporal),
          error: alFallar,
        });
        break;
      case 'desactivar':
        this.panelService.desactivarUsuario(id, autorizacion).subscribe({
          next: () => alTerminar('Usuario desactivado', `"${nombre}" ya no puede entrar. Sus pedidos se conservan y puedes reactivarlo cuando quieras.`),
          error: alFallar,
        });
        break;
      case 'reactivar':
        this.panelService.reactivarUsuario(id, autorizacion).subscribe({
          next: () => alTerminar('Usuario reactivado', `"${nombre}" ya puede volver a entrar.`),
          error: alFallar,
        });
        break;
    }
  }
}
