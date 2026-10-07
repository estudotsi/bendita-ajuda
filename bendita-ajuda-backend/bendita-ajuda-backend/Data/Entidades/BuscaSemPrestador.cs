namespace bendita_ajuda_backend.Data.Entidades;

/// <summary>
/// Procuraram um serviço numa região onde ainda não há prestador dele
/// (ex.: Encanador em Brasília - DF). Mostra onde vale a pena buscar prestadores.
/// Uma linha por serviço + cidade, contando as repetições.
/// </summary>
public class BuscaSemPrestador
{
    public string ServicoId { get; set; } = string.Empty;

    /// <summary>Vazio quando a pessoa ainda não informou onde está.</summary>
    public string Cidade { get; set; } = string.Empty;

    public string Uf { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    public DateTime UltimaVez { get; set; }
}
