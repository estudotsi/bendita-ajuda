import { CategorySummary } from './category.model';

/** Prestador de serviço. Formato esperado da futura API. */
export interface Provider {
  id: string;
  name: string;
  category: CategorySummary;
  neighborhood: string;
  /** Média de 0 a 5. */
  rating: number;
  reviewCount: number;
  /** Somente dígitos, com DDD e sem o 55 (ex.: "61999998888"). */
  whatsapp: string;
  photoUrl?: string;
}

/** Parâmetros de busca de prestadores (viram query string na API). */
export interface ProviderQuery {
  categoryId?: string | null;
  text?: string;
  /** Bairro de referência: prestadores dele aparecem primeiro. */
  neighborhood?: string;
}
