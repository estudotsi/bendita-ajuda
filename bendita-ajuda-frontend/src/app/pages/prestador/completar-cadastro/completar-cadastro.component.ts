import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, catchError, map, of, switchMap } from 'rxjs';

import { ServicoAtivo } from '../models/prestador.models';
import { PrestadorService } from '../services/prestador.service';
import {
  formatBrazilianPhoneInput,
  normalizeBrazilianPhoneToE164,
} from '../../../shared/utils/phone-normalizer';

@Component({
  selector: 'app-completar-cadastro',
  standalone: false,
  templateUrl: './completar-cadastro.component.html',
  styleUrl: './completar-cadastro.component.scss',
})
export class CompletarCadastroComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly limiteServicosVisiveis = 9;

  protected servicos: ServicoAtivo[] = [];
  protected servicosSelecionados = new Set<number>();
  protected termoBuscaServico = '';
  protected feedback = '';
  protected error = '';
  protected sugestaoFeedback = '';
  protected sugestaoError = '';
  protected cepFeedback = '';
  protected cepError = '';
  private latitudeEndereco: number | null = null;
  private longitudeEndereco: number | null = null;

  protected readonly enderecoForm = this.formBuilder.nonNullable.group({
    telefoneWhatsapp: ['', [Validators.required, Validators.minLength(14)]],
    cep: ['', [Validators.required, Validators.minLength(8)]],
    rua: ['', [Validators.required]],
    numero: ['', [Validators.required]],
    bairro: ['', [Validators.required]],
    cidade: ['', [Validators.required]],
    estado: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(2)]],
  });

  protected readonly sugestaoForm = this.formBuilder.nonNullable.group({
    nome: [''],
  });

  constructor(
    private readonly prestadorService: PrestadorService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.carregarServicos();
  }

  protected toggleServico(servicoId: number, checked: boolean): void {
    if (checked) {
      this.servicosSelecionados.add(servicoId);
      return;
    }

    this.servicosSelecionados.delete(servicoId);
  }

  protected buscarCep(): void {
    this.cepFeedback = '';
    this.cepError = '';
    const cep = this.enderecoForm.controls.cep.value.replace(/\D/g, '');

    if (!cep) {
      return;
    }

    if (cep.length !== 8) {
      this.cepError = 'Informe um CEP com 8 digitos.';
      return;
    }

    this.enderecoForm.controls.cep.setValue(cep);
    this.latitudeEndereco = null;
    this.longitudeEndereco = null;

    this.prestadorService.buscarEnderecoPorCep(cep).subscribe({
      next: (endereco) => {
        const latitude = endereco.location?.coordinates?.latitude ?? null;
        const longitude = endereco.location?.coordinates?.longitude ?? null;
        this.latitudeEndereco = latitude;
        this.longitudeEndereco = longitude;

        this.enderecoForm.patchValue({
          rua: endereco.street || this.enderecoForm.controls.rua.value,
          bairro: endereco.neighborhood || this.enderecoForm.controls.bairro.value,
          cidade: endereco.city || this.enderecoForm.controls.cidade.value,
          estado: endereco.state || this.enderecoForm.controls.estado.value,
        });

        if (latitude !== null && longitude !== null) {
          this.cepFeedback = 'Endereco e localizacao preenchidos pelo CEP.';
          return;
        }

        this.cepFeedback = 'Endereco preenchido pelo CEP. Buscando coordenadas pelo endereco...';
        this.preencherCoordenadasPorEndereco();
      },
      error: () => {
        this.cepError = 'Nao foi possivel buscar o CEP agora.';
      },
    });
  }

  protected formatarTelefoneWhatsapp(): void {
    const control = this.enderecoForm.controls.telefoneWhatsapp;
    const formattedPhone = formatBrazilianPhoneInput(control.value);

    if (formattedPhone !== control.value) {
      control.setValue(formattedPhone, { emitEvent: false });
    }
  }

  protected get servicosFiltrados(): ServicoAtivo[] {
    const termo = this.termoBuscaServico.trim().toLowerCase();
    const servicoPrestador = this.servicos.find((servico) => this.isServicoPrestador(servico));
    const servicosSemPrestador = this.servicos.filter((servico) => !this.isServicoPrestador(servico));
    const servicosFiltrados = termo
      ? servicosSemPrestador.filter((servico) => servico.nome.toLowerCase().includes(termo))
      : servicosSemPrestador;
    const limiteSemPrestador = servicoPrestador
      ? this.limiteServicosVisiveis - 1
      : this.limiteServicosVisiveis;

    return [
      ...(servicoPrestador ? [servicoPrestador] : []),
      ...servicosFiltrados.slice(0, limiteSemPrestador),
    ];
  }

  protected get totalServicosFiltrados(): number {
    const termo = this.termoBuscaServico.trim().toLowerCase();
    const servicosSemPrestador = this.servicos.filter((servico) => !this.isServicoPrestador(servico));
    const totalSemPrestador = termo
      ? servicosSemPrestador.filter((servico) => servico.nome.toLowerCase().includes(termo))
      : servicosSemPrestador;

    return (
      totalSemPrestador.length +
      (this.servicos.some((servico) => this.isServicoPrestador(servico)) ? 1 : 0)
    );
  }

  protected get temMaisServicos(): boolean {
    return this.totalServicosFiltrados > this.servicosFiltrados.length;
  }

  protected sugerirServico(): void {
    this.sugestaoFeedback = '';
    this.sugestaoError = '';
    const nome = this.sugestaoForm.controls.nome.value.trim();

    if (!nome) {
      this.sugestaoError = 'Informe o nome do servico que deseja sugerir.';
      return;
    }

    this.prestadorService.sugerirServico({ nome }).subscribe({
      next: () => {
        this.sugestaoFeedback = 'Sugestao enviada para aprovacao do admin.';
        this.sugestaoForm.reset();
      },
      error: (error) => {
        this.sugestaoError = this.getErrorMessage(error, 'Nao foi possivel enviar a sugestao.');
      },
    });
  }

  protected finalizarCadastro(): void {
    this.feedback = '';
    this.error = '';

    if (this.enderecoForm.invalid) {
      this.enderecoForm.markAllAsTouched();
      this.error = 'Preencha todos os dados do endereco.';
      return;
    }

    const telefoneWhatsapp = normalizeBrazilianPhoneToE164(
      this.enderecoForm.controls.telefoneWhatsapp.value,
    );

    if (!telefoneWhatsapp) {
      this.enderecoForm.controls.telefoneWhatsapp.markAsTouched();
      this.error = 'Informe o WhatsApp com DDD. Exemplo: (61) 99999-9999.';
      return;
    }

    if (!this.servicosSelecionados.size) {
      this.error = 'Selecione pelo menos um servico.';
      return;
    }

    this.obterCoordenadasParaEndereco()
      .pipe(
        switchMap((coordenadas) =>
          this.prestadorService.salvarEndereco({
            ...this.enderecoForm.getRawValue(),
            telefoneWhatsapp,
            latitude: coordenadas.latitude,
            longitude: coordenadas.longitude,
          }),
        ),
      )
      .pipe(
        switchMap(() =>
          this.prestadorService.salvarServicos({
            servicosIds: Array.from(this.servicosSelecionados),
          }),
        ),
        switchMap(() => this.prestadorService.getStatusCadastro()),
      )
      .subscribe({
        next: (status) => {
          if (status.cadastroCompleto) {
            this.router.navigate(['/prestador/home']);
            return;
          }

          this.feedback = 'Dados salvos. Ainda existem pendencias no cadastro.';
        },
        error: (error) => {
          this.error = this.getErrorMessage(error, 'Nao foi possivel finalizar o cadastro.');
        },
      });
  }

  private carregarServicos(): void {
    this.prestadorService.getServicosAtivos().subscribe({
      next: (servicos) => {
        this.servicos = this.ordenarServicos(servicos);
      },
      error: (error) => {
        this.error = this.getErrorMessage(error, 'Nao foi possivel carregar os servicos.');
      },
    });
  }

  private ordenarServicos(servicos: ServicoAtivo[]): ServicoAtivo[] {
    return [...servicos].sort((a, b) => {
      const aEhPrestador = this.isServicoPrestador(a);
      const bEhPrestador = this.isServicoPrestador(b);

      if (aEhPrestador && !bEhPrestador) {
        return -1;
      }

      if (!aEhPrestador && bEhPrestador) {
        return 1;
      }

      return a.nome.localeCompare(b.nome, 'pt-BR');
    });
  }

  private isServicoPrestador(servico: ServicoAtivo): boolean {
    return servico.nome.trim().toLowerCase() === 'prestador';
  }

  private obterCoordenadasParaEndereco(): Observable<{ latitude: number | null; longitude: number | null }> {
    if (this.latitudeEndereco !== null && this.longitudeEndereco !== null) {
      return of({
        latitude: this.latitudeEndereco,
        longitude: this.longitudeEndereco,
      });
    }

    return this.obterCoordenadasPorEndereco().pipe(
      switchMap((coordenadas) =>
        coordenadas.latitude !== null && coordenadas.longitude !== null
          ? of(coordenadas)
          : this.obterLocalizacaoAtual(),
      ),
      catchError(() => this.obterLocalizacaoAtual()),
      catchError(() =>
        of({
          latitude: null,
          longitude: null,
        }),
      ),
    );
  }

  private preencherCoordenadasPorEndereco(): void {
    this.obterCoordenadasPorEndereco()
      .pipe(
        catchError(() =>
          of({
            latitude: null,
            longitude: null,
          }),
        ),
      )
      .subscribe((coordenadas) => {
        this.latitudeEndereco = coordenadas.latitude;
        this.longitudeEndereco = coordenadas.longitude;

        if (coordenadas.latitude !== null && coordenadas.longitude !== null) {
          this.cepFeedback = 'Endereco preenchido pelo CEP e coordenadas encontradas.';
          return;
        }

        this.cepFeedback = 'Endereco preenchido pelo CEP. Tentando obter sua localizacao atual...';
        this.preencherLocalizacaoAtual();
      });
  }

  private obterCoordenadasPorEndereco(): Observable<{ latitude: number | null; longitude: number | null }> {
    const endereco = this.montarEnderecoParaGeocoding();

    if (!endereco) {
      return of({ latitude: null, longitude: null });
    }

    return this.prestadorService.buscarCoordenadasPorEndereco(endereco).pipe(
      map((resultados) => {
        const resultado = resultados[0];
        const latitude = resultado ? Number(resultado.lat) : Number.NaN;
        const longitude = resultado ? Number(resultado.lon) : Number.NaN;

        if (!Number.isFinite(latitude) || !Number.isFinite(longitude)) {
          return {
            latitude: null,
            longitude: null,
          };
        }

        return {
          latitude,
          longitude,
        };
      }),
    );
  }

  private montarEnderecoParaGeocoding(): string {
    const endereco = this.enderecoForm.getRawValue();

    return [
      endereco.rua,
      endereco.numero,
      endereco.bairro,
      endereco.cidade,
      endereco.estado,
      'Brasil',
    ]
      .map((parte) => parte.trim())
      .filter(Boolean)
      .join(', ');
  }

  private preencherLocalizacaoAtual(): void {
    this.obterLocalizacaoAtual()
      .pipe(
        catchError(() =>
          of({
            latitude: null,
            longitude: null,
          }),
        ),
      )
      .subscribe((coordenadas) => {
        this.latitudeEndereco = coordenadas.latitude;
        this.longitudeEndereco = coordenadas.longitude;
        this.cepFeedback =
          coordenadas.latitude !== null && coordenadas.longitude !== null
            ? 'Endereco preenchido pelo CEP e localizacao atual capturada.'
            : 'Endereco preenchido pelo CEP. Nao foi possivel capturar a localizacao; ela sera enviada como nula.';
      });
  }

  private obterLocalizacaoAtual(): Observable<{ latitude: number | null; longitude: number | null }> {
    return new Observable((observer) => {
      if (!navigator.geolocation) {
        observer.next({ latitude: null, longitude: null });
        observer.complete();
        return;
      }

      navigator.geolocation.getCurrentPosition(
        (position) => {
          observer.next({
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
          });
          observer.complete();
        },
        (error) => observer.error(error),
        {
          enableHighAccuracy: true,
          timeout: 8000,
          maximumAge: 60000,
        },
      );
    });
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
