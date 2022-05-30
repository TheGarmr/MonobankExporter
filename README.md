# Monobank Exporter [<img src="https://img.shields.io/badge/Docker%20Hub-images-blue.svg?logo=Docker">](https://hub.docker.com/r/thegarmr/monobank-exporter)
### This application exports _Prometheus_ metrics for [Monobank](https://www.monobank.ua).
### This app was built with the [Monobank API library](https://github.com/maisak/monobank-api).
### Full API documentation can be found here: [Monobank open API](https://api.monobank.ua/docs/)

### Functionality
  * Publish currencies metrics
  * Publish a client's balance and credit limit for each card as metrics
  * You can use your own name different from that stored in the bank
  * basic auth to the metrics endpoint

### API limitations:
  * You can receive information about a client once per a minute
  * Information about currencies refreshes once per 5 minutes

### Quickstart:
  * create a docker-compose file
  * fill in a config file
  * setup you Prometheus instance to scrape metrics
  * run `docker-compose up -d`

### monobank-exporter usage
  * minimal request time for client info is 2 minutes
  * minimal request time for currencies info is 10 minutes
  * use HTTP or HTTPS for webhook only
  * `/webhook` ending is mandatory
  * a Redis instance is mandatory if you will use webhooks.
  * basic auth is not required. it can be added from the config

Currencies metrics will be provisioned in any case.<br>
The client's metrics will be provisioned only in the case of the existing token.<br>
Webhook will be set only in case of a valid URL (HTTP or HTTPS doesn't matter).<br>

# Examples<br>

## Docker-compose with image from [Docker Hub](https://hub.docker.com/r/thegarmr/monobank-exporter)
```yaml
version: '3.1'

services:
  exporter:
    image: thegarmr/monobank-exporter:latest
    container_name: monobank-exporter
    restart: always
    volumes:
      - ./monobank-exporter.yml:/etc/monobank-exporter/monobank-exporter.yml
    environment:
      - TZ=Europe/Kiev
    depends_on:
      - monobank-exporter-redis
    networks:
      - exporter-network

  redis:
    image: redis:latest
    container_name: monobank-exporter-redis
    restart: always
    networks:
      - exporter-network

networks:
  exporter-network:
    external: true
```

## Docker-compose with image from [Docker Hub](https://hub.docker.com/r/thegarmr/monobank-exporter) with Grafana and Prometheus
You can find this example in the `Example` folder<br>
Clone repository to your local folder<br>
`git clone https://github.com/TheGarmr/monobank-exporter.git`<br>

Go to folder with sources<br>
`cd monobank-exporter/Example`<br>

Edit monobank-exporter.yml in the root folder (you can find an example below)<br>

Compose up!<br>
`docker-compose up -d`<br>

## Docker-compose with image from sources
Clone repository to your local folder<br>
`git clone https://github.com/TheGarmr/monobank-exporter.git`<br>

Go to folder with sources<br>
`cd monobank-exporter`<br>

Edit monobank-exporter.yml in the root folder (you can find an example below)<br>

Compose up!<br>
`docker-compose up -d`<br>

## Config file example
```yaml
monobank-exporter:
  clients:
    - name: "John"
      token: "yourToken"
    - name: "Briana"
      token: "yourToken"
  webhookUrl: "http://yourUrl/webhook"
  clientsRefreshTimeInMinutes: 60
  currenciesRefreshTimeInMinutes: 720
redis:
  host: "monobank-exporter-redis"
  port: "6379"
basic-auth:
  username: "admin"
  password: "admin"
```

# What's new
  * 1.1 - Added ability to set up basic auth for the `metrics` endpoint