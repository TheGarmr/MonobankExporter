# Monobank Exporter
### This is an application which exports _Prometheus_ metrics for [Monobank](https://www.monobank.ua).
### This app was build with the [Monobank API library](https://github.com/maisak/monobank-api).
### Full API documentation can be found here: [Monobank open API](https://api.monobank.ua/docs/)

### Functionality
* Publish currencies metrics
* Publish a client's balance and credit limit for each card as metrics
* You can use your own name different from that stored in the bank

### API limitations:
  * You can receive information about a client once per a minute
  * Information about currencies refreshes once per 5 minutes

### Quick start:
  * create a docker-compose file
  * fill in a config file
  * setup you Prometheus instance to scrape metrics
  * run `docker-compose up -d`

### monobank-exporter usage
  * minimal request time for client info is 2 minutes
  * minimal request time for currencies info is 10 minutes
  * use http or https for webhook only
  * `/webhook` ending is mandatory
  * a Redis instance is mandatory if you will use webhooks.

Currencies metrics will be provisioned in any case.<br>
The client's metrics will be provisioned only in the case of the existing token.<br>
Webhook will be set only in case of a valid URL (HTTP or HTTPS doesn't matter).<br>

## Docker-compose example
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
```