import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of, throwError } from 'rxjs';
import { Category, Provider, ProviderQuery, ProviderSearchResult, ServiceOption } from '../core/models';
import { placeLabel } from '../core/utils/cep';

const API = '/api/prestadores';

/** Prestador como vem da API (GET /api/prestadores e /api/prestadores/{id}). */
interface ApiProvider {
  id: string;
  nome: string;
  fotoUrl: string | null;
  bio: string | null;
  bairro: string | null;
  cidade: string;
  uf: string;
  servicos: { id: string; nome: string }[];
  whatsapp: string | null;
}

interface ApiSearchResult {
  servicosEncontrados: ServiceOption[];
  prestadores: ApiProvider[];
}

/** Busca de prestadores e lista de categorias (a regra da busca fica no backend). */
@Injectable({ providedIn: 'root' })
export class ProviderService {
  private readonly http = inject(HttpClient);

  getCategories(): Observable<Category[]> {
    return this.http.get<ServiceOption[]>('/api/servicos').pipe(map((services) => services.map(toCategory)));
  }

  searchProviders({ categoryId, text, location }: ProviderQuery): Observable<ProviderSearchResult> {
    let params = new HttpParams();
    if (categoryId) params = params.set('servico', categoryId);
    else if (text) params = params.set('texto', text);
    if (location) {
      params = params.set('cidade', location.city).set('uf', location.uf);
      if (location.neighborhood) params = params.set('bairro', location.neighborhood);
    }

    return this.http.get<ApiSearchResult>(API, { params }).pipe(
      map((result) => ({
        matchedCategories: result.servicosEncontrados.map(toCategory),
        providers: result.prestadores.map(toProvider),
      })),
    );
  }

  /** Null quando o prestador não existe ou não está aparecendo na busca. */
  getProviderById(id: string): Observable<Provider | null> {
    return this.http.get<ApiProvider>(`${API}/${id}`).pipe(
      map(toProvider),
      catchError((err) =>
        err instanceof HttpErrorResponse && (err.status === 404 || err.status === 400) ? of(null) : throwError(() => err),
      ),
    );
  }
}

function toCategory(service: ServiceOption): Category {
  return { id: service.id, name: service.nome, pluralName: service.nomePlural };
}

function toProvider(api: ApiProvider): Provider {
  // Todo prestador que aparece tem pelo menos um serviço (o backend garante).
  const services = api.servicos.map((s) => ({ id: s.id, name: s.nome }));
  return {
    id: api.id,
    name: api.nome,
    category: services[0],
    services,
    place: placeLabel(api),
    bio: api.bio,
    photoUrl: api.fotoUrl ?? undefined,
    whatsapp: api.whatsapp,
  };
}
