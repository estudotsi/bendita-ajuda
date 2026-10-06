import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { LocationService } from '../../../../data/location.service';

/** "Perto de você — Asa Norte" com opção de trocar o bairro. */
@Component({
  selector: 'app-location-chip',
  standalone: false,
  templateUrl: './location-chip.html',
  styleUrl: './location-chip.scss',
})
export class LocationChip {
  private readonly locationService = inject(LocationService);

  protected readonly location = this.locationService.current;
  protected readonly isChoosing = signal(false);

  /** Só busca a lista de bairros quando a pessoa pede para trocar. */
  protected readonly neighborhoods = rxResource({
    params: () => (this.isChoosing() ? true : undefined),
    stream: () => this.locationService.getNeighborhoods(),
  });

  protected choose(neighborhood: string): void {
    this.locationService.setNeighborhood(neighborhood);
    this.isChoosing.set(false);
  }
}
