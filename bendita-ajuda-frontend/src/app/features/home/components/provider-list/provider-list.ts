import { Component, computed, input, output } from '@angular/core';
import { Provider } from '../../../../core/models';

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

  readonly showAll = output<void>();

  protected readonly skeletons = [0, 1, 2];

  /** Texto anunciado pelo leitor de tela quando a lista muda. */
  protected readonly announcement = computed(() => {
    if (this.loading()) return 'Procurando profissionais...';
    if (this.failed()) return '';
    const n = this.providers().length;
    if (n === 0) return 'Nenhum profissional encontrado.';
    return n === 1 ? '1 profissional encontrado.' : `${n} profissionais encontrados.`;
  });
}
