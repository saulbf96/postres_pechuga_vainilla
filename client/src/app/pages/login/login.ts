import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth';

@Component({
  imports: [FormsModule, RouterLink],
  selector: 'app-login',
  styleUrl: './login.css',
  templateUrl: './login.html',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly email = signal('');
  protected readonly password = signal('');
  protected readonly enviando = signal(false);
  protected readonly error = signal<string | null>(null);

  protected enviar(): void {
    this.error.set(null);

    if (!this.email().trim() || !this.password()) {
      this.error.set('Escribe tu correo y tu contraseña.');
      return;
    }

    this.enviando.set(true);
    this.auth.login({ email: this.email(), password: this.password() }).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.enviando.set(false);
        this.error.set(err.error?.mensaje ?? 'No se pudo iniciar sesión.');
      },
    });
  }
}
