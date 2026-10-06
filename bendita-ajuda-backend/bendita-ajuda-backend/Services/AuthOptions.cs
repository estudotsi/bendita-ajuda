namespace bendita_ajuda_backend.Services;

/// <summary>Configurações da seção "Auth".</summary>
public class AuthOptions
{
    public const string Secao = "Auth";

    /// <summary>Segredo do HMAC dos códigos de verificação (mínimo 32 caracteres).</summary>
    public string SegredoCodigo { get; set; } = string.Empty;
}
