# Monobank Exporter [<img src="https://img.shields.io/badge/Docker%20Hub-images-blue.svg?logo=Docker">](https://hub.docker.com/r/thegarmr/monobank-exporter)
### This application exports _Prometheus_ metrics for [Monobank](https://www.monobank.ua).
### Full API documentation can be found here: [Monobank open API](https://api.monobank.ua/docs/)

### Functionality
  * Publish currencies metrics
  * Publish a client's balance and credit limit for each card as metrics
  * You can use your name different from that stored in the bank
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
  * basic auth is not required. it can be added from the config

Currencies metrics will be provisioned in any case.<br/>
The client's metrics will be provisioned only in the case of the existing token.<br/>
Webhook will be set only in case of a valid URL (HTTP or HTTPS doesn't matter).<br/>

# Logs
  * logs are shown at the console and written to file `/var/log/monobank-exporter.log`
  * currently image is not able to create the log file by himself. you need to create a log file by yourself. if anyone knows how to fix this - I will be open to communication

# Examples<br/>

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
```

## Docker-compose with the image from [Docker Hub](https://hub.docker.com/r/thegarmr/monobank-exporter) with Grafana and Prometheus
You can find this example in the `Example` folder<br/>
Clone the repository to your local folder<br/>
`git clone https://github.com/TheGarmr/monobank-exporter.git`<br/>

Go to folder with sources<br/>
`cd monobank-exporter/Example`<br/>

Edit monobank-exporter.yml in the root folder (you can find an example below)<br/>

Compose up!<br/>
`docker-compose up -d`<br/>

## Docker-compose with images from sources
Clone the repository to your local folder<br/>
`git clone https://github.com/TheGarmr/monobank-exporter.git`<br/>

Go to folder with sources<br/>
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
           Simply add the `monobank-api` section with the `ApiBaseUrl`  property. Currently, it uses the `https://api.monobank.ua` url by default.
