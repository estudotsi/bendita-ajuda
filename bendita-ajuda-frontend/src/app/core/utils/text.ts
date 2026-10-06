/** Deixa o texto em minúsculas, sem acentos e com espaços simples. */
export function normalizeText(value: string): string {
  return value
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, ' ')
    .trim();
}

const NAME_CONNECTORS = new Set(['da', 'das', 'de', 'do', 'dos', 'e']);

/** Iniciais do nome (primeiro e último), ex.: "Maria das Dores" → "MD". */
export function initialsOf(name: string): string {
  const parts = name
    .trim()
    .split(/\s+/)
    .filter((p) => p && !NAME_CONNECTORS.has(p.toLowerCase()));
  if (parts.length === 0) return '?';
  const first = parts[0][0];
  const last = parts.length > 1 ? parts[parts.length - 1][0] : '';
  return (first + last).toUpperCase();
}
