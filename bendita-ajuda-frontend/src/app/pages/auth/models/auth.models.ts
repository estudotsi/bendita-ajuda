export interface AuthCredentials {
  email: string;
  senha: string;
}

export interface RegisterRequest extends AuthCredentials {
  nome: string;
}

export interface MessageResponse {
  mensagem: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
}

export interface GoogleLoginRequest {
  idToken: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  novaSenha: string;
}

export type UserRole = 'Admin' | 'Operador' | 'Prestador' | 'Cliente';

export interface AccessTokenPayload {
  exp?: number;
  role?: string | string[];
  roles?: string | string[];
  [claim: string]: string | string[] | number | undefined;
}
