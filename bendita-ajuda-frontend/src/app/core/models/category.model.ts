/** Categoria de serviço (ex.: Eletricista). Formato esperado da futura API. */
export interface Category {
  id: string;
  /** Nome no singular, usado nos botões e na mensagem do WhatsApp. */
  name: string;
  /** Nome no plural, usado em títulos ("Eletricistas perto de você"). */
  pluralName: string;
  /** Palavras que as pessoas usam para descrever o problema ("pia", "tomada"...). */
  keywords: string[];
}

/** Versão resumida da categoria que vem junto com cada prestador. */
export type CategorySummary = Pick<Category, 'id' | 'name'>;
