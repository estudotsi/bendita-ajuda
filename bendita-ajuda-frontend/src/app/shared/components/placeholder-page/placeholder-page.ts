import { Component, input } from '@angular/core';

/** Página "em breve", usada pelas telas que ainda serão construídas. */
@Component({
  selector: 'app-placeholder-page',
  standalone: false,
  templateUrl: './placeholder-page.html',
  styleUrl: './placeholder-page.scss',
})
export class PlaceholderPage {
  readonly heading = input.required<string>();
  readonly description = input.required<string>();
  readonly icon = input('hand-helping');
}
