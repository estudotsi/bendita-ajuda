/** Aparência visual de cada categoria (fica no front, não vem da API). */
export interface CategoryAppearance {
  icon: string;
  /** Fundo suave do círculo do ícone. */
  background: string;
  /** Cor do ícone (contraste AA sobre o fundo). */
  color: string;
}

const APPEARANCES: Record<string, CategoryAppearance> = {
  eletricista: { icon: 'zap', background: '#fff1cc', color: '#7a4f00' },
  encanador: { icon: 'droplets', background: '#dfeeff', color: '#1b5aa0' },
  faxina: { icon: 'sparkles', background: '#efe5ff', color: '#6237a0' },
  pedreiro: { icon: 'brick-wall', background: '#fde4d8', color: '#9a3c14' },
  pintor: { icon: 'paint-roller', background: '#ffe1eb', color: '#9e2049' },
  jardineiro: { icon: 'sprout', background: '#e0f3dd', color: '#2c6527' },
  'montador-de-moveis': { icon: 'hammer', background: '#efe7dc', color: '#674a2a' },
  'tecnico-ar-condicionado': { icon: 'air-vent', background: '#daf3f5', color: '#0c6570' },
};

const FALLBACK: CategoryAppearance = { icon: 'hand-helping', background: '#e4f1f4', color: '#0f5c6e' };

export function categoryAppearance(categoryId: string): CategoryAppearance {
  return APPEARANCES[categoryId] ?? FALLBACK;
}
