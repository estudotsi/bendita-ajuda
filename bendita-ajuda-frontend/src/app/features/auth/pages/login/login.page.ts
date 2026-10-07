import { Component, DestroyRef, ElementRef, computed, effect, inject, input, signal, viewChild } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, apiErrorMessage } from '../../../../core/services/auth.service';
import { formatPhone, isValidPhone, phoneDigits } from '../../../../core/utils/phone';

type Step = 'phone' | 'code' | 'name';

/** Segundos até liberar "Enviar de novo". */
const RESEND_SECONDS = 60;

/**
 * Página "Entrar": celular → código do SMS → nome (só no primeiro acesso).
 * Recebe `?voltarPara=/caminho` para levar a pessoa de volta depois de entrar;
 * sem ele, volta para a página inicial.
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

  protected readonly step = signal<Step>('phone');
  protected readonly phone = signal('');
  protected readonly code = signal('');
  protected readonly name = signal('');
  protected readonly busy = signal(false);
  protected readonly error = signal('');
  protected readonly resendIn = signal(0);

  private readonly field = viewChild<ElementRef<HTMLInputElement>>('field');

  constructor() {
    // Quem já está logado não precisa desta tela.
    effect(() => {
      if (this.auth.isLoggedIn()) this.router.navigateByUrl(this.returnUrl(), { replaceUrl: true });
    });

    // A cada passo, o cursor já vai para o campo.
    effect(() => {
      this.step();
      queueMicrotask(() => this.field()?.nativeElement.focus());
    });

    const timer = setInterval(() => this.resendIn.update((s) => Math.max(0, s - 1)), 1000);
    inject(DestroyRef).onDestroy(() => clearInterval(timer));
  }

  protected onPhoneInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.phone.set(formatPhone(input.value));
    input.value = this.phone();
    this.error.set('');
  }

  protected onCodeInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.code.set(input.value.replace(/\D/g, '').slice(0, 6));
    input.value = this.code();
    this.error.set('');
    // O celular pode preencher sozinho o código do SMS: confirma direto.
    if (this.code().length === 6) this.confirm();
  }

  protected onNameInput(event: Event): void {
    this.name.set((event.target as HTMLInputElement).value);
    this.error.set('');
  }

  protected submit(event: Event): void {
    event.preventDefault();
    if (this.busy()) return;

    switch (this.step()) {
      case 'phone':
        return this.sendCode();
      case 'code':
        return this.confirm();
      case 'name':
        return this.finish();
    }
  }

  protected sendCode(): void {
    if (!isValidPhone(this.phone())) {
      this.error.set('Digite o DDD e o número do celular. Ex.: (61) 99999-8888');
      return;
    }

    this.busy.set(true);
    this.error.set('');
    this.auth.sendCode(phoneDigits(this.phone())).subscribe({
      next: () => {
        this.busy.set(false);
        this.code.set('');
        this.resendIn.set(RESEND_SECONDS);
        this.step.set('code');
      },
      error: (err) => {
        this.busy.set(false);
        this.error.set(apiErrorMessage(err));
      },
    });
  }

  protected changePhone(): void {
    this.error.set('');
    this.step.set('phone');
  }

  private confirm(): void {
    if (this.busy()) return;
    if (this.code().length !== 6) {
      this.error.set('O código tem 6 números.');
      return;
    }

    this.busy.set(true);
    this.error.set('');
    this.auth.confirmCode(phoneDigits(this.phone()), this.code()).subscribe({
      next: (result) => {
        this.busy.set(false);
        // Logado: o effect do construtor leva para a página certa.
        if ('precisaNome' in result) this.step.set('name');
      },
      error: (err) => {
        this.busy.set(false);
        this.error.set(apiErrorMessage(err));
      },
    });
  }

  private finish(): void {
    const name = this.name().trim();
    if (name.length < 2) {
      this.error.set('Escreva seu nome.');
      return;
    }

    this.busy.set(true);
    this.error.set('');
    this.auth.confirmCode(phoneDigits(this.phone()), this.code(), name).subscribe({
      next: () => this.busy.set(false),
      error: (err) => {
        this.busy.set(false);
        this.error.set(apiErrorMessage(err));
      },
    });
  }
}
