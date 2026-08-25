import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
})
export class RegisterComponent {
  private readonly formBuilder = inject(FormBuilder);

  protected feedback = '';
  protected error = '';
  protected showPassword = false;

  protected readonly form = this.formBuilder.nonNullable.group({
    nome: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required, Validators.minLength(6)]],
  });

  constructor(
    private readonly authService: AuthService,
  ) {}

  protected submit(): void {
    this.feedback = '';
    this.error = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error = 'Preencha nome, e-mail valido e uma senha com pelo menos 6 caracteres.';
      return;
    }

    this.authService.register(this.form.getRawValue()).subscribe({
      next: (response) => {
        this.feedback =
          response.mensagem || 'Conta criada com sucesso. Verifique seu e-mail para confirmar.';
        this.form.reset();
      },
      error: (error) => {
        this.error = this.getErrorMessage(error, 'Nao foi possivel criar sua conta agora.');
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
