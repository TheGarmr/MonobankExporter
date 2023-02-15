using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Monobank.Client;
using Monobank.Client.Enums;
using Monobank.Client.Models;
using MonobankExporter.Application.Enums;
using MonobankExporter.Application.Interfaces;
using MonobankExporter.Application.Models;
using MonobankExporter.Application.Options;
using MonobankExporter.Application.Services;
using Moq;
using Xunit;
using MemoryCacheEntryOptions = Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions;

namespace MonobankExporter.UnitTests.Services;

public class MonobankServiceTests
{
    private const string ValidWebHookUrl = "https://example.com/webhook";
    private readonly Mock<IMetricsExporterService> _metricsExporterMock;
    private readonly Mock<ILookupsMemoryCacheService> _cacheServiceMock;
    private readonly Mock<ILogger<MonobankService>> _loggerMock;
    private readonly Mock<IMonobankClient> _monobankClientMock;

    private AccountInfo _tryGetResult;

    public MonobankServiceTests()
    {
        _metricsExporterMock = new Mock<IMetricsExporterService>();
        _cacheServiceMock = new Mock<ILookupsMemoryCacheService>();
        _loggerMock = new Mock<ILogger<MonobankService>>();
        _monobankClientMock = new Mock<IMonobankClient>();
    }

