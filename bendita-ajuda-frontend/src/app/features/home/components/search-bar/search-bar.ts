import { Component, OnDestroy, computed, inject, input, linkedSignal, output } from '@angular/core';
import { SpeechRecognitionService } from '../../../../core/services/speech-recognition.service';

/** Campo de busca grande, com botão de falar (quando o navegador permite). */
@Component({
  selector: 'app-search-bar',
  standalone: false,
  templateUrl: './search-bar.html',
  styleUrl: './search-bar.scss',
})
export class SearchBar implements OnDestroy {
  protected readonly speech = inject(SpeechRecognitionService);

  readonly value = input('');
  readonly valueChange = output<string>();
  /** A pessoa terminou de escrever (Enter) ou de falar. */
  readonly submitted = output<void>();

  /** Texto no campo; acompanha `value` quando o pai limpa a busca. */
  protected readonly text = linkedSignal(() => this.value());

  protected readonly statusMessage = computed(() => {
    if (this.speech.isListening()) return 'Estou ouvindo... fale agora';
    switch (this.speech.error()) {
      case 'sem-permissao':
        return 'Não conseguimos usar o microfone. Permita o uso do microfone no navegador e tente de novo.';
      case 'nao-ouviu':
        return 'Não ouvi nada. Toque em "Falar" e tente de novo.';
      case 'falhou':
        return 'Não deu para ouvir agora. Tente escrever.';
      default:
        return '';
    }
  });

  protected onInput(event: Event): void {
    this.update((event.target as HTMLInputElement).value);
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();
    (document.activeElement as HTMLElement | null)?.blur(); // fecha o teclado do celular
    this.submitted.emit();
  }

  protected clear(input: HTMLInputElement): void {
    this.update('');
    input.focus();
  }

  protected toggleMic(): void {
    if (this.speech.isListening()) {
      this.speech.stop();
      return;
    }
    this.speech.start((spoken) => {
      this.update(spoken);
      this.submitted.emit();
    });
  }

  ngOnDestroy(): void {
    this.speech.stop();
  }

  private update(value: string): void {
    this.text.set(value);
    this.valueChange.emit(value);
  }
}
