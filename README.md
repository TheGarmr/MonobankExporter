# Monobank Exporter [<img src="https://img.shields.io/badge/Docker%20Hub-images-blue.svg?logo=Docker">](https://hub.docker.com/r/thegarmr/monobank-exporter)<br>
[<img src="https://img.shields.io/docker/pulls/thegarmr/monobank-exporter?label=Docker%20pulls&style=for-the-badge">](https://hub.docker.com/r/thegarmr/monobank-exporter)
[<img src="https://img.shields.io/docker/v/thegarmr/monobank-exporter?label=Latest%20Docker%20version&style=for-the-badge">](https://www.nuget.org/packages/MonobankClient/)<br>

### This application exports _Prometheus_ metrics for [Monobank](https://www.monobank.ua).
### Full API documentation can be found here: [Monobank open API](https://api.monobank.ua/docs/)

### Functionality
  * Publish currencies metrics
  * Publish a client's balance and credit limit for each card as metrics
  * You can use your name differently from that stored in the bank
  * basic auth to the metrics endpoint

### API limitations:
  * You can receive information about a client once per a minute
  * Information about currencies refreshes once per 5 minutes

### Quickstart:
  * Create a docker-compose file
  * fill in a config file
  * setup your Prometheus instance to scrape metrics
  * run `docker-compose up -d`

### Metrics
| Metric name               | Description                             |
| ------------------------- | --------------------------------------- |
| monobank_balance          | Show the current balance for each card      |
| monobank_jars             | Show current list of jars               |
| monobank_credit_limit     | Show the current credit limit for each card |
| monobank_currencies_buy   | Shows currencies rate for buy           |
| monobank_currencies_sell  | Shows currencies rate for sell          |
| monobank_currencies_cross | Shows currencies rate for cross         |

Metrics names can be overridden in the `metrics` config section. You can provide any name for these metrics.
Here is the example with names as default.
```yaml
metrics:
  balance: "monobank_balance"
  jars: "monobank_jars"
  creditLimit: "monobank_credit_limit"
  currenciesBuy: "monobank_currencies_buy"
  currenciesSell: "monobank_currencies_sell"
  currenciesCross: "monobank_currencies_cross"
```

### monobank-exporter usage
  * minimal request time for client info is 2 minutes
  * minimal request time for currencies info is 10 minutes
  * use HTTP or HTTPS for webhook only
  * `/webhook` ending is mandatory
  * basic auth is not required. it can be added from the config

Currency metrics will be provisioned in any case.<br/>
The client's metrics will be provisioned only in the case of the existing token.<br/>
Webhook will be set only in case of a valid URL (HTTP or HTTPS doesn't matter).<br/>

# Logs
  * logs are shown at the console and written to file `/etc/monobank-exporter/logs/monobank-exporter.log`
  * Serilog is used as a logger. Settings are defined in the `/etc/monobank-exporter/appsettings.json` file.
    You can override these settings.<br/>
	Documentation can be found here: [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration)

# Examples<br/>

## Docker-compose with the image from [Docker Hub](https://hub.docker.com/r/thegarmr/monobank-exporter)
```yaml
version: '3.1'

services:
  exporter:
    image: thegarmr/monobank-exporter:latest
    container_name: monobank-exporter
    restart: always
    volumes:
      - ./monobank-exporter.yml:/etc/monobank-exporter/monobank-exporter.yml
      - ./logs:/etc/monobank-exporter/logs
```

## Docker-compose with the image from [Docker Hub](https://hub.docker.com/r/thegarmr/monobank-exporter) with Grafana and Prometheus
You can find this example in the `Example` folder<br/>
Clone the repository to your local folder<br/>
`git clone https://github.com/TheGarmr/monobank-exporter.git`<br/>

Go to the folder with sources<br/>
`cd monobank-exporter/Example`<br/>

Edit monobank-exporter.yml in the root folder (you can find an example below)<br/>

Compose up!<br/>
`docker-compose up -d`<br/>

## Docker-compose with images from sources
Clone the repository to your local folder<br/>
`git clone https://github.com/TheGarmr/monobank-exporter.git`<br/>

Go to the folder with sources<br/>
`cd monobank-exporter`<br/>

Edit monobank-exporter.yml in the root folder (you can find an example below)<br/>

Compose up!<br/>
`docker-compose up -d`<br/>

## Config file example
```yaml
monobank-exporter:
  clients: #optional
    - name: "John"
      token: "yourToken"
    - name: "Briana"
      token: "yourToken"
  webhookUrl: "http://yourUrl/webhook" #optional
  clientsRefreshTimeInMinutes: 60 #optional
  currenciesRefreshTimeInMinutes: 720 #optional
basic-auth: #optional
  username: "admin"
  password: "admin"
monobank-api: #optional
  apiBaseUrl: "https://api.monobank.ua"
```
# What's new
  * v1.1 - Added ability to set up basic auth for the `metrics` endpoint.
  * v1.2 - Added Serilog as a logger and cleaned up a lot of useless commands.
  * v1.3 - Changed timezone to Europe/Kiev at the alpine image. Changed logs file naming.
  * v1.4 - Fix wrong behavior for webhooks setting and redundant logs.<br/>
           Switched from root user to non-root at the Dockerfile.<br/>
           Added GET endpoint for the webhook controller to avoid any possible problems with webhooks setting. According to the documentation, the provided URL should respond with 200 status.<br/>
           Added upgrading of musl at the image to avoid all vulnerabilities.<br/>
           Added some labels to the Dockerfile.<br/>
  * v1.5 - Removed Redis dependency with IMemoryCache. Small refactoring of library for monobank client
  * v1.5.1 - Hotfix of webhook publishing
  * v1.6 - Refactored project. Added small features. Added newly created package for HTTP client.<br/>
           You can add an API url settings section if it will change.<br>
           Simply add the `monobank-api` section with the `ApiBaseUrl`  property. Currently, it uses the `https://api.monobank.ua` url by default.<br/>
           Deleted the `credit_limit` field from the `monobank_balance`.<br/>
           Instead of this `credit_limit` will be exposed as a separate metric.
  * v1.6.1 - Hotfix. Changed package version for client's NuGet package with webhook not setting.
  * v1.6.2 - Changed log level for the Microsoft's http client to `Warning` for better logs readability
  * v1.7 - Migrated the app to .Net 6. Refactored a lot and small redisign on balance exporting.<br/>
           Added ability to override metrics names.<br/>
           Added ability to override Serilog's settings.
  * v1.8 - Resolved issue with logs.<br/>
           Renamed project 'API' to 'Service'.<br/>
           Added .gitlab-ci.yml
  * v1.9 - Disabled non-root user at container to fix issue with logs on Unix systems
  * v1.10 - Added export of actual jars
  * v1.11 - Upgraded .Net version to 7.0. Added minimal API.
  * v1.11.1 - Hotfix of missed adding configuration
