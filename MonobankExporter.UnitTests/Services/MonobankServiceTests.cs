using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Monobank.Core.Models;
using MonobankExporter.BusinessLogic.Interfaces;
using MonobankExporter.BusinessLogic.Models;
using MonobankExporter.BusinessLogic.Services;
using Moq;
using Xunit;

namespace MonobankExporter.UnitTests.Services
{
    public class MonobankServiceTests
    {
        private readonly Mock<IMetricsExporterService> _metricsExporterMock;
        private readonly Mock<ILookupsMemoryCache> _cacheServiceMock;
        private readonly Mock<ILogger<MonobankService>> _loggerMock;

        private AccountInfoModel _tryGetResult = null;

        public MonobankServiceTests()
        {
            _metricsExporterMock = new Mock<IMetricsExporterService>();
            _cacheServiceMock = new Mock<ILookupsMemoryCache>();
            _loggerMock = new Mock<ILogger<MonobankService>>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
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
        public void WebHookUrlIsValidShouldReturnTrueIfUrlIsValid(string webHookUrl)
        {
            // Arrange
            var service = GetService();

            // Act
            var result = service.WebHookUrlIsValid(webHookUrl);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ExportMetricsForWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfModelIsNull()
        {
            // Arrange
            var service = GetService();

            // Act
            service.ExportMetricsForWebHook(null, CancellationToken.None);

            // Assert
            _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<object>(), out _tryGetResult), Times.Never);
        }

        [Fact]
        public void ExportMetricsForWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfDataIsNull()
        {
            // Arrange
            var webhook = new WebHookModel
            {
                Data = null
            };
            var service = GetService();

            // Act
            service.ExportMetricsForWebHook(webhook, CancellationToken.None);

            // Assert
            _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<object>(), out _tryGetResult), Times.Never);
        }

        [Fact]
        public void ExportMetricsForWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfAccountIsNull()
        {
            // Arrange
            var webhook = new WebHookModel
            {
                Data = new WebHookData { Account = null }
            };
            var service = GetService();

            // Act
            service.ExportMetricsForWebHook(null, CancellationToken.None);

            // Assert
            _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<object>(), out _tryGetResult), Times.Never);
        }

        [Fact]
        public async Task ExportMetricsForWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfStatementItemIsNull()
        {
            // Arrange
            var webhook = new WebHookModel
            {
                Data = new WebHookData { Account = "SomeAccount", StatementItem = null }
            };
            var service = GetService();

            // Act
            service.ExportMetricsForWebHook(webhook, CancellationToken.None);

            // Assert
            _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            AccountInfoModel result;
            _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, It.IsAny<object>(), out result), Times.Never);
        }

        [Fact]
        public void ExportMetricsForWebHookShouldRetrieveRecordFromCacheAndDontCallExporterServiceIfRecordInCacheNotValid()
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
            service.ExportMetricsForWebHook(webhook, CancellationToken.None);

            // Assert
            _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            _cacheServiceMock.Verify(x => x.TryGetValue(CacheType.AccountInfo, account, out _tryGetResult), Times.Once);
        }

        [Fact]
        public void ExportMetricsForWebHookShouldCallExportService()
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
            service.ExportMetricsForWebHook(webhook, CancellationToken.None);

            // Assert
            _metricsExporterMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), expectedBalance), Times.Once);
        }

        private MonobankService GetService(MonobankExporterOptions options = null)
        {
            options ??= new MonobankExporterOptions();
            return new MonobankService(options, _metricsExporterMock.Object, _cacheServiceMock.Object, _loggerMock.Object);
        }
    }
}