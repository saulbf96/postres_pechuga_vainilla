import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { panelGuard } from './core/guards/panel-guard';
import { adminGuard } from './core/guards/admin-guard';
import { Catalogo } from './pages/catalogo/catalogo';
import { Login } from './pages/login/login';
import { Registro } from './pages/registro/registro';
import { Carrito } from './pages/carrito/carrito';
import { Confirmar } from './pages/confirmar/confirmar';
import { MisPedidos } from './pages/mis-pedidos/mis-pedidos';
import { PanelPedidos } from './pages/panel-pedidos/panel-pedidos';
import { PanelProductos } from './pages/panel-productos/panel-productos';
import { PanelPuntosEntrega } from './pages/panel-puntos-entrega/panel-puntos-entrega';
import { PanelPerfiles } from './pages/panel-perfiles/panel-perfiles';

export const routes: Routes = [
  { path: '', component: Catalogo },
  { path: 'login', component: Login },
  { path: 'registro', component: Registro },
  { path: 'carrito', component: Carrito },
  { path: 'confirmar', component: Confirmar, canActivate: [authGuard] },
  { path: 'mis-pedidos', component: MisPedidos, canActivate: [authGuard] },
  { path: 'panel/pedidos', component: PanelPedidos, canActivate: [panelGuard] },
  { path: 'panel/productos', component: PanelProductos, canActivate: [panelGuard] },
  { path: 'panel/puntos-entrega', component: PanelPuntosEntrega, canActivate: [panelGuard] },
  { path: 'panel/perfiles', component: PanelPerfiles, canActivate: [adminGuard] },
];
