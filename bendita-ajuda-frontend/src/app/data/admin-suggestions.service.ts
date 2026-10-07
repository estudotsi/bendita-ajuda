import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PendingSuggestion } from '../core/models';

const API = '/api/admin/sugestoes';

/**
 * Admin resolvendo os serviços digitados pelos prestadores.
 * Cada ação vale também para as outras sugestões com o mesmo texto (o back cuida disso).
 */
@Injectable({ providedIn: 'root' })
export class AdminSuggestionsService {
  private readonly http = inject(HttpClient);

  list(): Observable<PendingSuggestion[]> {
    return this.http.get<PendingSuggestion[]>(API);
  }

  /** "É o mesmo serviço que...". */
  markSimilar(suggestionId: string, serviceId: string): Observable<void> {
    return this.http.post<void>(`${API}/${suggestionId}/similar`, { servicoId: serviceId });
  }

  createService(suggestionId: string, name: string, pluralName: string): Observable<void> {
    return this.http.post<void>(`${API}/${suggestionId}/novo`, { nome: name, nomePlural: pluralName });
  }

  reject(suggestionId: string): Observable<void> {
    return this.http.delete<void>(`${API}/${suggestionId}`);
  }
}
