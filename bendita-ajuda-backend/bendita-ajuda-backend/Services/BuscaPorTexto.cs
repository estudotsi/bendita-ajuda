using bendita_ajuda_backend.Data.Entidades;

namespace bendita_ajuda_backend.Services;

/// <summary>
/// Descobre o serviço a partir do que a pessoa escreveu ou falou ("minha pia está vazando" → Encanador).
/// Não entende a frase: procura nela o nome e as palavras-chave de cada serviço.
/// </summary>
public static class BuscaPorTexto
{
    /// <summary>Palavras comuns que não ajudam a descobrir o serviço.</summary>
    private static readonly HashSet<string> PalavrasVazias =
    [
        "a", "o", "as", "os", "de", "da", "do", "das", "dos", "e", "em", "no", "na", "nos", "nas", "um", "uma",
        "meu", "minha", "meus", "minhas", "para", "pra", "com", "que", "nao", "esta", "estou", "ta", "tem",
        "muito", "preciso", "precisando", "quero", "alguem", "consertar", "conserto", "arrumar", "trocar", "troca",
        "instalar", "ajuda", "servico", "casa", "aqui", "por", "favor",
    ];

    /// <summary>Palavras da frase que valem para a busca (sem as vazias e com 3 letras ou mais).</summary>
    public static IReadOnlyList<string> PalavrasUteis(string textoNormalizado) =>
        textoNormalizado.Split(' ').Where(p => p.Length >= 3 && !PalavrasVazias.Contains(p)).Distinct().ToList();

    /// <summary>Serviços que combinam com o texto, na ordem dos botões.</summary>
    public static IReadOnlyList<Servico> ServicosDoTexto(IEnumerable<Servico> servicos, string texto)
    {
        var frase = Texto.Normalizar(texto);
        if (frase == "")
            return [];

        var palavras = PalavrasUteis(frase);
        return servicos
            .Where(s => Combina(s, frase, palavras))
            .OrderBy(s => s.Ordem)
            .ThenBy(s => s.Nome)
            .ToList();
    }

    private static bool Combina(Servico servico, string frase, IReadOnlyList<string> palavras)
    {
        // Nome do serviço: cada palavra vale sozinha ("tecnico" → Técnico de ar-condicionado).
        var palavrasDoNome = PalavrasUteis($"{Texto.Normalizar(servico.Nome)} {Texto.Normalizar(servico.NomePlural)}");
        if (palavrasDoNome.Any(n => ComecaCom(n, palavras)))
            return true;

        foreach (var termo in PalavrasChave.Separar(servico.PalavrasChave))
        {
            // Termo inteiro na frase: "quadro de luz", "nao gela", "ar".
            if ($" {frase} ".Contains($" {termo} "))
                return true;

            // Termo de uma palavra também vale enquanto a pessoa digita ("vaza" → vazamento)
            // e no plural ("tomadas" → tomada). Termo de várias palavras só vale inteiro:
            // assim "pendurar um quadro" não vira Eletricista por causa de "quadro de luz".
            if (!termo.Contains(' ') && ComecaCom(termo, palavras))
                return true;
        }
        return false;
    }

    /// <summary>Alguma palavra da frase (ou ela sem o plural) é o começo do termo.</summary>
    private static bool ComecaCom(string termo, IReadOnlyList<string> palavras) =>
        palavras.Any(p => FormasDaPalavra(p).Any(forma => termo.StartsWith(forma, StringComparison.Ordinal)));

    /// <summary>"luzes" → luzes, luze, luz. Só formas com 3 letras ou mais.</summary>
    private static IEnumerable<string> FormasDaPalavra(string palavra)
    {
        yield return palavra;
        if (palavra.EndsWith('s') && palavra.Length > 3)
            yield return palavra[..^1];
        if (palavra.EndsWith("es") && palavra.Length > 4)
            yield return palavra[..^2];
    }
}
