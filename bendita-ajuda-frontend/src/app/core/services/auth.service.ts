import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { LoggedUser } from '../models';

const API = '/api/auth';

/** Resposta do "confirmar": ou a pessoa entrou, ou falta o nome (primeiro acesso). */
export type ConfirmResult = LoggedUser | { precisaNome: true };

/**
 * "Entrar com celular". A sessão fica num cookie HttpOnly gravado pela API;
 * aqui só guardamos quem está logado para a tela saber.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly current = signal<LoggedUser | null>(null);
  private readonly checked = signal(false);

  readonly user = this.current.asReadonly();
  readonly isLoggedIn = computed(() => this.current() !== null);
  readonly isAdmin = computed(() => this.current()?.papel === 'Admin');
  /** false enquanto ainda não sabemos se o cookie vale (evita piscar "Entrar"). */
  readonly isChecked = this.checked.asReadonly();

  constructor() {
    // Ao abrir o site, o cookie (se houver) diz quem está logado.
    this.http.get<LoggedUser>(`${API}/eu`).subscribe({
      next: (user) => {
        this.current.set(user);
        this.checked.set(true);
      },
      error: () => this.checked.set(true),
    });
  }

  sendCode(phone: string): Observable<void> {
    return this.http.post<void>(`${API}/celular/enviar-codigo`, { celular: phone });
  }

  confirmCode(phone: string, code: string, name?: string): Observable<ConfirmResult> {
    return this.http
      .post<ConfirmResult>(`${API}/celular/confirmar`, { celular: phone, codigo: code, nome: name || null })
      .pipe(
        tap((result) => {
          if (!('precisaNome' in result)) this.current.set(result);
        }),
      );
  }

  /** Depois de terminar o cadastro de prestador, sem precisar perguntar à API de novo. */
  markAsProvider(): void {
    this.current.update((user) => (user ? { ...user, ehPrestador: true } : user));
  }

  logout(): void {
    this.current.set(null);
    this.http.post<void>(`${API}/sair`, {}).subscribe({ error: () => {} });
  }
}

/** Mensagem amigável a partir do erro da API ({ mensagem }). */
export function apiErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 0) return 'Sem conexão com a internet. Confira e tente de novo.';
    const message = error.error?.mensagem;
    if (typeof message === 'string' && message) return message;
  }
  return 'Algo deu errado. Tente de novo em alguns minutos.';
}
