using System;
using System.Threading;
using System.Threading.Tasks;
using Monobank.Core.Models;
using MonobankExporter.BusinessLogic.Interfaces;
using MonobankExporter.BusinessLogic.Models;
using MonobankExporter.BusinessLogic.Services;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace MonobankExporter.UnitTests.Services
{
    public class MonobankServiceTests
    {
        private readonly Mock<IPrometheusExporterService> _prometheusServiceMock;
        private readonly Mock<IRedisCacheService> _redisCacheServiceMock;

        public MonobankServiceTests()
        {
            _prometheusServiceMock = new Mock<IPrometheusExporterService>();
            _redisCacheServiceMock = new Mock<IRedisCacheService>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        //[InlineData("http://asd/webhook")]
        //[InlineData("https://asd/webhook")]
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
        public async Task ExportMetricsForWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfModelIsNull()
        {
            // Arrange
            var service = GetService();

            // Act
            await service.ExportMetricsForWebHook(null, CancellationToken.None);

            // Assert
            _prometheusServiceMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            _redisCacheServiceMock.Verify(x => x.GetRecordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExportMetricsForWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfDataIsNull()
        {
            // Arrange
            var webhook = new WebHookModel
            {
                Data = null
            };
            var service = GetService();

            // Act
            await service.ExportMetricsForWebHook(webhook, CancellationToken.None);

            // Assert
            _prometheusServiceMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            _redisCacheServiceMock.Verify(x => x.GetRecordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExportMetricsForWebHookShouldNotRetrieveRecordFromCacheAndCallExportServiceIfAccountIsNull()
        {
            // Arrange
            var webhook = new WebHookModel
            {
                Data = new WebHookData { Account = null }
            };
            var service = GetService();

            // Act
            await service.ExportMetricsForWebHook(webhook, CancellationToken.None);

            // Assert
            _prometheusServiceMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            _redisCacheServiceMock.Verify(x => x.GetRecordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
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
            await service.ExportMetricsForWebHook(webhook, CancellationToken.None);

            // Assert
            _prometheusServiceMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            _redisCacheServiceMock.Verify(x => x.GetRecordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExportMetricsForWebHookShouldRetrieveRecordFromCacheAndDontCallExporterServiceIfRecordInCacheNotValid()
        {
            // Arrange
            var account = Guid.NewGuid().ToString();
            var webhook = new WebHookModel
            {
                Data = new WebHookData { Account = account, StatementItem = new Statement() }
            };
            _redisCacheServiceMock
                .Setup(x => x.GetRecordAsync(It.IsAny<string>(), CancellationToken.None))
                .ReturnsAsync(() => null);
            var service = GetService();

            // Act
            await service.ExportMetricsForWebHook(webhook, CancellationToken.None);

            // Assert
            _prometheusServiceMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), It.IsAny<double>()), Times.Never);
            _redisCacheServiceMock.Verify(x => x.GetRecordAsync(account, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExportMetricsForWebHookShouldCallExportService()
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
            _redisCacheServiceMock
                .Setup(x => x.GetRecordAsync(webhook.Data.Account, CancellationToken.None))
                .ReturnsAsync(JsonConvert.SerializeObject(accountInfo));
            var service = GetService();

            // Act
            await service.ExportMetricsForWebHook(webhook, CancellationToken.None);

            // Assert
            _prometheusServiceMock.Verify(x => x.ObserveAccount(It.IsAny<AccountInfoModel>(), expectedBalance));
        }

        private MonobankService GetService(MonobankExporterOptions options = null)
        {
            options ??= new MonobankExporterOptions();
            return new MonobankService(options, _prometheusServiceMock.Object, _redisCacheServiceMock.Object);
        }
    }
}