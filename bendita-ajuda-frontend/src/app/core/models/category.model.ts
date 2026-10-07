/** Categoria de serviço (ex.: Eletricista), vinda de /api/servicos. */
export interface Category {
  id: string;
  /** Nome no singular, usado nos botões e na mensagem do WhatsApp. */
  name: string;
  /** Nome no plural, usado em títulos ("Eletricistas perto de você"). */
  pluralName: string;
}

/** Versão resumida da categoria que vem junto com cada prestador. */
export type CategorySummary = Pick<Category, 'id' | 'name'>;
