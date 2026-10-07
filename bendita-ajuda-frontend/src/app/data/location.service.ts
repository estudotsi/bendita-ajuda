import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { CepAddress, UserLocation } from '../core/models';

const STORAGE_KEY = 'bendita-ajuda.local';

/**
 * Onde a pessoa está. Ela informa o CEP uma vez e fica guardado neste aparelho
 * (só bairro, cidade e UF: o CEP em si não é guardado). Null = ainda não informou.
 */
@Injectable({ providedIn: 'root' })
export class LocationService {
  private readonly http = inject(HttpClient);

  readonly current = signal<UserLocation | null>(readSaved());

  /** Consulta o CEP e passa a usar esse local. Erros chegam com a mensagem da API ({ mensagem }). */
  setFromCep(cep: string): Observable<UserLocation> {
    return this.http.get<CepAddress>(`/api/cep/${cep}`).pipe(
      map((address) => ({ neighborhood: address.bairro, city: address.cidade, uf: address.uf })),
      tap((location) => {
        this.current.set(location);
        save(location);
      }),
    );
  }
}

/** "Asa Norte, Brasília" ou, sem bairro, só a cidade. */
export function locationLabel(location: UserLocation): string {
  return location.neighborhood ? `${location.neighborhood}, ${location.city}` : location.city;
}

// O navegador pode bloquear o armazenamento (aba anônima, configurações): aí só não lembra.
function readSaved(): UserLocation | null {
  try {
    const saved = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? 'null');
    return saved?.city && saved?.uf ? saved : null;
  } catch {
    return null;
  }
}

function save(location: UserLocation): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(location));
  } catch {
    // Sem armazenamento: vale só enquanto a página estiver aberta.
  }
}
