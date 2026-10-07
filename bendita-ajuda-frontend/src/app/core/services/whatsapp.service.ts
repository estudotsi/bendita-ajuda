import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Provider } from '../models';
import { AuthService } from './auth.service';

/** Abre a conversa no WhatsApp com o prestador (exige estar dentro do app). */
@Injectable({ providedIn: 'root' })
export class WhatsappService {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  call(provider: Provider): void {
    if (!this.auth.isLoggedIn()) {
      // Guarda para onde voltar, para a pessoa continuar de onde parou.
      this.router.navigate(['/entrar'], {
        queryParams: { voltarPara: `/prestador/${provider.id}` },
      });
      return;
    }

    // O número só vem da API para quem entrou. Se a lista foi carregada antes de entrar,
    // a página do prestador busca de novo, já com o número.
    if (!provider.whatsapp) {
      this.router.navigate(['/prestador', provider.id]);
      return;
    }

    window.open(this.buildUrl(provider), '_blank', 'noopener');
  }

  /** Só chamar com `provider.whatsapp` preenchido. */
  buildUrl(provider: Provider): string {
    const firstName = provider.name.split(' ')[0];
    const service = provider.category.name.toLowerCase();
    const message = `Olá, ${firstName}! Vi seu perfil no Bendita Ajuda e preciso de ${service}.`;
    return `https://wa.me/55${provider.whatsapp}?text=${encodeURIComponent(message)}`;
  }
}
