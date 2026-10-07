import { Component, computed, inject, signal } from '@angular/core';
import { apiErrorMessage } from '../../../../core/services/auth.service';
import { cepDigits, formatCep } from '../../../../core/utils/cep';
import { LocationService, locationLabel } from '../../../../data/location.service';

/** "Perto de você — Asa Norte, Brasília" com opção de trocar, informando o CEP. */
@Component({
  selector: 'app-location-chip',
  standalone: false,
  templateUrl: './location-chip.html',
  styleUrl: './location-chip.scss',
})
export class LocationChip {
  private readonly locationService = inject(LocationService);

  protected readonly location = this.locationService.current;
  protected readonly label = computed(() => {
    const location = this.location();
    return location ? locationLabel(location) : null;
  });

  protected readonly isChoosing = signal(false);
  protected readonly cep = signal('');
  protected readonly busy = signal(false);
  protected readonly error = signal('');

  protected toggle(): void {
    this.isChoosing.set(!this.isChoosing());
    this.cep.set('');
    this.error.set('');
  }

  protected onCepInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.cep.set(formatCep(input.value));
    input.value = this.cep();
    this.error.set('');
    // Com os 8 números já procura: a pessoa não precisa achar o botão.
    if (cepDigits(this.cep()).length === 8) this.confirm();
  }

  protected confirm(event?: Event): void {
    event?.preventDefault();
    if (this.busy()) return;

    const digits = cepDigits(this.cep());
    if (digits.length !== 8) {
      this.error.set('O CEP tem 8 números. Confira e digite de novo.');
      return;
    }

    this.busy.set(true);
    this.locationService.setFromCep(digits).subscribe({
      next: () => {
        this.busy.set(false);
        this.isChoosing.set(false);
      },
      error: (err) => {
        this.busy.set(false);
        this.error.set(apiErrorMessage(err));
      },
    });
  }
}
