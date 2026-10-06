import { Component } from '@angular/core';

/** Cartão "fantasma" exibido enquanto a lista carrega. */
@Component({
  selector: 'app-provider-card-skeleton',
  standalone: false,
  template: `
    <div class="card">
      <div class="row">
        <span class="bone bone--avatar"></span>
        <div class="lines">
          <span class="bone bone--title"></span>
          <span class="bone"></span>
          <span class="bone bone--short"></span>
        </div>
      </div>
      <span class="bone bone--button"></span>
    </div>
  `,
  styleUrl: './provider-card-skeleton.scss',
})
export class ProviderCardSkeleton {}
