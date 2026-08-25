import { HttpErrorResponse } from '@angular/common/http';
import {
  AfterViewInit,
  Component,
  ElementRef,
  NgZone,
  OnInit,
  ViewChild,
  inject,
} from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';
import { environment } from '../../../../environments/environment';

interface GoogleIdentityBrowser {
  google?: {
    accounts?: {
      id?: {
        initialize(config: GoogleIdentityConfig): void;
        renderButton(element: HTMLElement, options: GoogleButtonOptions): void;
      };
    };
  };
}

type GoogleIdentityAccounts = NonNullable<NonNullable<GoogleIdentityBrowser['google']>['accounts']>;
type GoogleIdentityClient = GoogleIdentityAccounts['id'];

interface GoogleIdentityConfig {
  client_id: string;
  callback: (response: GoogleCredentialResponse) => void;
}

interface GoogleCredentialResponse {
  credential?: string;
}

interface GoogleButtonOptions {
  theme: 'outline' | 'filled_blue' | 'filled_black';
  size: 'large' | 'medium' | 'small';
  type: 'standard' | 'icon';
  shape: 'rectangular' | 'pill' | 'circle' | 'square';
  text: 'signin_with' | 'signup_with' | 'continue_with' | 'signin';
  width?: number;
  locale?: string;
}

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
})
export class LoginComponent implements OnInit, AfterViewInit {
  @ViewChild('googleButton', { static: false })
  private readonly googleButton?: ElementRef<HTMLElement>;

  private readonly formBuilder = inject(FormBuilder);
  private readonly zone = inject(NgZone);
  private googleScriptLoadPromise?: Promise<void>;

  protected feedback = '';
  protected error = '';
  protected showPassword = false;
  protected isGoogleLoading = false;

  protected readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required]],
  });

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.router.navigate([this.authService.getDefaultRouteForCurrentUser()]);
    }
  }

  ngAfterViewInit(): void {
    this.renderGoogleButton();
  }

  protected submit(): void {
    this.feedback = '';
    this.error = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error = 'Informe um e-mail valido e sua senha.';
      return;
    }

    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate([this.authService.getDefaultRouteForCurrentUser()]),
      error: (error) => {
        this.error = this.getErrorMessage(error, 'Nao foi possivel entrar. Confira seus dados.');
      },
    });
  }

  protected togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  private async renderGoogleButton(): Promise<void> {
    try {
      await this.loadGoogleIdentityScript();

      const googleIdentity = this.getGoogleIdentity();
      const buttonElement = this.googleButton?.nativeElement;

      if (!googleIdentity || !buttonElement) {
        return;
      }

      googleIdentity.initialize({
        client_id: environment.googleClientId,
        callback: (response) => this.handleGoogleCredential(response),
      });

      googleIdentity.renderButton(buttonElement, {
        theme: 'outline',
        size: 'large',
        type: 'standard',
        shape: 'rectangular',
        text: 'signin_with',
        width: 400,
        locale: 'pt-BR',
      });
    } catch {
      this.zone.run(() => {
        this.error = 'Nao foi possivel carregar o login do Google.';
      });
    }
  }

  private handleGoogleCredential(response: GoogleCredentialResponse): void {
    this.zone.run(() => {
      this.feedback = '';
      this.error = '';

      if (!response.credential) {
        this.error = 'Nao foi possivel obter a credencial do Google.';
        return;
      }

      this.isGoogleLoading = true;

      this.authService.loginWithGoogle({ idToken: response.credential }).subscribe({
        next: () => this.router.navigate([this.authService.getDefaultRouteForCurrentUser()]),
        error: (error) => {
          this.error = this.getErrorMessage(
            error,
            'Nao foi possivel entrar com Google. Tente novamente.',
          );
          this.isGoogleLoading = false;
        },
      });
    });
  }

  private loadGoogleIdentityScript(): Promise<void> {
    if (this.googleScriptLoadPromise) {
      return this.googleScriptLoadPromise;
    }

    this.googleScriptLoadPromise = new Promise((resolve, reject) => {
      if (this.getGoogleIdentity()) {
        resolve();
        return;
      }

      const existingScript = document.querySelector<HTMLScriptElement>(
        'script[src="https://accounts.google.com/gsi/client"]',
      );

      if (existingScript) {
        existingScript.addEventListener('load', () => resolve(), { once: true });
        existingScript.addEventListener('error', () => reject(new Error('Falha ao carregar Google.')), {
          once: true,
        });
        return;
      }

      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.defer = true;
      script.onload = () => resolve();
      script.onerror = () => reject(new Error('Falha ao carregar Google.'));

      document.head.appendChild(script);
    });

    return this.googleScriptLoadPromise;
  }

  private getGoogleIdentity(): GoogleIdentityClient | undefined {
    return (window as unknown as GoogleIdentityBrowser).google?.accounts?.id;
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