    #region WebHookUrlIsValid

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("some-invalid-url")]
    [InlineData("http://asd/webhook")]
    [InlineData("https://asd/webhook")]
    [InlineData("ftp://asd/webhook")]
    [InlineData("sftp://asd/webhook")]
    [InlineData("http://example.com/webhooks")]
    [InlineData("https://example.com/webhooks")]
    public void WebHookUrlIsValidShouldReturnFalseIfUrlIsNotValid(string webHookUrl)
    {
        // Arrange
        var service = GetService();

        // Act
        var result = service.WebHookUrlIsValid(webHookUrl);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("http://example.com/webhook")]
    [InlineData("https://example.com/webhook")]
    [InlineData("https://subdomain.example.com/webhook")]
    [InlineData("https://subdomain.subdomain.example.com/webhook")]
    public void WebHookUrlIsValidShouldReturnTrueIfUrlIsValid(string webHookUrl)
    {
        // Arrange
        var service = GetService();

        // Act
        var result = service.WebHookUrlIsValid(webHookUrl);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region ExportMetricsOnWebHook

    [Fact]
    public void ExportMetricsOnWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfModelIsNull()
    {
        // Arrange
        var service = GetService();

        // Act
        service.ExportMetricsOnWebHook(null, CancellationToken.None);

        // Assert
        _metricsExporterMock.Verify(x => x.ObserveAccountBalance(It.IsAny<AccountInfo>(), It.IsAny<double>()), Times.Never);
        _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<object>(), out _tryGetResult), Times.Never);
    }

    [Fact]
    public void ExportMetricsOnWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfDataIsNull()
    {
        // Arrange
        var webhook = new WebHook
        {
            Data = null
        };
        var service = GetService();

        // Act
        service.ExportMetricsOnWebHook(webhook, CancellationToken.None);

        // Assert
        _metricsExporterMock.Verify(x => x.ObserveAccountBalance(It.IsAny<AccountInfo>(), It.IsAny<double>()), Times.Never);
        _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<object>(), out _tryGetResult), Times.Never);
    }

    [Fact]
    public void ExportMetricsOnWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfAccountIsNull()
    {
        // Arrange
        var webhook = new WebHook
        {
            Data = new WebHookData { Account = null }
        };
        var service = GetService();

        // Act
        service.ExportMetricsOnWebHook(webhook, CancellationToken.None);

        // Assert
        _metricsExporterMock.Verify(x => x.ObserveAccountBalance(It.IsAny<AccountInfo>(), It.IsAny<double>()), Times.Never);
        _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<object>(), out _tryGetResult), Times.Never);
    }

    [Fact]
    public void ExportMetricsOnWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfStatementItemIsNull()
    {
        // Arrange
        var webhook = new WebHook
        {
            Data = new WebHookData { Account = "SomeAccount", StatementItem = null }
        };
        var service = GetService();

        // Act
        service.ExportMetricsOnWebHook(webhook, CancellationToken.None);

        // Assert
        _metricsExporterMock.Verify(x => x.ObserveAccountBalance(It.IsAny<AccountInfo>(), It.IsAny<double>()), Times.Never);
        AccountInfo result;
        _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<object>(), out result), Times.Never);
    }

    [Fact]
    public void ExportMetricsOnWebHookShouldRetrieveRecordFromCacheAndDontCallExporterServiceIfRecordInCacheNotValid()
    {
        // Arrange
        var account = Guid.NewGuid().ToString();
        var webhook = new WebHook
        {
            Data = new WebHookData { Account = account, StatementItem = new Statement() }
        };
        AccountInfo accountInfo = null;

        _cacheServiceMock
            .Setup(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<string>(), out accountInfo))
            .Returns(false);
        var service = GetService();

        // Act
        service.ExportMetricsOnWebHook(webhook, CancellationToken.None);

        // Assert
        _metricsExporterMock.Verify(x => x.ObserveAccountBalance(It.IsAny<AccountInfo>(), It.IsAny<double>()), Times.Never);
        _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, account, out _tryGetResult), Times.Once);
    }

    [Fact]
    public void ExportMetricsOnWebHookShouldCallExportService()
    {
        // Arrange
        var expectedBalance = 12345678.00;
        var accountInfo = new AccountInfo();
        var webhook = new WebHook
        {
            Data = new WebHookData
            {
                Account = "SomeAccountId",
                StatementItem = new Statement { Balance = 1234567800 }
            }
        };

        _cacheServiceMock
            .Setup(x => x.TryGetValue(CacheType.AccountInfo, webhook.Data.Account, out accountInfo))
            .Returns(true);
        var service = GetService();

        // Act
        service.ExportMetricsOnWebHook(webhook, CancellationToken.None);

        // Assert
        _metricsExporterMock.Verify(x => x.ObserveAccountBalance(It.IsAny<AccountInfo>(), expectedBalance), Times.Once);
    }

    #endregion

    #region SetupWebHookAndExportMetricsForUsersAsync

    [Fact]
    public async Task SetupWebHookAndExportMetricsForUsersAsyncShouldNotCallClientIfListOfAccountsIsNull()
    {
        // Arrange
        var service = GetService();

        // Act
        await service.SetupWebHookAndExportMetricsForUsersAsync(ValidWebHookUrl, null, CancellationToken.None);

        // Assert
        _monobankClientMock.Verify(x => x.SetWebhookAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetupWebHookAndExportMetricsForUsersAsyncShouldNotCallClientIfListOfAccountsIsEmpty()
    {
        // Arrange
        var webhookUrl = ValidWebHookUrl;
        var clients = new List<ClientInfoOptions>();
        var service = GetService();

        // Act
        await service.SetupWebHookAndExportMetricsForUsersAsync(webhookUrl, clients, CancellationToken.None);

        // Assert
        _monobankClientMock.Verify(x => x.SetWebhookAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetupWebHookAndExportMetricsForUsersAsyncShouldCallWebhookSetupIfClientsListIsValid()
    {
        // Arrange
        var webhookUrl = ValidWebHookUrl;
        var clients = new List<ClientInfoOptions>
        {
            new () { Token = Guid.NewGuid().ToString() },
            new () { Token = Guid.NewGuid().ToString() },
            new () { Token = Guid.NewGuid().ToString() }
        };
        _monobankClientMock
            .Setup(x => x.GetClientInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new UserInfo());

        var service = GetService();

        // Act
        await service.SetupWebHookAndExportMetricsForUsersAsync(webhookUrl, clients, CancellationToken.None);

        // Assert
        clients.ForEach(client =>
        {
            _monobankClientMock
                .Verify(x => x.SetWebhookAsync(webhookUrl, client.Token, It.IsAny<CancellationToken>()),
                    Times.Once);
        });
    }

    [Fact]
    public async Task SetupWebHookAndExportMetricsForUsersAsyncShouldUseNameFromClientsListIfProvidedForObserving()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var accountId = Guid.NewGuid().ToString();
        var nameFromConfig = "John Wick";
        var clientFromApi = GetValidClient(accountId, name: "John Doe");
        var clients = new List<ClientInfoOptions> { new() { Token = token, Name = nameFromConfig } };
        _monobankClientMock.Setup(x => x.GetClientInfoAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientFromApi);
        AccountInfo expectedAccount = null;
        _metricsExporterMock.Setup(x => x.ObserveAccountBalance(It.IsAny<AccountInfo>(), It.IsAny<double>()))
            .Callback((AccountInfo account, double balance) => expectedAccount = account);
        var service = GetService();

        // Act
        await service.SetupWebHookAndExportMetricsForUsersAsync(ValidWebHookUrl, clients, CancellationToken.None);

        // Assert
        Assert.NotNull(expectedAccount);
        Assert.Equal(expectedAccount.HolderName, clientFromApi.Name);
        Assert.Equal(expectedAccount.HolderName, nameFromConfig);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task SetupWebHookAndExportMetricsForUsersAsyncShouldShouldNotExportMetricsForUserIfUserTokenIsNotValid(string token)
    {
        // Arrange
        var clients = new List<ClientInfoOptions> { new() { Token = token } };
        var service = GetService();

        // Act
        await service.SetupWebHookAndExportMetricsForUsersAsync(ValidWebHookUrl, clients, CancellationToken.None);

        // Assert
        _monobankClientMock.Verify(x => x.GetClientInfoAsync(It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _metricsExporterMock.Verify(x => x.ObserveAccountBalance(It.IsAny<AccountInfo>(),
            It.IsAny<double>()), Times.Never);
        _cacheServiceMock.Verify(x => x.Set(It.IsAny<CacheType>(),
            It.IsAny<string>(), It.IsAny<AccountInfo>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Never);
    }

    [Fact]
    public async Task SetupWebHookAndExportMetricsForUsersAsyncShouldSetWebHookIfClientOptionHasItsOwnValue()
    {
        // Arrange
        var mainWebHookUrl = ValidWebHookUrl;
        var userWebhookUrl = "https://another-valid-url.com/webhook";
        var token = Guid.NewGuid().ToString();
        var clients = new List<ClientInfoOptions> { new() { Token = token, WebHookUrl = userWebhookUrl } };
        var userFromApi = GetValidClient();
        userFromApi.WebHookUrl = userWebhookUrl;
        _monobankClientMock
            .Setup(x => x.GetClientInfoAsync(token, CancellationToken.None))
            .ReturnsAsync(() => userFromApi);
        _monobankClientMock
            .Setup(x => x.SetWebhookAsync(userWebhookUrl, token, CancellationToken.None))
            .ReturnsAsync(true);

        var service = GetService();

        // Act
        await service.SetupWebHookAndExportMetricsForUsersAsync(mainWebHookUrl, clients, CancellationToken.None);

        // Assert
        _monobankClientMock.Verify(x => x.SetWebhookAsync(userWebhookUrl, token, CancellationToken.None), Times.Once);
        _monobankClientMock.Verify(x => x.SetWebhookAsync(mainWebHookUrl, token, CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task SetupWebHookAndExportMetricsForUsersAsyncShouldReturnListOfValidClients()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var validClient = new ClientInfoOptions { Token = token };
        var invalidClient = new ClientInfoOptions { Token = null };

        var clients = new List<ClientInfoOptions> { validClient, invalidClient };
        var userFromApi = GetValidClient();
        _monobankClientMock
            .Setup(x => x.GetClientInfoAsync(token, CancellationToken.None))
            .ReturnsAsync(() => userFromApi);
        _monobankClientMock
            .Setup(x => x.SetWebhookAsync(It.IsAny<string>(), token, CancellationToken.None))
            .ReturnsAsync(true);

        var service = GetService();

        // Act
        var result = await service.SetupWebHookAndExportMetricsForUsersAsync(ValidWebHookUrl, clients, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Single(result);
    }

    #endregion

    #region ExportMetricsForUsersAsync

    [Fact]
    public async Task ExportBalanceMetricsForUsersAsyncShouldNotCallClientIfListOfAccountsIsNull()
    {
        // Arrange
        var service = GetService();

        // Act
        await service.ExportBalanceMetricsForUsersAsync(null, CancellationToken.None);

        // Assert
        _monobankClientMock.Verify(x => x.GetClientInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExportBalanceMetricsForUsersAsyncShouldNotCallClientIfListOfAccountsIsEmpty()
    {
        // Arrange
        var clients = new List<ClientInfoOptions>();
        var service = GetService();

        // Act
        await service.ExportBalanceMetricsForUsersAsync(clients, CancellationToken.None);

        // Assert
        _monobankClientMock.Verify(x => x.GetClientInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ExportMetricsForUsersAsyncExportMetricsForUsersAsyncShouldNotExportMetricsForUserIfUserTokenIsNotValid(string token)
    {
        // Arrange
        var clients = new List<ClientInfoOptions> { new() { Token = token } };
        var service = GetService();

        // Act
        await service.ExportBalanceMetricsForUsersAsync(clients, CancellationToken.None);

        // Assert
        _monobankClientMock.Verify(x => x.GetClientInfoAsync(It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _metricsExporterMock.Verify(x => x.ObserveAccountBalance(It.IsAny<AccountInfo>(),
            It.IsAny<double>()), Times.Never);
        _cacheServiceMock.Verify(x => x.Set(It.IsAny<CacheType>(),
            It.IsAny<string>(), It.IsAny<AccountInfo>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Never);
    }

    [Fact]
    public async Task ExportBalanceMetricsForUsersAsyncShouldExportMetricsForUser()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var accountId = Guid.NewGuid().ToString();
        var client = GetValidClient(accountId);
        var clients = new List<ClientInfoOptions> { new() { Token = token } };
        _monobankClientMock
            .Setup(x => x.GetClientInfoAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var service = GetService();

        // Act
        await service.ExportBalanceMetricsForUsersAsync(clients, CancellationToken.None);

        // Assert
        _monobankClientMock.Verify(x => x.GetClientInfoAsync(token,
            It.IsAny<CancellationToken>()), Times.Once);
        foreach (var account in client.Accounts)
        {
            _metricsExporterMock.Verify(x => x.ObserveAccountBalance(It.IsAny<AccountInfo>(),
                account.BalanceWithoutCreditLimit), Times.Once);
        }
    }

    [Fact]
    public async Task ExportBalanceMetricsForUsersAsyncShouldStoreDataToCacheIfWebHookIsSet()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var accountId = Guid.NewGuid().ToString();
        var clientFromApi = GetValidClient(accountId);
        clientFromApi.WebHookUrl = "some-webhook-url";
        var clients = new List<ClientInfoOptions> { new() { Token = token } };
        _monobankClientMock.Setup(x => x.GetClientInfoAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientFromApi);
        _monobankClientMock.Setup(x => x.SetWebhookAsync(It.IsAny<string>(), token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = GetService();

        // Act
        await service.ExportBalanceMetricsForUsersAsync(clients, CancellationToken.None);

        // Assert
        foreach (var account in clientFromApi.Accounts)
        {
            _cacheServiceMock.Verify(x => x.Set(CacheType.AccountInfo,
                account.Id, It.IsAny<AccountInfo>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        }
    }

    [Fact]
    public async Task ExportBalanceMetricsForUsersAsyncShouldStoreDataToCacheIfWebHookIsNotSet()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var accountId = Guid.NewGuid().ToString();
        var clientFromApi = GetValidClient(accountId);
        clientFromApi.WebHookUrl = null;
        var clients = new List<ClientInfoOptions> { new() { Token = token } };
        _monobankClientMock.Setup(x => x.GetClientInfoAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientFromApi);
        _monobankClientMock.Setup(x => x.SetWebhookAsync(It.IsAny<string>(), token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = GetService();

        // Act
        await service.ExportBalanceMetricsForUsersAsync(clients, CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(x => x.Set(CacheType.AccountInfo,
            accountId, It.IsAny<AccountInfo>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Never);
    }

    [Fact]
    public async Task ExportBalanceMetricsForUsersAsyncShouldUseNameFromClientsListIfProvidedForObserving()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var accountId = Guid.NewGuid().ToString();
        var nameFromConfig = "John Wick";
        var clientFromApi = GetValidClient(accountId, name: "John Doe");
        var clients = new List<ClientInfoOptions> { new() { Token = token, Name = nameFromConfig } };
        _monobankClientMock.Setup(x => x.GetClientInfoAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientFromApi);
        AccountInfo expectedAccount = null;
        _metricsExporterMock.Setup(x => x.ObserveAccountBalance(It.IsAny<AccountInfo>(), It.IsAny<double>()))
            .Callback((AccountInfo account, double balance) => expectedAccount = account);
        var service = GetService();

        // Act
        await service.ExportBalanceMetricsForUsersAsync(clients, CancellationToken.None);

        // Assert
        Assert.NotNull(expectedAccount);
        Assert.Equal(expectedAccount.HolderName, clientFromApi.Name);
        Assert.Equal(expectedAccount.HolderName, nameFromConfig);
    }

    #endregion

    #region ExportMetricsForCurrenciesAsync

    [Fact]
    public async Task ExportCurrenciesMetricsAsyncShouldNotCallExporterServiceIfMetricsCollectionIsNull()
    {
        // Arrange
        _monobankClientMock.Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);
        var service = GetService();

        // Act
        await service.ExportMetricsForCurrenciesAsync(CancellationToken.None);

        // Assert
        _metricsExporterMock.Verify(x => x.ObserveCurrency(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CurrencyObserveType>(), It.IsAny<float>()), Times.Never());
    }

    [Fact]
    public async Task ExportCurrenciesMetricsAsyncShouldNotCallExporterServiceIfMetricsCollectionIsEmpty()
    {
        // Arrange
        var currencies = new List<CurrencyInfo>();
        _monobankClientMock.Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currencies);
        var service = GetService();

        // Act
        await service.ExportMetricsForCurrenciesAsync(CancellationToken.None);

        // Assert
        _metricsExporterMock.Verify(x => x.ObserveCurrency(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CurrencyObserveType>(), It.IsAny<float>()), Times.Never());
    }

    [Fact]
    public async Task ExportCurrenciesMetricsAsyncShouldCallExporterServiceForCurrency()
    {
        // Arrange
        var currencies = new List<CurrencyInfo>
        {
            new ()
            {
                CurrencyCodeA = 980,
                CurrencyCodeB = 840,
                Date = DateTime.UtcNow.Ticks,
                RateBuy = 100.12f,
                RateCross = 100.12f,
                RateSell = 100.12f
            }
        };
        _monobankClientMock.Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currencies);
        var service = GetService();

        // Act
        await service.ExportMetricsForCurrenciesAsync(CancellationToken.None);

        // Assert
        currencies.ForEach(currency =>
        {
            _metricsExporterMock.Verify(x => x.ObserveCurrency(currency.CurrencyNameA, currency.CurrencyNameB,
                CurrencyObserveType.Buy, currency.RateBuy), Times.Once);
            _metricsExporterMock.Verify(x => x.ObserveCurrency(currency.CurrencyNameA, currency.CurrencyNameB,
                CurrencyObserveType.Sell, currency.RateSell), Times.Once);
            _metricsExporterMock.Verify(x => x.ObserveCurrency(currency.CurrencyNameA, currency.CurrencyNameB,
                CurrencyObserveType.Cross, currency.RateCross), Times.Once);
        });
    }

    [Fact]
    public async Task ExportCurrenciesMetricsAsyncShouldNotCallExporterServiceForCurrencyIfRateValueIsZero()
    {
        // Arrange
        var currencies = new List<CurrencyInfo>
        {
            new ()
            {
                CurrencyCodeA = 980,
                CurrencyCodeB = 840,
                Date = DateTime.UtcNow.Ticks,
                RateBuy = 0,
                RateCross = 0,
                RateSell = 0
            }
        };
        _monobankClientMock.Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currencies);
        var service = GetService();

        // Act
        await service.ExportMetricsForCurrenciesAsync(CancellationToken.None);

        // Assert
        currencies.ForEach(currency =>
        {
            _metricsExporterMock.Verify(x => x.ObserveCurrency(currency.CurrencyNameA, currency.CurrencyNameB,
                CurrencyObserveType.Buy, It.IsAny<float>()), Times.Never);
            _metricsExporterMock.Verify(x => x.ObserveCurrency(currency.CurrencyNameA, currency.CurrencyNameB,
                CurrencyObserveType.Sell, It.IsAny<float>()), Times.Never);
            _metricsExporterMock.Verify(x => x.ObserveCurrency(currency.CurrencyNameA, currency.CurrencyNameB,
                CurrencyObserveType.Cross, It.IsAny<float>()), Times.Never);
        });
    }

    #endregion

    #region Private methods

    private MonobankService GetService(MonobankExporterOptions options = null)
    {
        options ??= new MonobankExporterOptions();
        return new MonobankService(options, _monobankClientMock.Object, _metricsExporterMock.Object, _cacheServiceMock.Object, _loggerMock.Object);
    }

    private static UserInfo GetValidClient(string accountId = null, string name = "John Doe", short countOfAccounts = 2)
    {
        var client = new UserInfo
        {
            Id = accountId ?? Guid.NewGuid().ToString(),
            Name = name,
            Accounts = new List<Account>()
        };

        for (var i = 0; i < countOfAccounts; i++)
        {
            client.Accounts.Add(GetValidAccount(Guid.NewGuid().ToString()));
        }

        return client;
    }

    private static Account GetValidAccount(string accountId, int currencyCode = 980, AccountTypes accountType = AccountTypes.Black, long creditLimit = 0, long? balance = null)
    {
        var random = new Random();
        return new Account
        {
            Id = accountId,
            CurrencyCode = currencyCode,
            Type = accountType,
            CreditLimit = creditLimit,
            Balance = balance ?? random.Next(0, 1000000)
        };
    }

    #endregion
}