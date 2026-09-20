import { Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth';

@Component({
  imports: [RouterLink],
  selector: 'app-panel-nav',
  styleUrl: './panel-nav.css',
  templateUrl: './panel-nav.html',
})
export class PanelNav {
  protected readonly auth = inject(AuthService);

  // Cual pestaña esta activa: 'pedidos' | 'productos' | 'puntos-entrega' | 'perfiles'
  readonly activa = input.required<string>();

  protected esAdmin(): boolean {
    return this.auth.usuario()?.roles.includes('Administrador') ?? false;
  }
}
