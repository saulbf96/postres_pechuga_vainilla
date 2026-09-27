import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth';

const REGEX_CORREO = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const REGEX_DIEZ_DIGITOS = /^\d{10}$/;

@Component({
  imports: [FormsModule, RouterLink],
  selector: 'app-registro',
  styleUrl: './registro.css',
  templateUrl: './registro.html',
})
export class Registro {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly nombre = signal('');
  protected readonly email = signal('');
  protected readonly whatsApp = signal('');
  protected readonly password = signal('');
  protected readonly enviando = signal(false);
  protected readonly errores = signal<string[]>([]);

  // Filtra mientras escribes: quita cualquier cosa que no sea digito y corta en 10.
  // Asi una letra simplemente no aparece, en vez de dejarte escribirla y avisar hasta enviar.
  protected onWhatsAppChange(valor: string): void {
    this.whatsApp.set(valor.replace(/\D/g, '').slice(0, 10));
  }

  protected enviar(): void {
    const errores = this.validar();

    if (errores.length > 0) {
      this.errores.set(errores);
      return;
    }

    this.errores.set([]);
    this.enviando.set(true);
    this.auth
      .registro({
        nombre: this.nombre(),
        email: this.email(),
        whatsApp: this.whatsApp(),
        password: this.password(),
      })
      .subscribe({
        next: () => this.router.navigateByUrl('/'),
        error: (err) => {
          this.enviando.set(false);
          const erroresApi: string[] | undefined = err.error?.errores;
          this.errores.set(erroresApi?.length ? erroresApi : ['No se pudo crear la cuenta.']);
        },
      });
  }

  // Junta TODOS los problemas encontrados (no solo el primero), asi el usuario los corrige
  // de una vez en lugar de ir descubriendolos uno por uno en varios intentos.
  private validar(): string[] {
    const errores: string[] = [];
    const nombre = this.nombre().trim();
    const email = this.email().trim();
    const whatsApp = this.whatsApp().trim();
    const password = this.password();

    if (!nombre) {
      errores.push('Escribe tu nombre.');
    } else if (/\d/.test(nombre)) {
      errores.push('El nombre no debe tener números.');
    }

    if (!email) {
      errores.push('Escribe tu correo.');
    } else if (!REGEX_CORREO.test(email)) {
      errores.push('El correo no es válido (debe verse como nombre@dominio.com).');
    }

    if (!whatsApp) {
      errores.push('Escribe tu WhatsApp.');
    } else if (!REGEX_DIEZ_DIGITOS.test(whatsApp)) {
      errores.push('El WhatsApp debe tener exactamente 10 números, sin espacios ni letras (ej. 5512345678).');
    }

    // Misma politica que el servidor (Program.cs): 8 caracteres, una mayuscula, un numero.
    // Revisarlo aqui evita el viaje al servidor solo para enterarte del mismo error.
    if (!password) {
      errores.push('Escribe una contraseña.');
    } else {
      if (password.length < 8) {
        errores.push('La contraseña debe tener al menos 8 caracteres.');
      }
      if (!/[A-Z]/.test(password)) {
        errores.push('La contraseña debe incluir al menos una letra mayúscula.');
      }
      if (!/\d/.test(password)) {
        errores.push('La contraseña debe incluir al menos un número.');
      }
    }

    return errores;
  }
}
