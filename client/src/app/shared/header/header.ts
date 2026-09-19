import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth';
import { CarritoService } from '../../core/services/carrito';

@Component({
  imports: [RouterLink],
  selector: 'app-header',
  styleUrl: './header.css',
  templateUrl: './header.html',
})
export class Header {
  private readonly router = inject(Router);
  protected readonly auth = inject(AuthService);
  protected readonly carrito = inject(CarritoService);

  protected salir(): void {
    this.auth.logout().subscribe(() => this.router.navigateByUrl('/'));
  }
}
