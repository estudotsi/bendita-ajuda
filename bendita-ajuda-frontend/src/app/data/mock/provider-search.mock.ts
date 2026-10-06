import { Category, Provider, ProviderQuery } from '../../core/models';
import { normalizeText } from '../../core/utils/text';

/**
 * Busca feita no navegador enquanto não há backend.
 * Quando a API existir, esta lógica passa para o servidor e este arquivo some.
 */

/** Palavras comuns que não ajudam a descobrir o serviço. */
const STOP_WORDS = new Set([
  'a', 'o', 'as', 'os', 'de', 'da', 'do', 'das', 'dos', 'e', 'em', 'no', 'na', 'um', 'uma',
  'meu', 'minha', 'para', 'pra', 'com', 'que', 'nao', 'esta', 'estou', 'ta', 'tem', 'muito',
  'preciso', 'quero', 'alguem', 'consertar', 'conserto', 'arrumar', 'trocar', 'troca',
  'instalar', 'ajuda', 'servico', 'casa', 'aqui',
]);

function queryWords(query: string): string[] {
  return query.split(' ').filter((w) => w.length >= 3 && !STOP_WORDS.has(w));
}

/** O termo aparece inteiro na frase, ou alguma palavra digitada é o começo dele. */
function termMatches(term: string, query: string, words: string[]): boolean {
  const t = normalizeText(term);
  if ((' ' + query + ' ').includes(' ' + t + ' ')) return true;
  const termWords = t.split(' ');
  return words.some((w) => termWords.some((tw) => tw.startsWith(w)));
}

export function searchProvidersMock(
  providers: Provider[],
  categories: Category[],
  { categoryId, text, neighborhood }: ProviderQuery,
): Provider[] {
  const query = normalizeText(text ?? '');
  const words = queryWords(query);

  const matchedCategoryIds = new Set(
    categories
      .filter((c) => [c.name, ...c.keywords].some((term) => termMatches(term, query, words)))
      .map((c) => c.id),
  );

  return providers
    .filter((p) => !categoryId || p.category.id === categoryId)
    .filter((p) => {
      if (!query) return true;
      const nameWords = normalizeText(p.name).split(' ');
      const nameMatches = words.some((w) => nameWords.some((nw) => nw.startsWith(w)));
      return matchedCategoryIds.has(p.category.id) || nameMatches;
    })
    .sort((a, b) => {
      const aNear = a.neighborhood === neighborhood ? 0 : 1;
      const bNear = b.neighborhood === neighborhood ? 0 : 1;
      return aNear - bNear || b.rating - a.rating;
    });
}
