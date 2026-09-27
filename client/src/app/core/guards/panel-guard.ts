import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '../services/auth';

// Solo deja entrar a /panel/* a quien tenga rol Administrador.
// A un Cliente normal lo manda al catalogo (no a /login - ya tiene sesion, solo no tiene permiso).
export const panelGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.esperarSesionInicial().pipe(
    map(() => {
      const usuario = auth.usuario();
      if (usuario?.debeCambiarPassword) {
        return router.createUrlTree(['/cambiar-password']);
      }
      return usuario?.roles.includes('Administrador') ? true : router.createUrlTree(['/']);
    }),
  );
};
