export interface StatusCadastroPrestador {
  temEndereco: boolean;
  temServicos: boolean;
  cadastroCompleto: boolean;
}

export interface ServicoAtivo {
  id: number;
  nome: string;
}

export interface EnderecoPrestadorRequest {
  telefoneWhatsapp: string;
  cep: string;
  rua: string;
  numero: string;
  bairro: string;
  cidade: string;
  estado: string;
  latitude: number | null;
  longitude: number | null;
}

export interface PrestadorServicosRequest {
  servicosIds: number[];
}

export interface SugestaoServicoRequest {
  nome: string;
}

export interface BrasilApiCepResponse {
  cep?: string;
  state?: string;
  city?: string;
  neighborhood?: string;
  street?: string;
  location?: {
    coordinates?: {
      latitude?: number | null;
      longitude?: number | null;
    };
  };
}

export interface NominatimSearchResponse {
  lat: string;
  lon: string;
  display_name?: string;
}

export interface MeuCadastroPrestador {
  id: number;
  nome: string;
  email: string;
  telefoneWhatsapp: string;
  cep: string;
  rua: string;
  numero: string;
  bairro: string;
  cidade: string;
  estado: string;
  servicos: ServicoPrestadorCadastro[];
  servicosAguardandoAprovacao: ServicoPrestadorAguardandoAprovacao[];
}

export interface ServicoPrestadorCadastro {
  id: number;
  nome: string;
}

export interface ServicoPrestadorAguardandoAprovacao extends ServicoPrestadorCadastro {
  status: string;
}
