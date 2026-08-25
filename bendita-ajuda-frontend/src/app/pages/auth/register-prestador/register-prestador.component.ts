import { Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import { AuthService } from '../../../core/services/auth.service';
import { getHttpErrorMessage } from '../../../core/utils/http-error-message';

@Component({
  selector: 'app-register-prestador',
  standalone: false,
  templateUrl: './register-prestador.component.html',
})
export class RegisterPrestadorComponent {
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

    this.authService.registerPrestador(this.form.getRawValue()).subscribe({
      next: (response) => {
        this.feedback =
          response.mensagem ||
          'Conta de prestador criada com sucesso. Verifique seu e-mail para confirmar.';
        this.form.reset();
      },
      error: (error) => {
        this.error = getHttpErrorMessage(
          error,
          'Nao foi possivel criar sua conta de prestador.',
        );
      },
    });
  }

  protected togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
