/** Onde a pessoa está, a partir do CEP que ela informou. */
export interface UserLocation {
  /** Vazio em cidades pequenas com CEP único. */
  neighborhood: string | null;
  city: string;
  /** Sigla do estado (ex.: "DF"). */
  uf: string;
}
