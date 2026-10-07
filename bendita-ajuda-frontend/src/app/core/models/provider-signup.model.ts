import { ServiceOption } from './service-option.model';

/** Resposta de /api/cep/{cep}. Bairro vem vazio em cidades com CEP único. */
export interface CepAddress {
  cep: string;
  bairro: string | null;
  cidade: string;
  uf: string;
}

/** Corpo do POST /api/prestadores/eu. */
export interface ProviderSignupRequest {
  servicos: string[];
  outroServico: string | null;
  cep: string;
  bairro: string | null;
}

/** Meu cadastro de prestador (GET /api/prestadores/eu). */
export interface MyProviderProfile {
  cep: string;
  bairro: string | null;
  cidade: string;
  uf: string;
  visivel: boolean;
  servicos: ServiceOption[];
  /** Serviços digitados que o admin ainda não analisou. */
  sugestoesPendentes: string[];
}

/** Linha da tela de sugestões do admin (GET /api/admin/sugestoes). */
export interface PendingSuggestion {
  id: string;
  descricao: string;
  criadoEm: string;
  prestadorNome: string;
  cidade: string;
  uf: string;
}
