import { Component, inject, input } from '@angular/core';
import { Provider } from '../../../core/models';
import { WhatsappService } from '../../../core/services/whatsapp.service';

/** Botão verde "Chamar no WhatsApp" (pede para entrar se a pessoa ainda não entrou). */
@Component({
  selector: 'app-whatsapp-button',
  standalone: false,
  templateUrl: './whatsapp-button.html',
  styleUrl: './whatsapp-button.scss',
})
export class WhatsappButton {
  private readonly whatsapp = inject(WhatsappService);

  readonly provider = input.required<Provider>();

  protected call(): void {
    this.whatsapp.call(this.provider());
  }
}
