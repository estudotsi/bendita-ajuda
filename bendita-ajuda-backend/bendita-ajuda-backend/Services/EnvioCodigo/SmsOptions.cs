namespace bendita_ajuda_backend.Services.EnvioCodigo;

/// <summary>Configurações da seção "Sms" (TrackMax).</summary>
public class SmsOptions
{
    public const string Secao = "Sms";

    /// <summary>Token da API (gerado em "Meu Perfil" no portal TrackMax). Vazio = código só no console.</summary>
    public string Token { get; set; } = string.Empty;

    public string UrlBase { get; set; } = "https://sms.trackmax.com.br/";
}
