import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { UnmatchedSearches } from '../core/models';

const API = '/api/admin/buscas';

/** Admin olhando o que as pessoas procuraram e não acharam. */
@Injectable({ providedIn: 'root' })
export class AdminSearchesService {
  private readonly http = inject(HttpClient);

  list(): Observable<UnmatchedSearches> {
    return this.http.get<UnmatchedSearches>(API);
  }

  /** "É o mesmo que...": a palavra vira palavra-chave do serviço. */
  teach(searchId: number, serviceId: string, word: string): Observable<void> {
    return this.http.post<void>(`${API}/nao-entendidas/${searchId}/ensinar`, { servicoId: serviceId, palavra: word });
  }

  remove(searchId: number): Observable<void> {
    return this.http.delete<void>(`${API}/nao-entendidas/${searchId}`);
  }
}
