namespace Agendamento.Infrastructure.Messaging;

public class RabbitMqSettings
{
    public string Host { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
