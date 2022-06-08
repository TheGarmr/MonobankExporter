using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MonobankExporter.BusinessLogic.Interfaces;
using MonobankExporter.BusinessLogic.Models;
using MonobankExporter.BusinessLogic.Services;
using MonobankExporter.Client;
using MonobankExporter.Client.Models;
using MonobankExporter.Client.Models.Consts;
using MonobankExporter.Client.Services;
using Moq;
using Xunit;
using MemoryCacheEntryOptions = Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions;

namespace MonobankExporter.UnitTests.Services
{
    public class MonobankServiceTests
    {
        private readonly Mock<IMonoClient> _monoClientMock;
        private readonly Mock<IMetricsExporterService> _metricsExporterMock;
        private readonly Mock<ILookupsMemoryCache> _cacheServiceMock;
        private readonly Mock<ILogger<MonobankService>> _loggerMock;
        private readonly Mock<IMonobankCurrencyClient> _currencyClientMock;
        private readonly Mock<IMonobankServiceClient> _monobankServiceClientMock;

        private AccountInfoModel _tryGetResult;

        public MonobankServiceTests()
        {
            _monoClientMock = new Mock<IMonoClient>();
            _metricsExporterMock = new Mock<IMetricsExporterService>();
            _cacheServiceMock = new Mock<ILookupsMemoryCache>();
            _loggerMock = new Mock<ILogger<MonobankService>>();
            _currencyClientMock = new Mock<IMonobankCurrencyClient>();
            _monobankServiceClientMock = new Mock<IMonobankServiceClient>();
            _monoClientMock.SetupGet(x => x.Currency).Returns(_currencyClientMock.Object);
            _monoClientMock.SetupGet(x => x.Client).Returns(_monobankServiceClientMock.Object);
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
            _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<object>(), out _tryGetResult), Times.Never);
        }

        [Fact]
        public void ExportMetricsOnWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfDataIsNull()
        {
            // Arrange
            var webhook = new WebHookModel
            {
                Data = null
            };
            var service = GetService();

            // Act
            service.ExportMetricsOnWebHook(webhook, CancellationToken.None);

            // Assert
            _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<object>(), out _tryGetResult), Times.Never);
        }

        [Fact]
        public void ExportMetricsOnWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfAccountIsNull()
        {
            // Arrange
            var webhook = new WebHookModel
            {
                Data = new WebHookData { Account = null }
            };
            var service = GetService();

            // Act
            service.ExportMetricsOnWebHook(webhook, CancellationToken.None);

            // Assert
            _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<object>(), out _tryGetResult), Times.Never);
        }

        [Fact]
        public void ExportMetricsOnWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfStatementItemIsNull()
        {
            // Arrange
            var webhook = new WebHookModel
            {
                Data = new WebHookData { Account = "SomeAccount", StatementItem = null }
            };
            var service = GetService();

            // Act
            service.ExportMetricsOnWebHook(webhook, CancellationToken.None);

            // Assert
            _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            AccountInfoModel result;
            _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<object>(), out result), Times.Never);
        }

        [Fact]
        public void ExportMetricsOnWebHookShouldRetrieveRecordFromCacheAndDontCallExporterServiceIfRecordInCacheNotValid()
        {
            // Arrange
            var account = Guid.NewGuid().ToString();
            var webhook = new WebHookModel
            {
                Data = new WebHookData { Account = account, StatementItem = new Statement() }
            };
            AccountInfoModel accountInfo = null;

            _cacheServiceMock
                .Setup(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<string>(), out accountInfo))
                .Returns(false);
            var service = GetService();

            // Act
            service.ExportMetricsOnWebHook(webhook, CancellationToken.None);

            // Assert
            _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, account, out _tryGetResult), Times.Once);
        }

        [Fact]
        public void ExportMetricsOnWebHookShouldCallExportService()
        {
            // Arrange
            var expectedBalance = 12345678.00;
            var accountInfo = new AccountInfoModel();
            var webhook = new WebHookModel
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
            _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), expectedBalance), Times.Once);
        }

        #endregion

        #region SetupWebHookForUsersAsync

        [Fact]
        public async Task SetupWebHookForUsersAsyncShouldNotCallClientIfListOfAccountsIsNull()
        {
            // Arrange
            var service = GetService();

            // Act
            await service.SetupWebHookForUsersAsync("https://example.com", null, CancellationToken.None);

            // Assert
            _monoClientMock.VerifyGet(x => x.Client, Times.Never);
        }

        [Fact]
        public async Task SetupWebHookForUsersAsyncShouldNotCallClientIfListOfAccountsIsEmpty()
        {
            // Arrange
            var webhookUrl = "https://example.com";
            var clients = new List<ClientInfoOptions>();
            var service = GetService();

            // Act
            await service.SetupWebHookForUsersAsync(webhookUrl, clients, CancellationToken.None);

            // Assert
            _monoClientMock.VerifyGet(x => x.Client, Times.Never);
        }

        [Fact]
        public async Task SetupWebHookForUsersAsyncShouldCallWebhookSetupIfClientsListIsValid()
        {
            // Arrange
            var webhookUrl = "https://example.com";
            var clients = new List<ClientInfoOptions>
            {
                new () { Token = Guid.NewGuid().ToString() },
                new () { Token = Guid.NewGuid().ToString() },
                new () { Token = Guid.NewGuid().ToString() }
            };
            var service = GetService();

            // Act
            await service.SetupWebHookForUsersAsync(webhookUrl, clients, CancellationToken.None);

            // Assert
            clients.ForEach(client =>
            {
                _monobankServiceClientMock
                    .Verify(x => x.SetWebhookAsync(webhookUrl, client.Token, It.IsAny<CancellationToken>()),
                        Times.Once);
            });
        }

        #endregion

        #region ExportMetricsForUsersAsync

        [Fact]
        public async Task ExportMetricsForUsersAsyncShouldNotCallClientIfListOfAccountsIsNull()
        {
            // Arrange
            var service = GetService();

            // Act
            await service.ExportMetricsForUsersAsync(true, null, CancellationToken.None);

            // Assert
            _monoClientMock.VerifyGet(x => x.Client, Times.Never);
        }

        [Fact]
        public async Task ExportMetricsForUsersAsyncShouldNotCallClientIfListOfAccountsIsEmpty()
        {
            // Arrange
            var clients = new List<ClientInfoOptions>();
            var service = GetService();

            // Act
            await service.ExportMetricsForUsersAsync(true, clients, CancellationToken.None);

            // Assert
            _monoClientMock.VerifyGet(x => x.Client, Times.Never);
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
            await service.ExportMetricsForUsersAsync(true, clients, CancellationToken.None);

            // Assert
            _monobankServiceClientMock.Verify(x => x.GetClientInfoAsync(It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
            _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(),
                It.IsAny<double>()), Times.Never);
            _cacheServiceMock.Verify(x => x.Set(It.IsAny<CacheType>(),
                It.IsAny<string>(), It.IsAny<AccountInfoModel>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Never);
        }

        [Fact]
        public async Task ExportMetricsForUsersAsyncShouldExportMetricsForUser()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();
            var accountId = Guid.NewGuid().ToString();
            var client = GetValidClient(accountId);
            var clients = new List<ClientInfoOptions> { new () { Token = token } };
            _monobankServiceClientMock
                .Setup(x => x.GetClientInfoAsync(token, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            var service = GetService();

            // Act
            await service.ExportMetricsForUsersAsync(true, clients, CancellationToken.None);

            // Assert
            _monobankServiceClientMock.Verify(x => x.GetClientInfoAsync(token,
                It.IsAny<CancellationToken>()), Times.Once);
            foreach (var account in client.Accounts)
            {
                _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(),
                    account.BalanceWithoutCreditLimit), Times.Once);
            }
        }

        [Fact]
        public async Task ExportMetricsForUsersAsyncShouldStoreDataToCacheIfParamIsSet()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();
            var accountId = Guid.NewGuid().ToString();
            var client = GetValidClient(accountId);
            var clients = new List<ClientInfoOptions> { new() { Token = token } };
            _monobankServiceClientMock.Setup(x => x.GetClientInfoAsync(token, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            var service = GetService();

            // Act
            await service.ExportMetricsForUsersAsync(true, clients, CancellationToken.None);

            // Assert
            foreach (var account in client.Accounts)
            {
                _cacheServiceMock.Verify(x => x.Set(CacheType.AccountInfo,
                    account.Id, It.IsAny<AccountInfoModel>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
            }
        }

        [Fact]
        public async Task ExportMetricsForUsersAsyncShouldStoreDataToCacheIfParamIsNotSet()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();
            var accountId = Guid.NewGuid().ToString();
            var client = GetValidClient(accountId);
            var clients = new List<ClientInfoOptions> { new () { Token = token } };
            _monobankServiceClientMock.Setup(x => x.GetClientInfoAsync(token, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            var service = GetService();

            // Act
            await service.ExportMetricsForUsersAsync(true, clients, CancellationToken.None);

            // Assert
            _cacheServiceMock.Verify(x => x.Set(CacheType.AccountInfo,
                accountId, It.IsAny<AccountInfoModel>(), It.IsAny<MemoryCacheEntryOptions>()), Times.Never);
        }

        [Fact]
        public async Task ExportMetricsForUsersAsyncShouldUseNameFromClientsListIfProvidedForObserving()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();
            var accountId = Guid.NewGuid().ToString();
            var nameFromConfig = "John Wick";
            var client = GetValidClient(accountId, name: "John Doe");
            var clients = new List<ClientInfoOptions> { new () { Token = token, Name = nameFromConfig } };
            _monobankServiceClientMock.Setup(x => x.GetClientInfoAsync(token, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            AccountInfoModel expectedAccount = null;
            _metricsExporterMock.Setup(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()))
                .Callback((AccountInfoModel account, double balance) => expectedAccount = account);
            var service = GetService();

            // Act
            await service.ExportMetricsForUsersAsync(true, clients, CancellationToken.None);

            // Assert
            Assert.NotNull(expectedAccount);
            Assert.Equal(expectedAccount.HolderName, client.Name);
            Assert.Equal(expectedAccount.HolderName, nameFromConfig);
        }

        #endregion

        #region ExportMetricsForCurrenciesAsync

        [Fact]
        public async Task ExportCurrenciesMetricsAsyncShouldNotCallExporterServiceIfMetricsCollectionIsNull()
        {
            // Arrange
            _currencyClientMock.Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
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
            _currencyClientMock.Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
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
            _currencyClientMock.Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
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
            _currencyClientMock.Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
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
            return new MonobankService(options, _monoClientMock.Object, _metricsExporterMock.Object, _cacheServiceMock.Object, _loggerMock.Object);
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
}