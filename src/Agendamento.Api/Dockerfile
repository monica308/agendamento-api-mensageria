# ---------- ESTÁGIO 1: build ----------
# Usamos a imagem do SDK completo do .NET (pesada, ~800MB) só para COMPILAR
# o projeto. Ela tem o compilador, ferramentas de build, tudo que não
# precisamos para simplesmente RODAR a aplicação depois.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos primeiro só os arquivos .csproj de cada projeto, e restauramos
# os pacotes NuGet ANTES de copiar o resto do código-fonte. Isso é uma
# otimização de cache do Docker: se você só mudar um arquivo .cs depois,
# o Docker reaproveita a camada de "restore" (que é lenta) e refaz só a
# parte de compilação (rápida) - builds subsequentes ficam bem mais rápidos.
COPY src/Agendamento.Domain/*.csproj src/Agendamento.Domain/
COPY src/Agendamento.Application/*.csproj src/Agendamento.Application/
COPY src/Agendamento.Infrastructure/*.csproj src/Agendamento.Infrastructure/
COPY src/Agendamento.Api/*.csproj src/Agendamento.Api/
RUN dotnet restore src/Agendamento.Api/Agendamento.Api.csproj

# Agora sim copiamos todo o restante do código-fonte.
COPY . .
RUN dotnet publish src/Agendamento.Api/Agendamento.Api.csproj -c Release -o /app/publish --no-restore

# ---------- ESTÁGIO 2: runtime ----------
# Imagem final, bem mais leve (~200MB): só o necessário para EXECUTAR uma
# aplicação .NET já compilada, sem SDK, sem compilador. Isso reduz
# drasticamente o tamanho da imagem final que vai para produção/registry.
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Porta interna que o Kestrel (servidor HTTP do .NET) vai escutar dentro do container.
EXPOSE 8080
ENTRYPOINT ["dotnet", "Agendamento.Api.dll"]
