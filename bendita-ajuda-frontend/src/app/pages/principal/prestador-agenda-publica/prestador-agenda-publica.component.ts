import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs';

import {
  AgendaService,
  HorarioAgendaResponse,
  MesAgendaResponse,
} from '../../../shared/services/agenda.service';

@Component({
  selector: 'app-prestador-agenda-publica',
  standalone: false,
  templateUrl: './prestador-agenda-publica.component.html',
  styleUrl: './prestador-agenda-publica.component.scss',
})
export class PrestadorAgendaPublicaComponent implements OnInit {
  protected agenda: MesAgendaResponse[] = [];
  protected error = '';
  protected info = '';
  protected carregando = false;
  protected mesSelecionadoKey = '';
  protected diaSelecionadoKey = '';

  constructor(
    private readonly agendaService: AgendaService,
    private readonly route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    const prestadorId = Number(this.route.snapshot.paramMap.get('prestadorId'));

    if (!prestadorId) {
      this.error = 'Prestador nao encontrado.';
      return;
    }

    this.carregarAgenda(prestadorId);
  }

  protected selecionarHorario(horario: HorarioAgendaResponse): void {
    this.info = `O horario das ${horario.horaInicio} as ${horario.horaFim} esta disponivel. Entre em contato com o prestador pelo WhatsApp para combinar o atendimento.`;
  }

  protected formatarDataDia(data: string): string {
    return new Intl.DateTimeFormat('pt-BR').format(new Date(data));
  }

  protected get mesSelecionado(): MesAgendaResponse | null {
    return this.agenda.find((mes) => this.getMesKey(mes) === this.mesSelecionadoKey) ?? null;
  }

  protected get diaSelecionado() {
    return (
      this.mesSelecionado?.dias.find((dia) => this.getDiaKey(dia.data) === this.diaSelecionadoKey) ??
      null
    );
  }

  protected selecionarMes(mes: MesAgendaResponse): void {
    this.mesSelecionadoKey = this.getMesKey(mes);
    this.diaSelecionadoKey = mes.dias[0] ? this.getDiaKey(mes.dias[0].data) : '';
    this.info = '';
  }

  protected selecionarDia(data: string): void {
    this.diaSelecionadoKey = this.getDiaKey(data);
    this.info = '';
  }

  protected getMesKey(mes: MesAgendaResponse): string {
    return `${mes.ano}-${mes.mes}`;
  }

  protected getDiaKey(data: string): string {
    return data.slice(0, 10);
  }

  private carregarAgenda(prestadorId: number): void {
    const { dataInicial, dataFinal } = this.getPeriodoPadrao();
    this.carregando = true;

    this.agendaService
      .obterAgendaPublica(prestadorId, dataInicial, dataFinal)
      .pipe(finalize(() => (this.carregando = false)))
      .subscribe({
        next: (agenda) => {
          this.agenda = agenda;
          this.info = agenda.length ? '' : 'Esse prestador ainda nao tem horarios disponiveis.';
          this.selecionarPrimeiroPeriodoDisponivel();
        },
        error: (error) => {
          this.error = this.getErrorMessage(
            error,
            'Nao foi possivel carregar a agenda do prestador.',
          );
        },
      });
  }

  private getPeriodoPadrao(): { dataInicial: string; dataFinal: string } {
    const hoje = new Date();
    const dataInicial = new Date(hoje.getFullYear(), hoje.getMonth(), 1);
    const dataFinal = new Date(hoje.getFullYear(), hoje.getMonth() + 2, 0);

    return {
      dataInicial: this.formatarDataApi(dataInicial),
      dataFinal: this.formatarDataApi(dataFinal),
    };
  }

  private formatarDataApi(data: Date): string {
    const ano = data.getFullYear();
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const dia = String(data.getDate()).padStart(2, '0');

    return `${ano}-${mes}-${dia}`;
  }

  private selecionarPrimeiroPeriodoDisponivel(): void {
    if (!this.agenda.length) {
      this.mesSelecionadoKey = '';
      this.diaSelecionadoKey = '';
      return;
    }

    const mesAtual =
      this.agenda.find((mes) => this.getMesKey(mes) === this.mesSelecionadoKey) ?? this.agenda[0];

    this.mesSelecionadoKey = this.getMesKey(mesAtual);

    const diaAtual =
      mesAtual.dias.find((dia) => this.getDiaKey(dia.data) === this.diaSelecionadoKey) ??
      mesAtual.dias[0];

    this.diaSelecionadoKey = diaAtual ? this.getDiaKey(diaAtual.data) : '';
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse && error.error?.mensagem) {
      return error.error.mensagem;
    }

    return fallback;
  }
}
