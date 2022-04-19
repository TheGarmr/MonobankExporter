FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /etc/monobank-exporter
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Monobank.Core/Monobank.Core.csproj", "Monobank.Core/"]
COPY ["Monobank.Tests/Monobank.Tests.csproj", "Monobank.Tests/"]
COPY ["MonobankExporter.API/MonobankExporter.API.csproj", "MonobankExporter.API/"]

RUN dotnet restore "MonobankExporter.API/MonobankExporter.API.csproj"
COPY . .
WORKDIR "/src/MonobankExporter.API"
RUN dotnet build "MonobankExporter.API.csproj" -c Release -o /etc/monobank-exporter/build

FROM build AS publish
RUN dotnet publish "MonobankExporter.API.csproj" -c Release -o /etc/monobank-exporter/publish

FROM base AS final
WORKDIR /etc/monobank-exporter
COPY --from=publish /etc/monobank-exporter/publish .
ENTRYPOINT ["dotnet", "MonobankExporter.API.dll"]