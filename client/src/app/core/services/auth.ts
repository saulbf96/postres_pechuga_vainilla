import { HttpClient } from '@angular/common/http';
import { Service, inject, signal } from '@angular/core';
import { catchError, Observable, of, shareReplay, tap } from 'rxjs';
import type { LoginRequest, RegistroRequest, UsuarioDto } from '../models/auth.model';

@Service()
export class AuthService {
  private readonly http = inject(HttpClient);

  // Quien esta logueado ahorita (null = nadie). El header y los guards leen esto.
  readonly usuario = signal<UsuarioDto | null>(null);

  // Se dispara una sola vez (shareReplay) para preguntarle a la API "quien soy" al arrancar
  // la app, usando la cookie que ya pueda existir de una sesion anterior.
  private readonly sesionInicial$ = this.http.get<UsuarioDto>('/api/v1/auth/yo').pipe(
    tap((usuario) => this.usuario.set(usuario)),
    catchError(() => {
      this.usuario.set(null);
      return of(null);
    }),
    shareReplay(1),
  );

  esperarSesionInicial(): Observable<UsuarioDto | null> {
    return this.sesionInicial$;
  }

  login(request: LoginRequest): Observable<UsuarioDto> {
    return this.http
      .post<UsuarioDto>('/api/v1/auth/login', request)
      .pipe(tap((usuario) => this.usuario.set(usuario)));
  }

  registro(request: RegistroRequest): Observable<UsuarioDto> {
    return this.http
      .post<UsuarioDto>('/api/v1/auth/registro', request)
      .pipe(tap((usuario) => this.usuario.set(usuario)));
  }

  logout(): Observable<void> {
    return this.http
      .post<void>('/api/v1/auth/logout', {})
      .pipe(tap(() => this.usuario.set(null)));
  }
}
