using System.Threading;
using Cysharp.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SDUI.Core;
using SDUI.Http;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace SDUI.Tests
{
    [TestFixture]
    public class SDUIServiceTests
    {
        private Mock<ISDUIHttpClient> _httpMock;
        private Mock<IUIBuilder> _builderMock;
        private ISDUIService _service;
        private SDUIConfig _config;
        private IPlayerProfile _profileA;
        private IPlayerProfile _profileB;

        [SetUp]
        public void SetUp()
        {
            _httpMock = new Mock<ISDUIHttpClient>();
            _builderMock = new Mock<IUIBuilder>();
            _config = ScriptableObject.CreateInstance<SDUIConfig>();

            _profileA = new PlayerProfile("player_000", "en");
            _profileB = new PlayerProfile("player_001", "en");

            _service = new SDUIService(_builderMock.Object, _httpMock.Object, _config, _profileA);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [UnityTest]
        public IEnumerator LoadPage_CallsHttpGet_WithCorrectUrl() => UniTask.ToCoroutine(async () =>
        {
            var pageJson = JObject.Parse(@"{
                ""id"": ""main_menu"",
                ""children"": []
            }");

            _httpMock
                .Setup(x => x.GetAsync(
                    It.Is<string>(u => u.Contains("main_menu")),
                    It.IsAny<CancellationToken>()))
                .Returns(UniTask.FromResult<JToken>(pageJson));

            await _service.LoadPageAsync("main_menu", null);

            _httpMock.Verify(
                x => x.GetAsync(
                    It.Is<string>(u => u.EndsWith("main_menu.json?userId=player_000")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        });
        
        [UnityTest]
        public IEnumerator LoadPage_IncludesUserId_InUrl() => UniTask.ToCoroutine(async () =>
        {
            var pageJson = JObject.Parse(@"{ ""id"": ""main_menu"", ""children"": [] }");

            _httpMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(UniTask.FromResult<JToken>(pageJson));

            await _service.LoadPageAsync("main_menu", null);

            _httpMock.Verify(
                x => x.GetAsync(
                    It.Is<string>(u => u.Contains("userId=player_000")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        });
        
        [UnityTest]
        public IEnumerator LoadPage_DifferentUrl_ForDifferentProfiles() => UniTask.ToCoroutine(async () =>
        {
            var pageJson = JObject.Parse(@"{ ""id"": ""main_menu"", ""children"": [] }");

            _httpMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(UniTask.FromResult<JToken>(pageJson));

            var serviceB = new SDUIService(
                _builderMock.Object, _httpMock.Object, _config, _profileB);

            await _service.LoadPageAsync("main_menu", null);
            await serviceB.LoadPageAsync("main_menu", null);

            _httpMock.Verify(
                x => x.GetAsync(
                    It.Is<string>(u => u.Contains("userId=player_000")),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _httpMock.Verify(
                x => x.GetAsync(
                    It.Is<string>(u => u.Contains("userId=player_001")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        });

        [UnityTest]
        public IEnumerator LoadPage_UsesCachedResult_OnSecondCall() => UniTask.ToCoroutine(async () =>
        {
            var pageJson = JObject.Parse(@"{ ""id"": ""shop"", ""children"": [] }");

            _httpMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(UniTask.FromResult<JToken>(pageJson));

            await _service.LoadPageAsync("shop", null);
            await _service.LoadPageAsync("shop", null);

            _httpMock.Verify(
                x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        });

        [UnityTest]
        public IEnumerator LoadPage_FetchesAgain_AfterCacheInvalidation() => UniTask.ToCoroutine(async () =>
        {
            var pageJson = JObject.Parse(@"{ ""id"": ""shop"", ""children"": [] }");

            _httpMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(UniTask.FromResult<JToken>(pageJson));

            await _service.LoadPageAsync("shop", null);
            _service.InvalidateCache("shop");
            await _service.LoadPageAsync("shop", null);

            _httpMock.Verify(
                x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        });

        [UnityTest]
        public IEnumerator LoadPage_Throws_WhenHttpFails() => UniTask.ToCoroutine(async () =>
        {
            Exception caughtEx = null;
            _service.OnLoadingFailed += e => caughtEx = e;

            _httpMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => UniTask.FromException<JToken>(
                    new SDUIHttpException(404, "Not Found", "/pages/missing.json")));

            await _service.LoadPageAsync("missing", null);

            Assert.IsNotNull(caughtEx);
            Assert.IsInstanceOf<SDUIHttpException>(caughtEx);
            Assert.AreEqual(404, ((SDUIHttpException)caughtEx).StatusCode);
        });
        
        [UnityTest]
        public IEnumerator LoadPage_FetchesAgain_WhenCacheExpired() => UniTask.ToCoroutine(async () =>
        {
            _config.CacheTtlSeconds = 0f;

            var pageJson = JObject.Parse(@"{ ""id"": ""shop"", ""children"": [] }");

            _httpMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(UniTask.FromResult<JToken>(pageJson));

            await _service.LoadPageAsync("shop", null);
            await _service.LoadPageAsync("shop", null);

            _httpMock.Verify(
                x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        });
    }
}