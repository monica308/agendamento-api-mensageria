FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Agendamento.Domain/*.csproj src/Agendamento.Domain/
COPY src/Agendamento.Application/*.csproj src/Agendamento.Application/
COPY src/Agendamento.Infrastructure/*.csproj src/Agendamento.Infrastructure/
COPY src/Agendamento.Worker/*.csproj src/Agendamento.Worker/
RUN dotnet restore src/Agendamento.Worker/Agendamento.Worker.csproj

COPY . .
RUN dotnet publish src/Agendamento.Worker/Agendamento.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Agendamento.Worker.dll"]
