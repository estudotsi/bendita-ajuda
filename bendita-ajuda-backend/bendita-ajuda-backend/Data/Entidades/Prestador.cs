namespace bendita_ajuda_backend.Data.Entidades;

/// <summary>
/// Dados de quem oferece serviços. É a mesma conta do <see cref="Usuario"/> (1:1):
/// ser prestador = ter um registro aqui.
/// </summary>
public class Prestador
{
    /// <summary>Chave primária e, ao mesmo tempo, ligação com o usuário.</summary>
    public Guid UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    /// <summary>
    /// Somente os 8 dígitos. Nunca é mostrado ao cliente (em muitos lugares um CEP é uma rua só);
    /// serve para preencher bairro, cidade e UF.
    /// </summary>
    public string Cep { get; set; } = string.Empty;

    /// <summary>Vazio em cidades pequenas com CEP único. Para o cliente: bairro ou, se vazio, a cidade.</summary>
    public string? Bairro { get; set; }

    public string Cidade { get; set; } = string.Empty;

    /// <summary>Sigla do estado (ex.: "DF").</summary>
    public string Uf { get; set; } = string.Empty;

    /// <summary>Texto livre: "Trabalho há 20 anos com instalação elétrica...". Opcional.</summary>
    public string? Bio { get; set; }

    public string? FotoUrl { get; set; }

    /// <summary>
    /// Aparece na busca? Começa true (sem aprovação manual); vira false
    /// quando o perfil é escondido por denúncias.
    /// </summary>
    public bool Visivel { get; set; } = true;

    public DateTime CriadoEm { get; set; }

    /// <summary>Serviços que ele faz (muitos para muitos).</summary>
    public List<Servico> Servicos { get; set; } = [];

    /// <summary>Serviços que ele digitou e não estão na lista, aguardando o admin.</summary>
    public List<ServicoSugerido> Sugestoes { get; set; } = [];
}
