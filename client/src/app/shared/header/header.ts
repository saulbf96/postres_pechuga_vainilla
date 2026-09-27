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

  // En vez de confirm() nativo del navegador (que se ve feo, con "localhost dice..."),
  // mostramos nuestro propio cuadro de confirmacion con el estilo de la marca.
  protected readonly mostrarConfirmacionSalir = signal(false);

  protected pedirConfirmacionSalir(): void {
    this.mostrarConfirmacionSalir.set(true);
  }

  protected cancelarSalir(): void {
    this.mostrarConfirmacionSalir.set(false);
  }

  protected confirmarSalir(): void {
    this.mostrarConfirmacionSalir.set(false);

    // El carrito es del navegador, no de la cuenta - si no lo vaciamos, la siguiente
    // persona que use este dispositivo veria lo que tu dejaste a medias.
    this.carrito.vaciar();

    // next Y error navegan - si la llamada al servidor fallara, no queremos que te quedes
    // atrapado en una pagina del panel a la que ya no deberias tener acceso.
    this.auth.logout().subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: () => this.router.navigateByUrl('/'),
    });
  }

  protected tieneAccesoPanel(): boolean {
    return this.auth.usuario()?.roles.includes('Administrador') ?? false;
  }
}
