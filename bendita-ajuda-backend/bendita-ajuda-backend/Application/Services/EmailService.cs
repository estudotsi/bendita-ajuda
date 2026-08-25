using Resend;

namespace bendita_ajuda_backend.Application.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarEmailAsync(string para, string assunto, string html)
        {
            try
            {
                var apiKey = _configuration["Resend:ApiKey"];
                var from = _configuration["Resend:From"];

                IResend resend = ResendClient.Create(apiKey!);

                await resend.EmailSendAsync(new EmailMessage
                {
                    From = from!,
                    To = para,
                    Subject = assunto,
                    HtmlBody = html
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                throw;
            }
        }
    }
}
