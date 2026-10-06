namespace bendita_ajuda_backend.Data.Entidades;

/// <summary>
/// Código enviado ao celular para entrar no app.
/// Fica ligado ao número (e não ao usuário), porque o usuário pode ainda não existir.
/// </summary>
public class CodigoVerificacao
{
    public Guid Id { get; set; }

    /// <summary>Número normalizado (somente dígitos).</summary>
    public string Celular { get; set; } = string.Empty;

    /// <summary>HMAC-SHA256 do código. O código puro nunca é salvo.</summary>
    public string CodigoHash { get; set; } = string.Empty;

    public DateTime ExpiraEm { get; set; }

    public DateTime? UsadoEm { get; set; }

    public int Tentativas { get; set; }

    public DateTime CriadoEm { get; set; }
}
