import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { ServicoAtivoLanding } from '../models/principal.models';
import { PrincipalService } from '../services/principal.service';

@Component({
  selector: 'app-landing-page',
  standalone: false,
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.scss',
})
export class LandingPageComponent implements OnInit {
  private readonly latitudePadrao = -15.783009786096823;
  private readonly longitudePadrao = -47.91385411568078;

  protected servicos: ServicoAtivoLanding[] = [];
  protected termoBuscaServico = '';
  protected servicoSelecionadoId: number | null = null;
  protected buscaError = '';

  protected readonly acessos = [
    {
      titulo: 'Doacoes',
      descricao: 'Fluxo para doadores acompanharem campanhas, itens e contribuicoes.',
      rota: '/doacoes',
    },
    {
      titulo: 'Voluntariado',
      descricao: 'Area para voluntarios consultarem oportunidades e acoes abertas.',
      rota: '/voluntariado',
    },
    {
      titulo: 'Administracao',
      descricao: 'Painel reservado para gestao de usuarios, cadastros e operacao.',
      rota: '/administracao',
    },
  ];

  constructor(
    private readonly principalService: PrincipalService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    const email = this.route.snapshot.queryParamMap.get('email');
    const token = this.route.snapshot.queryParamMap.get('token');

    if (email && token) {
      this.router.navigate(['/confirm-email'], {
        queryParams: {
          email,
          token,
        },
        replaceUrl: true,
      });
      return;
    }

    this.carregarServicos();
  }

  protected get servicosFiltrados(): ServicoAtivoLanding[] {
    const termo = this.termoBuscaServico.trim().toLowerCase();
    const servicos = termo
      ? this.servicos.filter((servico) => servico.nome.toLowerCase().includes(termo))
      : this.servicos;

    return servicos.slice(0, 12);
  }

  protected buscarProfissionais(): void {
    this.buscaError = '';

    if (!this.servicoSelecionadoId) {
      this.buscaError = 'Selecione um serviço para buscar profissionais.';
      return;
    }

    if (!navigator.geolocation) {
      this.navegarParaPrestadores(this.latitudePadrao, this.longitudePadrao);
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        this.navegarParaPrestadores(position.coords.latitude, position.coords.longitude);
      },
      () => {
        this.navegarParaPrestadores(this.latitudePadrao, this.longitudePadrao);
      },
      {
        enableHighAccuracy: true,
        timeout: 10000,
      },
    );
  }

  private navegarParaPrestadores(latitude: number, longitude: number): void {
    this.router.navigate(['/prestadores'], {
      queryParams: {
        servicoId: this.servicoSelecionadoId,
        latitude,
        longitude,
      },
    });
  }

  private carregarServicos(): void {
    this.principalService.getServicosAtivos().subscribe({
      next: (servicos) => {
        this.servicos = [...servicos].sort((a, b) => a.nome.localeCompare(b.nome, 'pt-BR'));
      },
      error: () => {
        this.servicos = [];
      },
    });
  }
}
