import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth';

// Pantalla para quien tiene la contraseña expirada o temporal (creado/restablecido desde el Panel).
@Component({
  imports: [FormsModule],
  selector: 'app-cambiar-password',
  templateUrl: './cambiar-password.html',
})
export class CambiarPassword implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly passwordNueva = signal('');
  protected readonly passwordConfirmar = signal('');
  protected readonly mostrarPassword = signal(false);
  protected readonly enviando = signal(false);
  protected readonly errores = signal<string[]>([]);

  ngOnInit(): void {
    // Sin sesion no hay nada que cambiar; si ya no la debe cambiar, no tiene que estar aqui.
    this.auth.esperarSesionInicial().subscribe(() => {
      const usuario = this.auth.usuario();
      if (!usuario) {
        this.router.navigate(['/login']);
      } else if (!usuario.debeCambiarPassword) {
        this.irADestino();
      }
    });
  }

  protected guardar(): void {
    const nueva = this.passwordNueva();

    // Junta todos los problemas de una vez (no solo el primero).
    const problemas: string[] = [];
    if (nueva.length < 8) {
      problemas.push('La contraseña debe tener al menos 8 caracteres.');
    }
    if (!/\d/.test(nueva)) {
      problemas.push('La contraseña debe tener al menos un número.');
    }
    if (!/[A-Z]/.test(nueva)) {
      problemas.push('La contraseña debe tener al menos una mayúscula.');
    }
    if (nueva !== this.passwordConfirmar()) {
      problemas.push('Las dos contraseñas no coinciden.');
    }
    this.errores.set(problemas);
    if (problemas.length > 0) {
      return;
    }

    this.enviando.set(true);
    this.auth.cambiarPasswordExpirada(nueva).subscribe({
      next: () => this.irADestino(),
      error: (err) => {
        this.enviando.set(false);
        this.errores.set([err.error?.mensaje ?? 'No se pudo cambiar la contraseña.']);
      },
    });
  }

  private irADestino(): void {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/';
    this.router.navigateByUrl(returnUrl);
  }
}
