export interface UsuarioDto {
  id: string;
  nombre: string;
  email: string;
  whatsApp: string | null;
  roles: string[];
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
