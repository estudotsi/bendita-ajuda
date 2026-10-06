import { Component, computed, input, output } from '@angular/core';
import { Category } from '../../../../core/models';
import { categoryAppearance } from '../../../../core/icons/category-appearance';

/** Grade de botões grandes, um por categoria de serviço. */
@Component({
  selector: 'app-category-grid',
  standalone: false,
  templateUrl: './category-grid.html',
  styleUrl: './category-grid.scss',
})
export class CategoryGrid {
  readonly categories = input.required<Category[]>();
  readonly selectedId = input<string | null>(null);
  readonly loading = input(false);

  readonly selected = output<string>();

  protected readonly tiles = computed(() =>
    this.categories().map((category) => ({ category, look: categoryAppearance(category.id) })),
  );

  protected readonly skeletons = Array.from({ length: 8 }, (_, i) => i);
}
