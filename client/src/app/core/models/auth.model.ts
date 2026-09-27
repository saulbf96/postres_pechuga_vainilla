export interface UsuarioDto {
  id: string;
  nombre: string;
  email: string;
  whatsApp: string | null;
  roles: string[];
  // true = contraseña expirada/temporal: debe poner una nueva antes de usar la app
  debeCambiarPassword: boolean;
}

export interface RegistroRequest {
  nombre: string;
  email: string;
  whatsApp: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}
