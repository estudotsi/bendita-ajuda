/** Só os 8 dígitos do CEP. */
export function cepDigits(value: string): string {
  return value.replace(/\D/g, '').slice(0, 8);
}

/** Formata enquanto a pessoa digita, ex.: "70040010" → "70040-010". */
export function formatCep(value: string): string {
  const d = cepDigits(value);
  return d.length > 5 ? `${d.slice(0, 5)}-${d.slice(5)}` : d;
}

/** Para o cliente: o bairro ou, se não tiver (cidade pequena), a cidade. */
export function placeLabel(place: { bairro: string | null; cidade: string; uf: string }): string {
  return place.bairro ? `${place.bairro}, ${place.cidade} - ${place.uf}` : `${place.cidade} - ${place.uf}`;
}
