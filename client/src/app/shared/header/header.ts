import { Component, OnInit, effect, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth';
import { CarritoService } from '../../core/services/carrito';
import { PanelService } from '../../core/services/panel';

@Component({
  imports: [RouterLink],
  selector: 'app-header',
  styleUrl: './header.css',
  templateUrl: './header.html',
})
export class Header implements OnInit {
  private readonly router = inject(Router);
  private readonly panelService = inject(PanelService);
  protected readonly auth = inject(AuthService);
  protected readonly carrito = inject(CarritoService);

  protected readonly pedidosNuevos = signal(0);

  constructor() {
    // Cada vez que cambia quien esta logueado (login/logout), revisa si debe cargar el avisito.
    effect(() => {
      if (this.tieneAccesoPanel()) {
        this.cargarPedidosNuevos();
      } else {
        this.pedidosNuevos.set(0);
      }
    });
  }

  ngOnInit(): void {
    // Refresca el conteo cada 30s mientras la pestaña este abierta - el "aviso dentro del panel".
    setInterval(() => {
      if (this.tieneAccesoPanel()) {
        this.cargarPedidosNuevos();
      }
    }, 30000);
  }

  private cargarPedidosNuevos(): void {
    this.panelService.contarPedidosNuevos().subscribe({
      next: (cantidad) => this.pedidosNuevos.set(cantidad),
      error: () => this.pedidosNuevos.set(0),
    });
  }

  protected salir(): void {
    this.auth.logout().subscribe(() => this.router.navigateByUrl('/'));
  }

  protected tieneAccesoPanel(): boolean {
    const roles = this.auth.usuario()?.roles ?? [];
    return roles.includes('Administrador') || roles.includes('Vendedor');
  }
}
