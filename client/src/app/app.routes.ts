import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { Catalogo } from './pages/catalogo/catalogo';
import { Login } from './pages/login/login';
import { Registro } from './pages/registro/registro';
import { Carrito } from './pages/carrito/carrito';
import { Confirmar } from './pages/confirmar/confirmar';
import { MisPedidos } from './pages/mis-pedidos/mis-pedidos';

export const routes: Routes = [
  { path: '', component: Catalogo },
  { path: 'login', component: Login },
  { path: 'registro', component: Registro },
  { path: 'carrito', component: Carrito },
  { path: 'confirmar', component: Confirmar, canActivate: [authGuard] },
  { path: 'mis-pedidos', component: MisPedidos, canActivate: [authGuard] },
];
