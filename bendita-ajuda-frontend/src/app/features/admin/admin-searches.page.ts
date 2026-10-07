import { Component, effect, inject, signal, untracked } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, forkJoin } from 'rxjs';
import { NotUnderstoodSearch, SearchWithoutProvider, ServiceOption } from '../../core/models';
import { AuthService, apiErrorMessage } from '../../core/services/auth.service';
import { AdminSearchesService } from '../../data/admin-searches.service';
import { ProviderSignupService } from '../../data/provider-signup.service';

type Mode = 'loading' | 'ready' | 'forbidden' | 'failed';

/**
 * Admin: o que as pessoas procuraram e não acharam.
 * Texto não entendido → ensinar a palavra a um serviço. Serviço sem ninguém na cidade → só informação.
 */
@Component({
  selector: 'app-admin-searches-page',
  standalone: false,
  templateUrl: './admin-searches.page.html',
  styleUrl: './admin-searches.page.scss',
})
export class AdminSearchesPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly api = inject(AdminSearchesService);
  private readonly signupApi = inject(ProviderSignupService);

  protected readonly mode = signal<Mode>('loading');
  protected readonly notUnderstood = signal<NotUnderstoodSearch[]>([]);
  protected readonly withoutProvider = signal<SearchWithoutProvider[]>([]);
  protected readonly services = signal<ServiceOption[]>([]);
  /** Busca com uma ação em andamento. */
  protected readonly busyId = signal<number | null>(null);
  protected readonly error = signal('');
  protected readonly done = signal('');

  constructor() {
    effect(() => {
      if (!this.auth.isChecked()) return;
      const loggedIn = this.auth.isLoggedIn();
      const admin = this.auth.isAdmin();
      untracked(() => {
        if (!loggedIn) {
          this.router.navigate(['/entrar'], { queryParams: { voltarPara: '/admin/buscas' }, replaceUrl: true });
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
    forkJoin({ searches: this.api.list(), services: this.signupApi.getServices() }).subscribe({
      next: ({ searches, services }) => {
        this.notUnderstood.set(searches.naoEntendidas);
        this.withoutProvider.set(searches.semPrestador);
        this.services.set(services);
        this.mode.set('ready');
      },
      error: () => this.mode.set('failed'),
    });
  }

  protected teach(search: NotUnderstoodSearch, word: string, serviceId: string): void {
    if (!serviceId) {
      this.error.set('Escolha o serviço na lista.');
      return;
    }
    if (word.trim().length < 2) {
      this.error.set('Escreva a palavra que vai ser ensinada.');
      return;
    }
    const name = this.services().find((s) => s.id === serviceId)?.nome ?? '';
    this.run(search, this.api.teach(search.id, serviceId, word.trim()), `Quem buscar "${word.trim()}" agora acha ${name}.`);
  }

  protected remove(search: NotUnderstoodSearch): void {
    this.run(search, this.api.remove(search.id), `"${search.texto}" apagado.`);
  }

  protected place(row: SearchWithoutProvider): string {
    return row.cidade ? `${row.cidade} - ${row.uf}` : 'sem local informado';
  }

  private run(search: NotUnderstoodSearch, action: Observable<void>, message: string): void {
    this.busyId.set(search.id);
    this.error.set('');
    this.done.set('');
    action.subscribe({
      next: () => {
        this.busyId.set(null);
        this.done.set(message);
        this.load();
      },
      error: (err) => {
        this.busyId.set(null);
        this.error.set(apiErrorMessage(err));
      },
    });
  }
}
