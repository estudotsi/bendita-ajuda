import { Injectable, computed, signal } from '@angular/core';

/**
 * Controle simples de "estar dentro" do app.
 * Por enquanto é só um sinal em memória; quando o backend existir,
 * `login`/`logout` passam a chamar a API e guardar o token.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly loggedIn = signal(false);

  readonly isLoggedIn = computed(() => this.loggedIn());

  /** Login simulado (apenas para testes enquanto não há backend). */
  loginForTesting(): void {
    this.loggedIn.set(true);
  }

  logout(): void {
    this.loggedIn.set(false);
  }
}
