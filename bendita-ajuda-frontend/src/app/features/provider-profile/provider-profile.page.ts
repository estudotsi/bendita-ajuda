import { Component, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ProviderService } from '../../data/provider.service';

/** Página do prestador (placeholder: só o básico + botão de chamar). */
@Component({
  selector: 'app-provider-profile-page',
  standalone: false,
  templateUrl: './provider-profile.page.html',
  styleUrl: './provider-profile.page.scss',
})
export class ProviderProfilePage {
  private readonly providerService = inject(ProviderService);

  /** Vem da rota /prestador/:id. */
  readonly id = input.required<string>();

  protected readonly provider = rxResource({
    params: () => this.id(),
    stream: ({ params }) => this.providerService.getProviderById(params),
  });
}
