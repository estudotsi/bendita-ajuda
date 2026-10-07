import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CepAddress, MyProviderProfile, ProviderSignupRequest, ServiceOption } from '../core/models';

/** Cadastro de prestador: lista de serviços, CEP e o próprio cadastro. */
@Injectable({ providedIn: 'root' })
export class ProviderSignupService {
  private readonly http = inject(HttpClient);

  getServices(): Observable<ServiceOption[]> {
    return this.http.get<ServiceOption[]>('/api/servicos');
  }

  lookupCep(cep: string): Observable<CepAddress> {
    return this.http.get<CepAddress>(`/api/cep/${cep}`);
  }

  /** 404 quando a pessoa ainda não é prestador. */
  getMine(): Observable<MyProviderProfile> {
    return this.http.get<MyProviderProfile>('/api/prestadores/eu');
  }

  register(request: ProviderSignupRequest): Observable<MyProviderProfile> {
    return this.http.post<MyProviderProfile>('/api/prestadores/eu', request);
  }
}
