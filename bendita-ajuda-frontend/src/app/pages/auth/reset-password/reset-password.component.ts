import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: false,
  templateUrl: './reset-password.component.html',
})
export class ResetPasswordComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);

  protected feedback = '';
  protected error = '';
  protected showPassword = false;

  protected readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    token: ['', [Validators.required]],
    novaSenha: ['', [Validators.required, Validators.minLength(6)]],
  });

  constructor(
    private readonly authService: AuthService,
    private readonly route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    this.form.patchValue({
      email: this.route.snapshot.queryParamMap.get('email') || '',
      token: this.route.snapshot.queryParamMap.get('token') || '',
    });
  }

  protected submit(): void {
    this.feedback = '';
    this.error = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error = 'Confira o link recebido e informe uma nova senha com pelo menos 6 caracteres.';
      return;
    }

    this.authService.resetPassword(this.form.getRawValue()).subscribe({
      next: (response) => {
        localStorage.removeItem('pendingPasswordResetEmail');
        this.feedback = response || 'Senha redefinida com sucesso.';
        this.form.controls.novaSenha.reset();
      },
      error: (error) => {
        this.error = this.getErrorMessage(error, 'Nao foi possivel redefinir sua senha.');
      },
    });
  }

  protected togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
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
