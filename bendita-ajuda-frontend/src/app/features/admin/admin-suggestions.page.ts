import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { PendingSuggestion, ServiceOption } from '../../core/models';
import { AuthService, apiErrorMessage } from '../../core/services/auth.service';
import { normalizeText } from '../../core/utils/text';
import { AdminSuggestionsService } from '../../data/admin-suggestions.service';
import { ProviderSignupService } from '../../data/provider-signup.service';

type Mode = 'loading' | 'ready' | 'forbidden' | 'failed';

/** Sugestões com o mesmo texto ("boleira", "Boleira ") aparecem juntas e são resolvidas de uma vez. */
interface SuggestionGroup {
  key: string;
  first: PendingSuggestion;
  items: PendingSuggestion[];
}

/** Admin: serviços que os prestadores digitaram e não estão na lista. */
@Component({
  selector: 'app-admin-suggestions-page',
  standalone: false,
  templateUrl: './admin-suggestions.page.html',
  styleUrl: './admin-suggestions.page.scss',
})
export class AdminSuggestionsPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly api = inject(AdminSuggestionsService);
  private readonly signupApi = inject(ProviderSignupService);

  protected readonly mode = signal<Mode>('loading');
  protected readonly suggestions = signal<PendingSuggestion[]>([]);
  protected readonly services = signal<ServiceOption[]>([]);
  /** Grupo com uma ação em andamento. */
  protected readonly busyKey = signal<string | null>(null);
  protected readonly error = signal('');
  protected readonly done = signal('');

  protected readonly groups = computed<SuggestionGroup[]>(() => {
    const byKey = new Map<string, PendingSuggestion[]>();
    for (const s of this.suggestions()) {
      const key = normalizeText(s.descricao);
      byKey.set(key, [...(byKey.get(key) ?? []), s]);
    }
    return [...byKey].map(([key, items]) => ({ key, first: items[0], items }));
  });

  constructor() {
    effect(() => {
      if (!this.auth.isChecked()) return;
      const loggedIn = this.auth.isLoggedIn();
      const admin = this.auth.isAdmin();
      untracked(() => {
        if (!loggedIn) {
          this.router.navigate(['/entrar'], { queryParams: { voltarPara: '/admin' }, replaceUrl: true });
        } else if (!admin) {
          this.mode.set('forbidden');
        } else if (this.mode() === 'loading') {
          this.load();
        }
      });
    });
  }

  protected load(): void {
    this.mode.set('loading');
    forkJoin({ suggestions: this.api.list(), services: this.signupApi.getServices() }).subscribe({
      next: ({ suggestions, services }) => {
        this.suggestions.set(suggestions);
        this.services.set(services);
        this.mode.set('ready');
      },
      error: () => this.mode.set('failed'),
    });
  }

  /** Sugestão do nome do serviço novo: "boleira" → "Boleira". */
  protected suggestedName(group: SuggestionGroup): string {
    const text = group.first.descricao.trim();
    return text.charAt(0).toUpperCase() + text.slice(1);
  }

  protected markSimilar(group: SuggestionGroup, serviceId: string): void {
    if (!serviceId) {
      this.error.set('Escolha o serviço na lista.');
      return;
    }
    const name = this.services().find((s) => s.id === serviceId)?.nome ?? '';
    this.run(group, this.api.markSimilar(group.first.id, serviceId), `"${group.first.descricao}" agora é ${name}.`);
  }

  protected createService(group: SuggestionGroup, name: string, pluralName: string): void {
    if (name.trim().length < 2 || pluralName.trim().length < 2) {
      this.error.set('Escreva o nome do serviço e o plural.');
      return;
    }
    this.run(
      group,
      this.api.createService(group.first.id, name.trim(), pluralName.trim()),
      `Serviço "${name.trim()}" criado.`,
    );
  }

  protected reject(group: SuggestionGroup): void {
    const ok = window.confirm(`Recusar "${group.first.descricao}"? Isso não pode ser desfeito.`);
    if (ok) this.run(group, this.api.reject(group.first.id), `"${group.first.descricao}" recusado.`);
  }

  private run(group: SuggestionGroup, action: ReturnType<AdminSuggestionsService['reject']>, message: string): void {
    this.busyKey.set(group.key);
    this.error.set('');
    this.done.set('');
    action.subscribe({
      next: () => {
        this.busyKey.set(null);
        this.done.set(message);
        this.load();
      },
      error: (err) => {
        this.busyKey.set(null);
        this.error.set(apiErrorMessage(err));
      },
    });
  }
}
