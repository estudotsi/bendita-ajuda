namespace bendita_ajuda_backend.Services.EnvioCodigo;

/// <summary>
/// Apenas para desenvolvimento: escreve o código no log do console
/// em vez de enviar de verdade.
/// </summary>
public class EnvioCodigoConsole(ILogger<EnvioCodigoConsole> logger) : IEnvioCodigo
{
    public Task EnviarAsync(string celular, string codigo, CancellationToken cancellationToken)
    {
        logger.LogWarning("[DEV] Código de verificação para {Celular}: {Codigo}", celular, codigo);
        return Task.CompletedTask;
    }
}
