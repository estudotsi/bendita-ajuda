import { Component, computed, input } from '@angular/core';

type StarFill = 'full' | 'half' | 'empty';

/** Estrelas + nota + quantidade de avaliações, com texto completo para leitor de tela. */
@Component({
  selector: 'app-star-rating',
  standalone: false,
  templateUrl: './star-rating.html',
  styleUrl: './star-rating.scss',
})
export class StarRating {
  readonly rating = input.required<number>();
  readonly reviewCount = input.required<number>();

  protected readonly stars = computed<StarFill[]>(() =>
    [1, 2, 3, 4, 5].map((n) => {
      const r = this.rating();
      if (r >= n - 0.25) return 'full';
      if (r >= n - 0.75) return 'half';
      return 'empty';
    }),
  );

  protected readonly ratingText = computed(() =>
    this.rating().toLocaleString('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 }),
  );

  protected readonly reviewsText = computed(() => {
    const n = this.reviewCount();
    return `${n.toLocaleString('pt-BR')} ${n === 1 ? 'avaliação' : 'avaliações'}`;
  });

  /** Ex.: "4,8 de 5 estrelas, 32 avaliações". */
  protected readonly screenReaderText = computed(() => `${this.ratingText()} de 5 estrelas, ${this.reviewsText()}`);
}
