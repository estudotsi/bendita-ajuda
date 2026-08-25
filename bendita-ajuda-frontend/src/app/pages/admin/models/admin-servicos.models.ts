export interface AdminServico {
  id: number;
  nome: string;
}

export interface AdminServicoPendente extends AdminServico {
  status: string;
}

export interface CorrigirAprovarServicoRequest {
  nome: string;
}

export interface SubstituirServicoRequest {
  servicoCorretoId: number;
}

export interface CriarServicoRequest {
  nome: string;
}
