export interface ServicoAtivoLanding {
  id: number;
  nome: string;
}

export interface PrestadorPorServico {
  id: number;
  nome: string;
  telefoneWhatsapp: string;
  cidade: string;
  estado: string;
  latitude: number;
  longitude: number;
  distanciaKm: number;
}
