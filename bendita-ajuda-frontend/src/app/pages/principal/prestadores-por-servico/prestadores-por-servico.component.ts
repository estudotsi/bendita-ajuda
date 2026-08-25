import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { PrestadorPorServico } from '../models/principal.models';
import { GoogleContactsService } from '../services/google-contacts.service';
import { PrincipalService } from '../services/principal.service';
import {
  normalizeBrazilianPhoneForWhatsApp,
  normalizeBrazilianPhoneToE164,
} from '../../../shared/utils/phone-normalizer';

@Component({
  selector: 'app-prestadores-por-servico',
  standalone: false,
  templateUrl: './prestadores-por-servico.component.html',
  styleUrl: './prestadores-por-servico.component.scss',
})
export class PrestadoresPorServicoComponent implements OnInit {
  protected prestadores: PrestadorPorServico[] = [];
  protected error = '';
  protected contatosError = '';
  protected contatosStatus: 'idle' | 'loading' | 'done' | 'error' = 'idle';
  protected servicoId = 0;
  protected nomesAgendaPorPrestador = new Map<number, string>();

  constructor(
    private readonly principalService: PrincipalService,
    private readonly googleContactsService: GoogleContactsService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    const servicoId = Number(this.route.snapshot.queryParamMap.get('servicoId'));
    const latitude = Number(this.route.snapshot.queryParamMap.get('latitude'));
    const longitude = Number(this.route.snapshot.queryParamMap.get('longitude'));
    this.servicoId = servicoId;

    if (!servicoId) {
      this.error = 'Selecione um serviço para buscar profissionais.';
      return;
    }

    if (Number.isFinite(latitude) && Number.isFinite(longitude)) {
      this.carregarPrestadores(servicoId, latitude, longitude);
      return;
    }

    this.obterLocalizacao(servicoId);
  }

  protected voltar(): void {
    this.router.navigate(['/']);
  }

  protected getWhatsappUrl(prestador: PrestadorPorServico): string {
    const telefone = normalizeBrazilianPhoneForWhatsApp(prestador.telefoneWhatsapp);
    const mensagem = encodeURIComponent(
      `Ola, ${prestador.nome}. Encontrei seu perfil na Bendita Ajuda e gostaria de falar sobre um atendimento.`,
    );

    return `https://wa.me/${telefone}?text=${mensagem}`;
  }

  protected prestadorEstaNaAgenda(prestador: PrestadorPorServico): boolean {
    return this.nomesAgendaPorPrestador.has(prestador.id);
  }

  protected getNomeAgenda(prestador: PrestadorPorServico): string {
    return this.nomesAgendaPorPrestador.get(prestador.id) ?? '';
  }

  protected async verificarAgendaGoogle(): Promise<void> {
    this.contatosStatus = 'loading';
    this.contatosError = '';

    try {
      const telefonesContatos = await this.googleContactsService.getNormalizedContactPhones();
      const nomesAgendaPorPrestador = new Map<number, string>();

      for (const prestador of this.prestadores) {
        const telefonePrestador = normalizeBrazilianPhoneToE164(prestador.telefoneWhatsapp);
        const nomeAgenda = telefonePrestador ? telefonesContatos.get(telefonePrestador) : undefined;

        if (nomeAgenda) {
          nomesAgendaPorPrestador.set(prestador.id, nomeAgenda);
        }
      }

      this.nomesAgendaPorPrestador = nomesAgendaPorPrestador;
      this.contatosStatus = 'done';
    } catch {
      this.contatosStatus = 'error';
      this.contatosError =
        'Nao foi possivel ler seus contatos do Google. Verifique a permissao e tente novamente.';
      this.nomesAgendaPorPrestador = new Map();
    }
  }

  private obterLocalizacao(servicoId: number): void {
    if (!navigator.geolocation) {
      this.error = 'Seu navegador não permite buscar profissionais por distância.';
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        this.carregarPrestadores(servicoId, position.coords.latitude, position.coords.longitude);
      },
      () => {
        this.error = 'Permita o acesso à localização para buscar profissionais próximos.';
      },
      {
        enableHighAccuracy: true,
        timeout: 10000,
      },
    );
  }

  private carregarPrestadores(servicoId: number, latitude: number, longitude: number): void {
    this.principalService.getPrestadoresPorServico(servicoId, latitude, longitude).subscribe({
      next: (prestadores) => {
        this.prestadores = prestadores;
      },
      error: () => {
        this.error = 'Não foi possível carregar os profissionais desse serviço.';
      },
    });
  }
}
