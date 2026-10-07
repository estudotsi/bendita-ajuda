namespace bendita_ajuda_backend.Data.Entidades;

/// <summary>
/// Serviço padronizado (Eletricista, Encanador...). A lista é fixa, mantida por nós:
/// o prestador escolhe nela, e as palavras-chave ligam o que a pessoa digita ao serviço certo.
/// </summary>
public class Servico
{
    /// <summary>Identificador legível, usado também nas URLs e no front (ex.: "eletricista").</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Singular, para botões e para a mensagem do WhatsApp (ex.: "Eletricista").</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Plural, para títulos (ex.: "Eletricistas perto de você").</summary>
    public string NomePlural { get; set; } = string.Empty;

    /// <summary>
    /// Sinônimos e problemas que as pessoas digitam, separados por vírgula
    /// (ex.: "tomada, chuveiro, tecnico eletrico"). Sem acento: a busca ignora acentos.
    /// </summary>
    public string PalavrasChave { get; set; } = string.Empty;

    /// <summary>Posição na lista de botões.</summary>
    public int Ordem { get; set; }

    public List<Prestador> Prestadores { get; set; } = [];
}
