import { Component, computed, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';

/** Cabeçalho fixo do app: nome à esquerda, "Entrar" (ou "Olá, fulano" + "Sair") à direita. */
@Component({
  selector: 'app-site-header',
  standalone: false,
  templateUrl: './site-header.html',
  styleUrl: './site-header.scss',
})
export class SiteHeader {
  protected readonly auth = inject(AuthService);

  protected readonly firstName = computed(() => this.auth.user()?.nome.trim().split(/\s+/)[0] ?? '');
}
