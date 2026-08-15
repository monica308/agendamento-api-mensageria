FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Agendamento.Domain/*.csproj src/Agendamento.Domain/
COPY src/Agendamento.Application/*.csproj src/Agendamento.Application/
COPY src/Agendamento.Infrastructure/*.csproj src/Agendamento.Infrastructure/
COPY src/Agendamento.Worker/*.csproj src/Agendamento.Worker/
RUN dotnet restore src/Agendamento.Worker/Agendamento.Worker.csproj

COPY . .
RUN dotnet publish src/Agendamento.Worker/Agendamento.Worker.csproj -c Release -o /app/publish --no-restore

# Repare: aqui usamos a imagem "runtime" pura (não "aspnet"), porque o
# Worker NÃO é uma aplicação web - ele não precisa do Kestrel nem de
# nada relacionado a HTTP, só do runtime básico do .NET para executar.
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Agendamento.Worker.dll"]
