namespace Agendamento.Infrastructure.Messaging;

// Classe simples que recebe os valores da seção "RabbitMQ" do appsettings.json
// (ou das variáveis de ambiente, quando rodando via Docker). Isso evita
// "hardcodar" host/usuário/senha espalhados pelo código.
public class RabbitMqSettings
{
    public string Host { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
