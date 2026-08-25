import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { finalize } from 'rxjs';

import {
  AgendaService,
  HorarioAgendaResponse,
  MesAgendaResponse,
} from '../../../shared/services/agenda.service';

@Component({
  selector: 'app-prestador-agenda',
  standalone: false,
  templateUrl: './prestador-agenda.component.html',
  styleUrl: './prestador-agenda.component.scss',
})
export class PrestadorAgendaComponent implements OnInit {
  protected agenda: MesAgendaResponse[] = [];
  protected error = '';
  protected success = '';
  protected info = '';
  protected carregando = false;
  protected gerando = false;
  protected horarioCancelandoId: number | null = null;
  protected mesSelecionadoKey = '';
  protected diaSelecionadoKey = '';
  protected readonly horasCheias = Array.from({ length: 24 }, (_, hora) => {
    return `${String(hora).padStart(2, '0')}:00`;
  });

  protected readonly agendaForm: FormGroup;

  constructor(
    private readonly agendaService: AgendaService,
    private readonly fb: FormBuilder,
  ) {
    this.agendaForm = this.fb.group(
      {
        data: ['', Validators.required],
        horaInicio: ['', Validators.required],
        horaFim: ['', Validators.required],
      },
      {
        validators: this.horaFimMaiorQueInicio,
      },
    );
  }

  ngOnInit(): void {
    this.carregarAgenda();
  }

  protected gerarHorarios(): void {
    this.error = '';
    this.success = '';
    this.info = '';

    if (this.agendaForm.invalid) {
      this.agendaForm.markAllAsTouched();
      this.error = 'Preencha data, hora inicio e hora fim corretamente.';
      return;
    }

    const { data, horaInicio, horaFim } = this.agendaForm.getRawValue();
    this.gerando = true;

    this.agendaService
      .gerarHorarios({
        data: data ?? '',
        horaInicio: this.normalizarHora(horaInicio ?? ''),
        horaFim: this.normalizarHora(horaFim ?? ''),
      })
      .pipe(finalize(() => (this.gerando = false)))
      .subscribe({
        next: (horarios) => {
          this.success = horarios.length
            ? 'Horarios gerados com sucesso.'
            : 'Nenhum novo horario foi gerado para esse periodo.';
          this.agendaForm.reset();
          this.carregarAgenda();
        },
        error: (error) => {
          this.error = this.getErrorMessage(error, 'Nao foi possivel gerar os horarios.');
        },
      });
  }

  protected cancelarHorario(horario: HorarioAgendaResponse): void {
    this.error = '';
    this.success = '';
    this.info = '';
    this.horarioCancelandoId = horario.id;

    this.agendaService
      .cancelarHorario(horario.id)
      .pipe(finalize(() => (this.horarioCancelandoId = null)))
      .subscribe({
        next: () => {
          this.success = 'Horario cancelado com sucesso.';
          this.carregarAgenda();
        },
        error: (error) => {
          this.error = this.getErrorMessage(error, 'Nao foi possivel cancelar esse horario.');
        },
      });
  }

  protected isLivre(horario: HorarioAgendaResponse): boolean {
    return horario.status.trim().toLowerCase() === 'livre';
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
  }

  protected selecionarDia(data: string): void {
    this.diaSelecionadoKey = this.getDiaKey(data);
  }

  protected getMesKey(mes: MesAgendaResponse): string {
    return `${mes.ano}-${mes.mes}`;
  }

  protected getDiaKey(data: string): string {
    return data.slice(0, 10);
  }

  private carregarAgenda(): void {
    const { dataInicial, dataFinal } = this.getPeriodoPadrao();
    this.carregando = true;

    this.agendaService
      .obterMinhaAgenda(dataInicial, dataFinal)
      .pipe(finalize(() => (this.carregando = false)))
      .subscribe({
        next: (agenda) => {
          this.agenda = agenda;
          this.info = agenda.length ? '' : 'Voce ainda nao tem horarios cadastrados.';
          this.selecionarPrimeiroPeriodoDisponivel();
        },
        error: (error) => {
          this.error = this.getErrorMessage(error, 'Nao foi possivel carregar sua agenda.');
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

  private normalizarHora(hora: string): string {
    return hora.length === 5 ? `${hora}:00` : hora;
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

  private horaFimMaiorQueInicio(control: AbstractControl): ValidationErrors | null {
    const horaInicio = control.get('horaInicio')?.value;
    const horaFim = control.get('horaFim')?.value;

    if (!horaInicio || !horaFim) {
      return null;
    }

    if (!horaInicio.endsWith(':00')) {
      return { horaInicioNaoCheia: true };
    }

    if (!horaFim.endsWith(':00')) {
      return { horaFimNaoCheia: true };
    }

    return horaFim > horaInicio ? null : { horaFimMenorOuIgual: true };
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse && error.error?.mensagem) {
      return error.error.mensagem;
    }

    return fallback;
  }
}
