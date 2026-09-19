import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth';

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

  protected enviar(): void {
    this.errores.set([]);

    if (!this.nombre().trim() || !this.email().trim() || !this.whatsApp().trim() || !this.password()) {
      this.errores.set(['Llena todos los campos.']);
      return;
    }

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
}
