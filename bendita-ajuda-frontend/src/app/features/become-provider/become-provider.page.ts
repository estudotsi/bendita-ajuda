import { Component } from '@angular/core';

/** Cadastro de prestador (placeholder). */
@Component({
  selector: 'app-become-provider-page',
  standalone: false,
  template: `
    <app-placeholder-page
      heading="Quero oferecer meus serviços"
      icon="hand-helping"
      description="Em breve você vai poder se cadastrar aqui e receber clientes pelo WhatsApp."
    />
  `,
})
export class BecomeProviderPage {}
