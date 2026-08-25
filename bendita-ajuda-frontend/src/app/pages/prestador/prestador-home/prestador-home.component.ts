import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { switchMap } from 'rxjs';

import { MeuCadastroPrestador, StatusCadastroPrestador } from '../models/prestador.models';
import { PrestadorService } from '../services/prestador.service';

@Component({
  selector: 'app-prestador-home',
  standalone: false,
  templateUrl: './prestador-home.component.html',
  styleUrl: './prestador-home.component.scss',
})
export class PrestadorHomeComponent implements OnInit {
  protected status: StatusCadastroPrestador | null = null;
  protected cadastro: MeuCadastroPrestador | null = null;
  protected error = '';

  constructor(
    private readonly prestadorService: PrestadorService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.prestadorService
      .getStatusCadastro()
      .pipe(
        switchMap((status) => {
        this.status = status;

        if (!status.cadastroCompleto) {
          this.router.navigate(['/prestador/completar-cadastro']);
        }

          return this.prestadorService.getMeuCadastro();
        }),
      )
      .subscribe({
        next: (cadastro) => {
          this.cadastro = cadastro;
      },
      error: () => {
          this.error = 'Nao foi possivel carregar o dashboard do prestador.';
      },
    });
  }
}
