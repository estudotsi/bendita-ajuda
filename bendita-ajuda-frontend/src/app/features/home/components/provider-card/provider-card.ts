import { Component, computed, input } from '@angular/core';
import { Provider } from '../../../../core/models';
import { categoryAppearance } from '../../../../core/icons/category-appearance';

/** Cartão de um prestador: tocar no cartão abre a página dele; o botão chama no WhatsApp. */
@Component({
  selector: 'app-provider-card',
  standalone: false,
  templateUrl: './provider-card.html',
  styleUrl: './provider-card.scss',
})
export class ProviderCard {
  readonly provider = input.required<Provider>();

  protected readonly look = computed(() => categoryAppearance(this.provider().category.id));
}
