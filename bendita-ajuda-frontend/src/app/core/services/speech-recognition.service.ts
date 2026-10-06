import { Injectable, signal } from '@angular/core';

/** Tipos mínimos da Web Speech API (ainda não fazem parte do lib.dom do TypeScript). */
interface SpeechRecognition {
  lang: string;
  interimResults: boolean;
  maxAlternatives: number;
  continuous: boolean;
  onresult: ((event: { results: ArrayLike<ArrayLike<{ transcript: string }>> }) => void) | null;
  onerror: ((event: { error: string }) => void) | null;
  onend: (() => void) | null;
  start(): void;
  stop(): void;
}

type SpeechRecognitionCtor = new () => SpeechRecognition;

export type SpeechError = 'sem-permissao' | 'nao-ouviu' | 'falhou';

/**
 * Ditado por voz usando a Web Speech API (pt-BR).
 * Em navegadores sem suporte, `isSupported` é falso e o botão não aparece.
 */
@Injectable({ providedIn: 'root' })
export class SpeechRecognitionService {
  private readonly ctor: SpeechRecognitionCtor | null = this.findCtor();
  private recognition: SpeechRecognition | null = null;

  readonly isSupported = this.ctor !== null;
  readonly isListening = signal(false);
  readonly error = signal<SpeechError | null>(null);

  /** Começa a ouvir; chama `onText` com o que a pessoa falou. */
  start(onText: (text: string) => void): void {
    if (!this.ctor || this.isListening()) return;

    const recognition = new this.ctor();
    recognition.lang = 'pt-BR';
    recognition.interimResults = false;
    recognition.maxAlternatives = 1;
    recognition.continuous = false;

    recognition.onresult = (event) => {
      const text = event.results[0]?.[0]?.transcript ?? '';
      if (text.trim()) onText(text.trim());
    };
    recognition.onerror = (event) => {
      if (event.error === 'aborted') return;
      this.error.set(
        event.error === 'not-allowed' || event.error === 'service-not-allowed'
          ? 'sem-permissao'
          : event.error === 'no-speech'
            ? 'nao-ouviu'
            : 'falhou',
      );
    };
    recognition.onend = () => {
      this.isListening.set(false);
      this.recognition = null;
    };

    this.error.set(null);
    this.recognition = recognition;
    this.isListening.set(true);
    try {
      recognition.start();
    } catch {
      this.isListening.set(false);
      this.error.set('falhou');
    }
  }

  stop(): void {
    this.recognition?.stop();
  }

  private findCtor(): SpeechRecognitionCtor | null {
    if (typeof window === 'undefined') return null;
    const w = window as unknown as Record<string, SpeechRecognitionCtor | undefined>;
    return w['SpeechRecognition'] ?? w['webkitSpeechRecognition'] ?? null;
  }
}
