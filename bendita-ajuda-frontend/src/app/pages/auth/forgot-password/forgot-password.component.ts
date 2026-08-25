import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: false,
  templateUrl: './forgot-password.component.html',
})
export class ForgotPasswordComponent {
  private readonly formBuilder = inject(FormBuilder);

  protected feedback = '';
  protected error = '';

  protected readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  constructor(
    private readonly authService: AuthService,
  ) {}

  protected submit(): void {
    this.feedback = '';
    this.error = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error = 'Informe um e-mail valido.';
      return;
    }

    this.authService.forgotPassword(this.form.controls.email.value || '').subscribe({
      next: (response) => {
        localStorage.setItem('pendingPasswordResetEmail', this.form.controls.email.value || '');
        this.feedback = response || 'Se o e-mail existir, enviaremos as instrucoes.';
      },
      error: (error) => {
        this.error = this.getErrorMessage(error, 'Nao foi possivel enviar as instrucoes agora.');
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
