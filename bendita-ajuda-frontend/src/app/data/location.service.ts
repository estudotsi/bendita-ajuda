import { Injectable, signal } from '@angular/core';
import { Observable, delay, of } from 'rxjs';
import { UserLocation } from '../core/models';
import { MOCK_NEIGHBORHOODS } from './mock/providers.mock';

/**
 * Local de referência da pessoa. Por enquanto fixo no mock;
 * no futuro pode vir do cadastro ou da geolocalização do aparelho.
 */
@Injectable({ providedIn: 'root' })
export class LocationService {
  readonly current = signal<UserLocation>({ neighborhood: 'Asa Norte', city: 'Brasília' });

  getNeighborhoods(): Observable<string[]> {
    return of(MOCK_NEIGHBORHOODS).pipe(delay(200));
  }

  setNeighborhood(neighborhood: string): void {
    this.current.update((loc) => ({ ...loc, neighborhood }));
  }
}
