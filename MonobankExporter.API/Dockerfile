FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS publish

LABEL version="1.4"
LABEL maintainer=TheGarmr
LABEL url=https://github.com/TheGarmr/monobank-exporter

WORKDIR /etc/monobank-exporter

COPY ["MonobankExporter.Domain/MonobankExporter.Domain.csproj", "MonobankExporter.Domain/"]
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

RUN apk add tzdata && cp /usr/share/zoneinfo/Europe/Kiev /etc/localtime && apk upgrade musl

RUN adduser --disabled-password \
  --home /app \
  --gecos '' monobankuser && chown -R monobankuser /app

RUN touch /var/log/monobank-exporter.log && chown -R monobankuser:monobankuser /var/log/monobank-exporter.log

USER monobankuser

WORKDIR /etc/monobank-exporter
EXPOSE 80
COPY --from=publish /etc/monobank-exporter/publish .
ENTRYPOINT ["./MonobankExporter.API"]