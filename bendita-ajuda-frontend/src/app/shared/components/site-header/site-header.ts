import { Component, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';

/** Cabeçalho fixo do app: nome à esquerda, "Entrar" (ou "Sair") à direita. */
@Component({
  selector: 'app-site-header',
  standalone: false,
  templateUrl: './site-header.html',
  styleUrl: './site-header.scss',
})
export class SiteHeader {
  protected readonly auth = inject(AuthService);
}
