import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '../services/auth';

// Mas estricto que panelGuard: solo Administrador (ni siquiera un Vendedor normal),
// para /panel/perfiles - dar de alta gente y roles es sensible.
export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.esperarSesionInicial().pipe(
    map(() => (auth.usuario()?.roles.includes('Administrador') ? true : router.createUrlTree(['/']))),
  );
};
