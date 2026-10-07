namespace bendita_ajuda_backend.Services;

/// <summary>Leitura e escrita do campo <c>Servico.PalavrasChave</c> ("tomada, chuveiro, ...").</summary>
public static class PalavrasChave
{
    /// <summary>Palavras-chave já normalizadas (sem acento, minúsculas).</summary>
    public static IReadOnlyList<string> Separar(string palavrasChave) =>
        palavrasChave.Split(',').Select(Texto.Normalizar).Where(p => p != "").ToList();

    /// <summary>Acrescenta o texto (normalizado) se ainda não estiver na lista.</summary>
    public static string Acrescentar(string palavrasChave, string texto)
    {
        var nova = Texto.Normalizar(texto);
        if (nova == "" || Separar(palavrasChave).Contains(nova))
            return palavrasChave;

        return string.IsNullOrWhiteSpace(palavrasChave) ? nova : $"{palavrasChave}, {nova}";
    }
}
