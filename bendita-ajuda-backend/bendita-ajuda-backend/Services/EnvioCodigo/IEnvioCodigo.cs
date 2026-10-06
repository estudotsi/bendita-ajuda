namespace bendita_ajuda_backend.Services.EnvioCodigo;

/// <summary>Envia o código de verificação para o celular (WhatsApp, SMS, console...).</summary>
public interface IEnvioCodigo
{
    Task EnviarAsync(string celular, string codigo, CancellationToken cancellationToken);
}
