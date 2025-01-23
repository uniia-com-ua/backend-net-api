using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UNIIAadminAPI.Models;
using UNIIAadminAPI.Services;

namespace TokenServiceTests
{
    [TestClass]
    public class TokenServiceTests
    {
        private readonly string clientId = "test-client-id";
        private readonly string clientSecret = "test-client-secret";
        private readonly string tokenkey = "okWshSl7frI8EUicqXLZSDF9YBIdLsbd";
        private TokenService _tokenService;

        [TestInitialize]
        public void Setup()
        {
            _tokenService = new TokenService(clientId, clientSecret, tokenkey);
        }

        [TestMethod]
        public async Task RefreshTokenAsync_ReturnsAccessToken_WhenSuccessful()
        {
            var refreshToken = "test-refresh-token";
            var expectedAccessToken = "new-access-token";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(new { access_token = expectedAccessToken }), Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var result = await _tokenService.RefreshTokenAsync(refreshToken, httpClient);

            Assert.AreEqual(expectedAccessToken, result);
        }

        [TestMethod]
        public async Task RefreshTokenAsync_ReturnsNull_WhenUnsuccessful()
        {
            var refreshToken = "test-refresh-token";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var result = await _tokenService.RefreshTokenAsync(refreshToken, httpClient);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RefreshTokenAsync_ThrowsException_WhenHttpRequestFails()
        {
            var refreshToken = "test-refresh-token";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException());

            var httpClient = new HttpClient(handlerMock.Object);

            await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _tokenService.RefreshTokenAsync(refreshToken, httpClient));
        }

        [TestMethod]
        public void GetTokensByUser_ReturnsCorrectTokens()
        {
            var originalAccessToken = "v/FVj4xDDkYcWEujp9Vwqg==:O01c0i3pNNBBNodN8tMZ0O7OVKqCYAtC5qfkbuWpUAd0QCEoIpot883VpWJ8W9nBZPADwfeCXwDOpoYCdUnwEcs2X8LPMIFr2Lyni/PvX5N1sFqNzfBDKhC4VFAtrvXDgIIOihO4m/rZPbdIuzdFfvOjciCP4MTxoE7BidTKsdn8UrB+QPIO46xcoRtdpSxmuP+WWJ0kXQk2shJuQONmhp7nFMut3jDcoUs3F0kSfPK8K3X9Xb3vj2QbJEvx0v+6rzkWq4V+0ya8Ev7L8pUm1+hg6pwZs/pWnDdw6Ev0HtQ=";

            var originalRefreshToken = "oLq/R0lSXz9HLSd3tjOqvA==:GvlDzPBdMG14522NLW49IIzIiuNNfh+eJBX9SfdCrqBcPUAvuK6SqZKhc2MN8v2yapX2CkSOrWNnMGcP8ashVZ/VoJSjIAKSPILdJSim78YO/d/XejLH9Zt0w5ivSj7ck8y7V33Th++rsekXE5+YcQ==";

            var user = new MongoIdentityUser
            {
                Tokens = _tokenService.MakeEncryptedTokenList(originalAccessToken, originalRefreshToken)
            };

            var (outputAccessToken, outputRefreshToken) = _tokenService.GetTokensByUser(user);

            Assert.AreEqual(originalAccessToken, outputAccessToken);
            Assert.AreEqual(originalRefreshToken, outputRefreshToken);
        }

        [TestMethod]
        public void GetTokensByUser_ReturnsNulls_WhenTokensAreMissing()
        {
            var user = new MongoIdentityUser
            {
                Tokens = new List<Token>()
            };

            var (accessToken, refreshToken) = _tokenService.GetTokensByUser(user);

            Assert.IsNull(accessToken);
            Assert.IsNull(refreshToken);
        }
    }
}
