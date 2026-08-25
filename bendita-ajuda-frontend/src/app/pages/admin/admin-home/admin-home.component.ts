import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';

import { AdminServico, AdminServicoPendente } from '../models/admin-servicos.models';
import { AdminServicosService } from '../services/admin-servicos.service';

@Component({
  selector: 'app-admin-home',
  standalone: false,
  templateUrl: './admin-home.component.html',
  styleUrl: './admin-home.component.scss',
})
export class AdminHomeComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly limiteAtivosSemBusca = 12;

  protected servicosAtivos: AdminServico[] = [];
  protected servicosPendentes: AdminServicoPendente[] = [];
  protected termoBusca = '';
  protected mostrandoTodosAtivos = false;
  protected feedback = '';
  protected error = '';

  protected servicoEmCorrecao: AdminServicoPendente | null = null;
  protected servicoEmSubstituicao: AdminServicoPendente | null = null;
  protected criandoServico = false;

  protected readonly corrigirForm = this.formBuilder.nonNullable.group({
    nome: ['', [Validators.required]],
  });

  protected readonly substituirForm = this.formBuilder.nonNullable.group({
    servicoCorretoId: [0, [Validators.required, Validators.min(1)]],
  });

  protected readonly criarForm = this.formBuilder.nonNullable.group({
    nome: ['', [Validators.required]],
  });

  constructor(private readonly adminServicosService: AdminServicosService) {}

  ngOnInit(): void {
    this.carregarListas();
  }

  protected get pendentesFiltrados(): AdminServicoPendente[] {
    const termo = this.termoBuscaNormalizado;
    return this.servicosPendentes.filter((servico) =>
      servico.nome.toLowerCase().includes(termo),
    );
  }

  protected get ativosFiltrados(): AdminServico[] {
    const termo = this.termoBuscaNormalizado;
    return this.servicosAtivos.filter((servico) => servico.nome.toLowerCase().includes(termo));
  }

  protected get ativosVisiveis(): AdminServico[] {
    if (this.termoBuscaNormalizado || this.mostrandoTodosAtivos) {
      return this.ativosFiltrados;
    }

    return this.ativosFiltrados.slice(0, this.limiteAtivosSemBusca);
  }

  protected get totalAtivosOcultos(): number {
    return Math.max(this.ativosFiltrados.length - this.ativosVisiveis.length, 0);
  }

  protected get deveMostrarControleAtivos(): boolean {
    return !this.termoBuscaNormalizado && this.servicosAtivos.length > this.limiteAtivosSemBusca;
  }

  protected atualizarTermoBusca(termo: string): void {
    this.termoBusca = termo;
    this.mostrandoTodosAtivos = false;
  }

  protected alternarAtivos(): void {
    this.mostrandoTodosAtivos = !this.mostrandoTodosAtivos;
  }

  protected aprovar(servico: AdminServicoPendente): void {
    this.limparMensagens();
    this.adminServicosService.aprovar(servico.id).subscribe({
      next: () => {
        this.feedback = 'Servico aprovado com sucesso.';
        this.carregarListas();
      },
      error: (error) => {
        this.error = this.getErrorMessage(error, 'Nao foi possivel aprovar o servico.');
      },
    });
  }

  protected abrirCorrecao(servico: AdminServicoPendente): void {
    this.servicoEmCorrecao = servico;
    this.corrigirForm.setValue({ nome: servico.nome });
    this.limparMensagens();
  }

  protected corrigirAprovar(): void {
    if (!this.servicoEmCorrecao) {
      return;
    }

    if (this.corrigirForm.invalid) {
      this.corrigirForm.markAllAsTouched();
      return;
    }

    this.limparMensagens();
    this.adminServicosService
      .corrigirAprovar(this.servicoEmCorrecao.id, this.corrigirForm.getRawValue())
      .subscribe({
        next: () => {
          this.feedback = 'Servico corrigido e aprovado com sucesso.';
          this.fecharModais();
          this.carregarListas();
        },
        error: (error) => {
          this.error = this.getErrorMessage(error, 'Nao foi possivel corrigir e aprovar.');
        },
      });
  }

  protected abrirSubstituicao(servico: AdminServicoPendente): void {
    this.servicoEmSubstituicao = servico;
    this.substituirForm.setValue({ servicoCorretoId: 0 });
    this.limparMensagens();
  }

  protected substituir(): void {
    if (!this.servicoEmSubstituicao) {
      return;
    }

    if (this.substituirForm.invalid) {
      this.substituirForm.markAllAsTouched();
      return;
    }

    this.limparMensagens();
    this.adminServicosService
      .substituir(this.servicoEmSubstituicao.id, this.substituirForm.getRawValue())
      .subscribe({
        next: () => {
          this.feedback = 'Servico substituido com sucesso.';
          this.fecharModais();
          this.carregarListas();
        },
        error: (error) => {
          this.error = this.getErrorMessage(error, 'Nao foi possivel substituir o servico.');
        },
      });
  }

  protected excluir(servico: AdminServicoPendente): void {
    const confirmou = window.confirm(`Excluir o servico "${servico.nome}"?`);

    if (!confirmou) {
      return;
    }

    this.limparMensagens();
    this.adminServicosService.excluir(servico.id).subscribe({
      next: () => {
        this.feedback = 'Servico excluido com sucesso.';
        this.carregarListas();
      },
      error: (error) => {
        this.error = this.getErrorMessage(error, 'Nao foi possivel excluir o servico.');
      },
    });
  }

  protected abrirCriacao(): void {
    this.criandoServico = true;
    this.criarForm.reset();
    this.limparMensagens();
  }

  protected criarServico(): void {
    if (this.criarForm.invalid) {
      this.criarForm.markAllAsTouched();
      return;
    }

    this.limparMensagens();
    this.adminServicosService.criar(this.criarForm.getRawValue()).subscribe({
      next: () => {
        this.feedback = 'Servico criado com sucesso.';
        this.fecharModais();
        this.carregarListas();
      },
      error: (error) => {
        this.error = this.getErrorMessage(error, 'Nao foi possivel criar o servico.');
      },
    });
  }

  protected fecharModais(): void {
    this.servicoEmCorrecao = null;
    this.servicoEmSubstituicao = null;
    this.criandoServico = false;
  }

  private carregarListas(): void {
    forkJoin({
      ativos: this.adminServicosService.getServicosAtivos(),
      pendentes: this.adminServicosService.getServicosPendentes(),
    }).subscribe({
      next: ({ ativos, pendentes }) => {
        this.servicosAtivos = this.ordenarPorNome(ativos);
        this.servicosPendentes = this.ordenarPorNome(pendentes);
      },
      error: (error) => {
        this.error = this.getErrorMessage(error, 'Nao foi possivel carregar os servicos.');
      },
    });
  }

  private ordenarPorNome<T extends AdminServico>(servicos: T[]): T[] {
    return [...servicos].sort((a, b) => a.nome.localeCompare(b.nome, 'pt-BR'));
  }

  private get termoBuscaNormalizado(): string {
    return this.termoBusca.trim().toLowerCase();
  }

  private limparMensagens(): void {
    this.feedback = '';
    this.error = '';
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse && typeof error.error === 'string') {
      return error.error;
    }

    if (error instanceof HttpErrorResponse && error.error?.mensagem) {
      return error.error.mensagem;
    }

    return fallback;
  }
}
