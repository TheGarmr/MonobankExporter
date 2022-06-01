FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS publish

WORKDIR /etc/monobank-exporter

COPY ["Monobank.Core/Monobank.Core.csproj", "Monobank.Core/"]
COPY ["Monobank.Tests/Monobank.Tests.csproj", "Monobank.Tests/"]
COPY ["MonobankExporter.BusinessLogic/MonobankExporter.BusinessLogic.csproj", "MonobankExporter.BusinessLogic/"]
COPY ["MonobankExporter.API/MonobankExporter.API.csproj", "MonobankExporter.API/"]

RUN dotnet restore "MonobankExporter.API/MonobankExporter.API.csproj" --runtime alpine-x64
COPY . .

RUN dotnet publish "MonobankExporter.API/MonobankExporter.API.csproj" \
    --configuration Release \
    --output /etc/monobank-exporter/publish \
    --no-restore \
    --nologo \
    --runtime alpine-x64 \
    --self-contained true \
    --verbosity quiet \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine AS final

RUN apk add tzdata && cp /usr/share/zoneinfo/Europe/Kiev /etc/localtime

WORKDIR /etc/monobank-exporter
EXPOSE 80
COPY --from=publish /etc/monobank-exporter/publish .
ENTRYPOINT ["./MonobankExporter.API"]