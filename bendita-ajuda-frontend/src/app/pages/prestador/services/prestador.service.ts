import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  EnderecoPrestadorRequest,
  BrasilApiCepResponse,
  MeuCadastroPrestador,
  NominatimSearchResponse,
  PrestadorServicosRequest,
  ServicoAtivo,
  StatusCadastroPrestador,
  SugestaoServicoRequest,
} from '../models/prestador.models';

@Injectable()
export class PrestadorService {
  private readonly apiUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  getStatusCadastro(): Observable<StatusCadastroPrestador> {
    return this.http.get<StatusCadastroPrestador>(`${this.apiUrl}prestador/status-cadastro`);
  }

  getMeuCadastro(): Observable<MeuCadastroPrestador> {
    return this.http.get<MeuCadastroPrestador>(`${this.apiUrl}prestador/meu-cadastro`);
  }

  getServicosAtivos(): Observable<ServicoAtivo[]> {
    return this.http.get<ServicoAtivo[]>(`${this.apiUrl}servicos/ativos`);
  }

  salvarEndereco(payload: EnderecoPrestadorRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}prestador/endereco`, payload);
  }

  salvarServicos(payload: PrestadorServicosRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}prestador/servicos`, payload);
  }

  sugerirServico(payload: SugestaoServicoRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}servicos/sugestao`, payload);
  }

  buscarEnderecoPorCep(cep: string): Observable<BrasilApiCepResponse> {
    return this.http.get<BrasilApiCepResponse>(`https://brasilapi.com.br/api/cep/v2/${cep}`);
  }

  buscarCoordenadasPorEndereco(endereco: string): Observable<NominatimSearchResponse[]> {
    const params = {
      format: 'jsonv2',
      limit: '1',
      countrycodes: 'br',
      q: endereco,
    };

    return this.http.get<NominatimSearchResponse[]>('https://nominatim.openstreetmap.org/search', {
      params,
    });
  }
}
