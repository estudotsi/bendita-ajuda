import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-confirm-email',
  standalone: false,
  templateUrl: './confirm-email.component.html',
})
export class ConfirmEmailComponent implements OnInit {
  protected feedback = '';
  protected error = '';
  protected showResetPasswordOption = false;
  protected resetPasswordQueryParams: { email: string; token: string } | null = null;

  constructor(
    private readonly authService: AuthService,
    private readonly route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    this.authService.logout();

    const email = this.route.snapshot.queryParamMap.get('email');
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!email || !token) {
      this.error = 'Link de confirmacao invalido ou incompleto.';
      return;
    }

    this.resetPasswordQueryParams = { email, token };

    this.authService.confirmEmail(email, token).subscribe({
      next: (response) => {
        localStorage.removeItem('pendingPasswordResetEmail');
        this.feedback = response || 'E-mail confirmado com sucesso.';
      },
      error: (error) => {
        this.error = this.getErrorMessage(error, 'Nao foi possivel confirmar seu e-mail.');
        this.showResetPasswordOption = true;
      },
    });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse && typeof error.error === 'string') {
      return error.error;
    }

    if (error instanceof HttpErrorResponse && error.error?.mensagem) {
      return error.error.mensagem;
    }

    return fallback;
  }
}
