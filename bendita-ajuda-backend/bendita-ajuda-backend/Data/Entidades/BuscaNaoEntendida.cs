namespace bendita_ajuda_backend.Data.Entidades;

/// <summary>
/// Texto buscado que não combinou com nenhum serviço nem com nome de prestador
/// (ex.: "porta emperrada"). Uma linha por texto, contando as repetições.
/// O admin olha a lista e ensina a palavra a um serviço, ou apaga.
/// </summary>
public class BuscaNaoEntendida
{
    public int Id { get; set; }

    /// <summary>Normalizado (sem acento, minúsculas): textos iguais caem na mesma linha.</summary>
    public string Texto { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    public DateTime PrimeiraVez { get; set; }

    public DateTime UltimaVez { get; set; }
}
