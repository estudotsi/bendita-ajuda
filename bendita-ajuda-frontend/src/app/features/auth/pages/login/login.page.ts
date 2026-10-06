import { Component, computed, inject, input } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

/**
 * Página "Entrar" (placeholder).
 * Recebe `?voltarPara=/caminho` para levar a pessoa de volta depois de entrar.
 */
@Component({
  selector: 'app-login-page',
  standalone: false,
  templateUrl: './login.page.html',
  styleUrl: './login.page.scss',
})
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  /** Vem da query string (?voltarPara=...). */
  readonly voltarPara = input<string>();

  /** Só aceita caminhos internos, para não redirecionar para outro site. */
  private readonly returnUrl = computed(() => {
    const url = this.voltarPara();
    return url && url.startsWith('/') && !url.startsWith('//') ? url : '/';
  });

  protected readonly hasReturn = computed(() => this.returnUrl() !== '/');

  protected enterForTesting(): void {
    this.auth.loginForTesting();
    this.router.navigateByUrl(this.returnUrl());
  }
}
