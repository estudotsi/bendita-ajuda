import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { CepAddress, MyProviderProfile, ServiceOption } from '../../core/models';
import { categoryAppearance } from '../../core/icons/category-appearance';
import { AuthService, apiErrorMessage } from '../../core/services/auth.service';
import { cepDigits, formatCep, placeLabel } from '../../core/utils/cep';
import { ProviderSignupService } from '../../data/provider-signup.service';

type Mode = 'loading' | 'wizard' | 'profile' | 'failed';
type Step = 1 | 2 | 3;

/**
 * "Quero oferecer meus serviços": 3 passos (serviços → CEP → conferir) e um único POST no fim.
 * Quem já é prestador vê o resumo do próprio cadastro.
 */
@Component({
  selector: 'app-become-provider-page',
  standalone: false,
  templateUrl: './become-provider.page.html',
  styleUrl: './become-provider.page.scss',
})
export class BecomeProviderPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly api = inject(ProviderSignupService);

  protected readonly mode = signal<Mode>('loading');
  protected readonly step = signal<Step>(1);
  protected readonly busy = signal(false);
  protected readonly error = signal('');

  // Passo 1: serviços
  protected readonly services = signal<ServiceOption[]>([]);
  protected readonly selectedIds = signal<string[]>([]);
  protected readonly showOther = signal(false);
  protected readonly other = signal('');

  // Passo 2: CEP
  protected readonly cep = signal('');
  protected readonly address = signal<CepAddress | null>(null);
  protected readonly neighborhood = signal('');

  // Depois de cadastrado
  protected readonly profile = signal<MyProviderProfile | null>(null);
  protected readonly justRegistered = signal(false);

  protected readonly tiles = computed(() =>
    this.services().map((service) => ({
      service,
      look: categoryAppearance(service.id),
      selected: this.selectedIds().includes(service.id),
    })),
  );

  protected readonly selectedNames = computed(() =>
    this.services()
      .filter((s) => this.selectedIds().includes(s.id))
      .map((s) => s.nome),
  );

  protected readonly placeText = computed(() => {
    const address = this.address();
    return address ? placeLabel({ ...address, bairro: this.neighborhood().trim() || null }) : '';
  });

  protected readonly profileServiceNames = computed(() =>
    (this.profile()?.servicos ?? []).map((s) => s.nome).join(', '),
  );

  protected readonly profilePlace = computed(() => {
    const profile = this.profile();
    return profile ? placeLabel(profile) : '';
  });

  constructor() {
    // Espera saber se há sessão: sem login, vai para "Entrar" e volta para cá.
    effect(() => {
      if (!this.auth.isChecked()) return;
      const user = this.auth.user();
      untracked(() => {
        if (!user) {
          this.router.navigate(['/entrar'], { queryParams: { voltarPara: '/seja-prestador' }, replaceUrl: true });
        } else if (this.mode() === 'loading') {
          user.ehPrestador ? this.loadProfile() : this.loadServices();
        }
      });
    });
  }

  // ---------- Passo 1 ----------

  protected toggleService(id: string): void {
    this.selectedIds.update((ids) => (ids.includes(id) ? ids.filter((x) => x !== id) : [...ids, id]));
    this.error.set('');
  }

  protected openOther(): void {
    this.showOther.set(true);
    queueMicrotask(() => document.getElementById('outro-servico')?.focus());
  }

  protected onOtherInput(event: Event): void {
    this.other.set((event.target as HTMLInputElement).value);
    this.error.set('');
  }

  // ---------- Passo 2 ----------

  protected onCepInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.cep.set(formatCep(input.value));
    input.value = this.cep();
    this.error.set('');
    this.address.set(null);
    if (cepDigits(this.cep()).length === 8) this.lookupCep();
  }

  protected onNeighborhoodInput(event: Event): void {
    this.neighborhood.set((event.target as HTMLInputElement).value);
  }

  private lookupCep(): void {
    const digits = cepDigits(this.cep());
    this.busy.set(true);
    this.api.lookupCep(digits).subscribe({
      next: (address) => {
        this.busy.set(false);
        // A pessoa pode ter mudado o CEP enquanto esperava.
        if (cepDigits(this.cep()) !== digits) return;
        this.address.set(address);
        this.neighborhood.set(address.bairro ?? '');
      },
      error: (err) => {
        this.busy.set(false);
        this.error.set(apiErrorMessage(err));
      },
    });
  }

  // ---------- Navegação ----------

  protected next(event?: Event): void {
    event?.preventDefault();
    if (this.busy()) return;

    if (this.step() === 1) {
      if (this.selectedIds().length === 0 && !this.other().trim()) {
        this.error.set('Escolha pelo menos um serviço ou escreva o que você faz.');
        return;
      }
      this.goTo(2);
    } else if (this.step() === 2) {
      if (!this.address()) {
        this.error.set(
          cepDigits(this.cep()).length === 8 ? 'Espere achar o CEP ou confira os números.' : 'Digite os 8 números do CEP.',
        );
        return;
      }
      this.goTo(3);
    } else {
      this.submit();
    }
  }

  protected back(): void {
    if (this.step() > 1) this.goTo((this.step() - 1) as Step);
  }

  private goTo(step: Step): void {
    this.error.set('');
    this.step.set(step);
    window.scrollTo({ top: 0 });
  }

  private submit(): void {
    const address = this.address();
    if (!address) return;

    this.busy.set(true);
    this.error.set('');
    this.api
      .register({
        servicos: this.selectedIds(),
        outroServico: this.other().trim() || null,
        cep: address.cep,
        bairro: this.neighborhood().trim() || null,
      })
      .subscribe({
        next: (profile) => {
          this.busy.set(false);
          this.auth.markAsProvider();
          this.profile.set(profile);
          this.justRegistered.set(true);
          this.mode.set('profile');
          window.scrollTo({ top: 0 });
        },
        error: (err) => {
          this.busy.set(false);
          if (err instanceof HttpErrorResponse && err.status === 409) {
            this.auth.markAsProvider();
            this.loadProfile();
            return;
          }
          this.error.set(apiErrorMessage(err));
        },
      });
  }

  // ---------- Carregamento ----------

  private loadServices(): void {
    this.mode.set('loading');
    this.api.getServices().subscribe({
      next: (services) => {
        this.services.set(services);
        this.mode.set('wizard');
      },
      error: () => this.mode.set('failed'),
    });
  }

  private loadProfile(): void {
    this.mode.set('loading');
    this.api.getMine().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.mode.set('profile');
      },
      // 404: o cookie dizia prestador, mas não há cadastro. Mostra o passo a passo.
      error: (err) => (err instanceof HttpErrorResponse && err.status === 404 ? this.loadServices() : this.mode.set('failed')),
    });
  }

  protected retry(): void {
    this.auth.user()?.ehPrestador ? this.loadProfile() : this.loadServices();
  }
}
