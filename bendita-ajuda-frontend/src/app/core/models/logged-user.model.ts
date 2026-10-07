/** Quem está logado (resposta de /api/auth/eu). Os nomes seguem o JSON da API. */
export interface LoggedUser {
  id: string;
  nome: string;
  celular: string | null;
  papel: string;
  ehPrestador: boolean;
}
