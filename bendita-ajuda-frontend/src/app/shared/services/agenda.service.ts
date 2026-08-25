import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

export interface GerarHorariosAgendaRequest {
  data: string;
  horaInicio: string;
  horaFim: string;
}

export interface HorarioAgendaResponse {
  id: number;
  inicio: string;
  fim: string;
  horaInicio: string;
  horaFim: string;
  status: string;
}

export interface DiaAgendaResponse {
  data: string;
  diaSemana: string;
  horarios: HorarioAgendaResponse[];
}

export interface MesAgendaResponse {
  mes: number;
  ano: number;
  nomeMes: string;
  dias: DiaAgendaResponse[];
}

@Injectable({
  providedIn: 'root',
})
export class AgendaService {
  private readonly apiUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  gerarHorarios(request: GerarHorariosAgendaRequest): Observable<HorarioAgendaResponse[]> {
    return this.http.post<HorarioAgendaResponse[]>(
      `${this.apiUrl}agenda/prestador/gerar-horarios`,
      request,
    );
  }

  obterMinhaAgenda(dataInicial?: string, dataFinal?: string): Observable<MesAgendaResponse[]> {
    return this.http.get<MesAgendaResponse[]>(`${this.apiUrl}agenda/prestador/minha-agenda`, {
      params: this.getPeriodoParams(dataInicial, dataFinal),
    });
  }

  obterAgendaPublica(
    prestadorId: number,
    dataInicial?: string,
    dataFinal?: string,
  ): Observable<MesAgendaResponse[]> {
    return this.http.get<MesAgendaResponse[]>(`${this.apiUrl}agenda/prestador/${prestadorId}`, {
      params: this.getPeriodoParams(dataInicial, dataFinal),
    });
  }

  cancelarHorario(horarioId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}agenda/prestador/horarios/${horarioId}`);
  }

  private getPeriodoParams(dataInicial?: string, dataFinal?: string): Record<string, string> {
    const params: Record<string, string> = {};

    if (dataInicial) {
      params['dataInicial'] = dataInicial;
    }

    if (dataFinal) {
      params['dataFinal'] = dataFinal;
    }

    return params;
  }
}
