import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  AdminServico,
  AdminServicoPendente,
  CorrigirAprovarServicoRequest,
  CriarServicoRequest,
  SubstituirServicoRequest,
} from '../models/admin-servicos.models';

@Injectable()
export class AdminServicosService {
  private readonly apiUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  getServicosAtivos(): Observable<AdminServico[]> {
    return this.http.get<AdminServico[]>(`${this.apiUrl}servicos/ativos`);
  }

  getServicosPendentes(): Observable<AdminServicoPendente[]> {
    return this.http.get<AdminServicoPendente[]>(`${this.apiUrl}admin/servicos/pendentes`);
  }

  aprovar(servicoId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}admin/servicos/${servicoId}/aprovar`, {});
  }

  corrigirAprovar(
    servicoId: number,
    payload: CorrigirAprovarServicoRequest,
  ): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}admin/servicos/${servicoId}/corrigir-aprovar`,
      payload,
    );
  }

  substituir(servicoId: number, payload: SubstituirServicoRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}admin/servicos/${servicoId}/substituir`, payload);
  }

  excluir(servicoId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}admin/servicos/${servicoId}`);
  }

  criar(payload: CriarServicoRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}admin/servicos`, payload);
  }
}
