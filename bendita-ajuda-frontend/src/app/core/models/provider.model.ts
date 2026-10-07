import { Category, CategorySummary } from './category.model';
import { UserLocation } from './user-location.model';

/** Prestador de serviço como o cliente vê (busca e página do prestador). */
export interface Provider {
  id: string;
  name: string;
  /** Serviço principal: o que combinou com a busca, ou o primeiro da lista. */
  category: CategorySummary;
  /** Todos os serviços dele, o principal primeiro. */
  services: CategorySummary[];
  /** Ex.: "Asa Norte, Brasília - DF". O CEP nunca é mostrado. */
  place: string;
  bio: string | null;
  photoUrl?: string;
  /**
   * Somente dígitos, com DDD e sem o 55 (ex.: "61999998888").
   * A API só manda para quem entrou; para os outros vem null.
   */
  whatsapp: string | null;
}

/** Parâmetros de busca de prestadores (viram query string na API). */
export interface ProviderQuery {
  categoryId?: string | null;
  text?: string;
  /** Onde a pessoa está: os do mesmo bairro e cidade aparecem primeiro. Null = Brasil todo. */
  location: UserLocation | null;
}

/** Resposta da busca. */
export interface ProviderSearchResult {
  /** Serviços que a busca entendeu. Vazio com texto preenchido = não entendemos o que a pessoa precisa. */
  matchedCategories: Category[];
  providers: Provider[];
}
