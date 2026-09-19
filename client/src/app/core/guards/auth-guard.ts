import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '../services/auth';

// Protege rutas como /confirmar y /mis-pedidos: si no hay sesion, manda a /login
// y recuerda a donde querias ir (returnUrl) para regresarte ahi despues de iniciar sesion.
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  // esperarSesionInicial() solo garantiza que ya se resolvio la consulta inicial (evita la
  // carrera al recargar la pagina). El estado real de "quien esta logueado ahora" siempre
  // se lee de la señal auth.usuario(), que login()/registro()/logout() si mantienen al dia.
  return auth.esperarSesionInicial().pipe(
    map(() => (auth.usuario() ? true : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } }))),
  );
};
