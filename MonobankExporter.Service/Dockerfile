FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS publish

ARG VERSION
ARG MAINTAINER
ARG URL

LABEL version=$VERSION
LABEL maintainer=$MAINTAINER
LABEL url=$URL

WORKDIR /etc/monobank-exporter

COPY ["MonobankExporter.Application/MonobankExporter.Application.csproj", "MonobankExporter.Application/"]
COPY ["MonobankExporter.Service/MonobankExporter.Service.csproj", "MonobankExporter.Service/"]

RUN dotnet restore "MonobankExporter.Service/MonobankExporter.Service.csproj" --runtime alpine-x64
COPY . .

RUN dotnet publish "MonobankExporter.Service/MonobankExporter.Service.csproj" \
    --configuration Release \
    --output /etc/monobank-exporter/publish \
    --no-restore \
    --nologo \
    --runtime alpine-x64 \
    --self-contained true \
    --verbosity quiet \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine AS final

RUN apk add tzdata && cp /usr/share/zoneinfo/Europe/Kiev /etc/localtime && apk upgrade musl

#RUN adduser --disabled-password \
  #--home /etc/monobank-exporter \
  #--gecos '' monobankuser && chown -R monobankuser /etc/monobank-exporter
#
#USER monobankuser

WORKDIR /etc/monobank-exporter
EXPOSE 80
COPY --from=publish /etc/monobank-exporter/publish .
ENTRYPOINT ["./MonobankExporter.Service"]