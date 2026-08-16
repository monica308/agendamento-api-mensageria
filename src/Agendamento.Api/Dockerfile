FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Agendamento.Domain/*.csproj src/Agendamento.Domain/
COPY src/Agendamento.Application/*.csproj src/Agendamento.Application/
COPY src/Agendamento.Infrastructure/*.csproj src/Agendamento.Infrastructure/
COPY src/Agendamento.Api/*.csproj src/Agendamento.Api/
RUN dotnet restore src/Agendamento.Api/Agendamento.Api.csproj

COPY . .
RUN dotnet publish src/Agendamento.Api/Agendamento.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Agendamento.Api.dll"]
