import { Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { rxResource, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, map } from 'rxjs';
import { ProviderQuery } from '../../core/models';
import { LocationService } from '../../data/location.service';
import { ProviderService } from '../../data/provider.service';

/** Espera a pessoa parar de digitar antes de buscar. */
const SEARCH_DEBOUNCE_MS = 400;

/**
 * Tela inicial: escolher categoria ou escrever/falar o que precisa,
 * e ver os prestadores perto de você.
 *
 * Regra: categoria e texto não se misturam — escolher uma categoria
 * apaga o texto, e começar a escrever desmarca a categoria.
 */
@Component({
  selector: 'app-home-page',
  standalone: false,
  templateUrl: './home.page.html',
  styleUrl: './home.page.scss',
})
export class HomePage {
  private readonly providerService = inject(ProviderService);
  protected readonly location = inject(LocationService).current;
  private readonly resultsSection = viewChild.required<ElementRef<HTMLElement>>('results');

  protected readonly selectedCategoryId = signal<string | null>(null);
  /** Texto exatamente como está no campo. */
  protected readonly searchDraft = signal('');

  private readonly debouncedSearch = toSignal(
    toObservable(this.searchDraft).pipe(
      map((text) => text.trim()),
      debounceTime(SEARCH_DEBOUNCE_MS),
      distinctUntilChanged(),
    ),
    { initialValue: '' },
  );

  /** Campo vazio limpa a busca na hora; texto novo espera o debounce. */
  protected readonly searchTerm = computed(() => (this.searchDraft().trim() ? this.debouncedSearch() : ''));

  protected readonly categories = rxResource({
    stream: () => this.providerService.getCategories(),
  });

  protected readonly selectedCategory = computed(() => {
    const id = this.selectedCategoryId();
    return this.categories.value()?.find((c) => c.id === id) ?? null;
  });

  private readonly query = computed<ProviderQuery>(() => ({
    categoryId: this.selectedCategoryId(),
    text: this.selectedCategoryId() ? '' : this.searchTerm(),
    location: this.location(),
  }));

  protected readonly search = rxResource({
    params: () => this.query(),
    stream: ({ params }) => this.providerService.searchProviders(params),
  });

  protected readonly providers = computed(() => this.search.value()?.providers ?? []);

  protected readonly isFiltered = computed(() => !!this.selectedCategoryId() || !!this.searchTerm());

  /** Escreveu/falou algo e não entendemos qual serviço é: a lista mostra os botões de serviço. */
  protected readonly notUnderstood = computed(() => {
    const result = this.search.value();
    return (
      !!this.searchTerm() &&
      !this.selectedCategoryId() &&
      !!result &&
      result.matchedCategories.length === 0 &&
      result.providers.length === 0
    );
  });

  /** Entendemos o serviço, mas ainda não há ninguém dele: "Ainda não temos Encanadores...". */
  protected readonly missingCategory = computed(() => {
    const result = this.search.value();
    if (!result || result.providers.length > 0) return null;
    return this.selectedCategory() ?? (result.matchedCategories.length === 1 ? result.matchedCategories[0] : null);
  });

  protected readonly listHeading = computed(() => {
    const near = this.location() ? ' perto de você' : '';
    const category = this.selectedCategory();
    if (category) return `${category.pluralName}${near}`;
    const term = this.searchTerm();
    if (term) return `Quem pode ajudar com “${term}”`;
    return this.location() ? 'Perto de você' : 'Profissionais';
  });

  protected selectCategory(id: string): void {
    const isSame = this.selectedCategoryId() === id;
    this.selectedCategoryId.set(isSame ? null : id);
    this.searchDraft.set('');
    if (!isSame) this.scrollToResults();
  }

  protected onSearchChange(text: string): void {
    this.searchDraft.set(text);
    if (text.trim()) this.selectedCategoryId.set(null);
  }

  protected showAll(): void {
    this.selectedCategoryId.set(null);
    this.searchDraft.set('');
  }

  /** No celular a lista fica abaixo da dobra: leva a pessoa até ela. */
  protected scrollToResults(): void {
    const el = this.resultsSection().nativeElement;
    if (el.getBoundingClientRect().top > window.innerHeight * 0.6) {
      el.scrollIntoView({ block: 'start' });
    }
  }
}
