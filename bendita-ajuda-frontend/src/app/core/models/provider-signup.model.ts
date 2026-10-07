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

/** Tela "Buscas sem resultado" do admin (GET /api/admin/buscas). */
export interface UnmatchedSearches {
  /** Textos que não combinaram com nenhum serviço, os mais repetidos primeiro. */
  naoEntendidas: NotUnderstoodSearch[];
  /** Serviços procurados em cidades onde ainda não há prestador deles. */
  semPrestador: SearchWithoutProvider[];
}

export interface NotUnderstoodSearch {
  id: number;
  texto: string;
  quantidade: number;
  ultimaVez: string;
}

export interface SearchWithoutProvider {
  servicoId: string;
  servicoNome: string;
  /** Vazio quando quem buscou não tinha informado o CEP. */
  cidade: string;
  uf: string;
  quantidade: number;
  ultimaVez: string;
}
