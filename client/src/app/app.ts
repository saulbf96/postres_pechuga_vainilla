import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Header } from './shared/header/header';
import { AuthService } from './core/services/auth';

@Component({
  imports: [RouterOutlet, Header],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {
  private readonly auth = inject(AuthService);

  constructor() {
    // Se pregunta "quien soy" una sola vez al arrancar la app (sin importar la ruta),
    // para que el header sepa desde el principio si ya hay sesion (cookie existente).
    this.auth.esperarSesionInicial().subscribe();
  }
}
