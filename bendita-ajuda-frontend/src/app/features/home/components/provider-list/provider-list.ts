import { Component, computed, input, output } from '@angular/core';
import { Category, Provider } from '../../../../core/models';

/** Lista de prestadores com carregamento (esqueleto), vazio e erro. */
@Component({
  selector: 'app-provider-list',
  standalone: false,
  templateUrl: './provider-list.html',
  styleUrl: './provider-list.scss',
})
export class ProviderList {
  readonly heading = input.required<string>();
  readonly providers = input.required<Provider[]>();
  readonly loading = input(false);
  readonly failed = input(false);
  /** Mostra "Ver todos" quando há algum filtro ou busca ativa. */
  readonly canShowAll = input(false);
  /** A busca por texto não combinou com nenhum serviço: mostra os botões de serviço. */
  readonly notUnderstood = input(false);
  /** Serviço entendido, mas ninguém dele ainda. */
  readonly missingCategory = input<Category | null>(null);
  readonly hasLocation = input(false);
  /** Botões mostrados quando não entendemos o texto. */
  readonly categories = input<Category[]>([]);

  readonly showAll = output<void>();
  readonly categoryPicked = output<string>();

  protected readonly skeletons = [0, 1, 2];

  /** Texto anunciado pelo leitor de tela quando a lista muda. */
  protected readonly announcement = computed(() => {
    if (this.loading()) return 'Procurando profissionais...';
    if (this.failed()) return '';
    if (this.notUnderstood()) return 'Não entendemos o que você precisa. Escolha o serviço.';
    const n = this.providers().length;
    if (n === 0) return 'Nenhum profissional encontrado.';
    return n === 1 ? '1 profissional encontrado.' : `${n} profissionais encontrados.`;
  });
}
