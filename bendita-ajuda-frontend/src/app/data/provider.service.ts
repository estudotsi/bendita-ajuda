import { Injectable } from '@angular/core';
import { Observable, delay, of } from 'rxjs';
import { Category, Provider, ProviderQuery } from '../core/models';
import { MOCK_CATEGORIES } from './mock/categories.mock';
import { MOCK_PROVIDERS } from './mock/providers.mock';
import { searchProvidersMock } from './mock/provider-search.mock';

/** Tempo de resposta simulado da "API". */
const MOCK_DELAY_MS = 600;

/**
 * Acesso aos prestadores e categorias.
 *
 * Hoje responde com dados falsos. Para ligar no backend, troque o corpo
 * de cada método por uma chamada HTTP mantendo as mesmas assinaturas, ex.:
 *   searchProviders(query) {
 *     return this.http.get<Provider[]>(`${API}/prestadores`, { params: {...} });
 *   }
 * Os componentes não precisam mudar.
 */
@Injectable({ providedIn: 'root' })
export class ProviderService {
  getCategories(): Observable<Category[]> {
    return of(MOCK_CATEGORIES).pipe(delay(MOCK_DELAY_MS / 2));
  }

  searchProviders(query: ProviderQuery): Observable<Provider[]> {
    return of(searchProvidersMock(MOCK_PROVIDERS, MOCK_CATEGORIES, query)).pipe(delay(MOCK_DELAY_MS));
  }

  getProviderById(id: string): Observable<Provider | null> {
    return of(MOCK_PROVIDERS.find((p) => p.id === id) ?? null).pipe(delay(MOCK_DELAY_MS / 2));
  }
}
