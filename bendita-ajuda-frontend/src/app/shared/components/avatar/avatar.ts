import { Component, computed, input, linkedSignal } from '@angular/core';
import { initialsOf } from '../../../core/utils/text';

/** Cores escuras para as iniciais (texto branco com contraste AA). */
const INITIALS_COLORS = ['#0f5c6e', '#6237a0', '#9a3c14', '#2c6527', '#1b5aa0', '#9e2049', '#674a2a', '#0c6570'];

/** Foto redonda da pessoa; sem foto (ou se falhar), mostra as iniciais coloridas. */
@Component({
  selector: 'app-avatar',
  standalone: false,
  templateUrl: './avatar.html',
  styleUrl: './avatar.scss',
  host: {
    '[style.width.px]': 'size()',
    '[style.height.px]': 'size()',
    '[style.font-size.px]': 'size() * 0.38',
  },
})
export class Avatar {
  readonly name = input.required<string>();
  readonly photoUrl = input<string>();
  readonly size = input(64);

  /** Volta a tentar a foto sempre que a URL mudar. */
  protected readonly photoFailed = linkedSignal(() => {
    this.photoUrl();
    return false;
  });

  protected readonly initials = computed(() => initialsOf(this.name()));

  protected readonly color = computed(() => {
    const hash = [...this.name()].reduce((acc, ch) => acc + ch.charCodeAt(0), 0);
    return INITIALS_COLORS[hash % INITIALS_COLORS.length];
  });
}
