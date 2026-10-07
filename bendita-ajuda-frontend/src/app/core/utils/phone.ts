/** Só os dígitos do celular (DDD + número), no máximo 11. */
export function phoneDigits(value: string): string {
  return value.replace(/\D/g, '').slice(0, 11);
}

/** Formata enquanto a pessoa digita, ex.: "61999998888" → "(61) 99999-8888". */
export function formatPhone(value: string): string {
  const d = phoneDigits(value);
  if (d.length === 0) return '';
  if (d.length <= 2) return `(${d}`;
  const ddd = d.slice(0, 2);
  const rest = d.slice(2);
  if (rest.length <= 4) return `(${ddd}) ${rest}`;
  const split = rest.length === 9 ? 5 : 4; // celular (9 dígitos) ou fixo (8)
  return `(${ddd}) ${rest.slice(0, split)}-${rest.slice(split)}`;
}

/** DDD + número com 10 ou 11 dígitos, sem começar por zero. */
export function isValidPhone(value: string): boolean {
  const d = phoneDigits(value);
  return (d.length === 10 || d.length === 11) && d[0] !== '0';
}
