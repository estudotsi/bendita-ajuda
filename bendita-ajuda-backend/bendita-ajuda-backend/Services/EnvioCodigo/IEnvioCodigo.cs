namespace bendita_ajuda_backend.Services.EnvioCodigo;

/// <summary>Envia o código de verificação para o celular (SMS, console...).</summary>
public interface IEnvioCodigo
{
    /// <summary>Retorna false se o envio falhou (ex.: provedor fora do ar).</summary>
    Task<bool> EnviarAsync(string celular, string codigo, CancellationToken cancellationToken);
}
