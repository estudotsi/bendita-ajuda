import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { PrestadorPorServico, ServicoAtivoLanding } from '../models/principal.models';

@Injectable()
export class PrincipalService {
  private readonly apiUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  getServicosAtivos(): Observable<ServicoAtivoLanding[]> {
    return this.http.get<ServicoAtivoLanding[]>(`${this.apiUrl}servicos/ativos`);
  }

  getPrestadoresPorServico(
    servicoId: number,
    latitude: number,
    longitude: number,
  ): Observable<PrestadorPorServico[]> {
    return this.http.get<PrestadorPorServico[]>(
      `${this.apiUrl}prestador/por-servico/${servicoId}`,
      {
        params: {
          latitude,
          longitude,
        },
      },
    );
  }
}
