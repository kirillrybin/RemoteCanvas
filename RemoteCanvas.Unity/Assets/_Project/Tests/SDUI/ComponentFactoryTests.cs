using Cysharp.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SDUI.Core;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace SDUI.Tests
{
	[TestFixture]
	public class ComponentFactoryTests
	{
		private ComponentFactory _factory;
		private Mock<IComponentBuilder> _buttonBuilderMock;
		private Mock<IComponentBuilder> _textBuilderMock;
		private Transform _root;

		[SetUp]
		public void SetUp()
		{
			_buttonBuilderMock = new Mock<IComponentBuilder>();
			_buttonBuilderMock
				.Setup(x => x.Type)
				.Returns("button");

			_textBuilderMock = new Mock<IComponentBuilder>();
			_textBuilderMock
				.Setup(x => x.Type)
				.Returns("text");

			_factory = new ComponentFactory(new[]
			{
				_buttonBuilderMock.Object,
				_textBuilderMock.Object
			});
			
			_root = new GameObject("Root").transform;
		}
		
		[TearDown]
		public void TearDown()
		{
			Object.DestroyImmediate(_root.gameObject);
		}

		[UnityTest]
		public IEnumerator Build_CallsCorrectBuilder_ByType() => UniTask.ToCoroutine(async () =>
		{
			var json = JObject.Parse(@"{ ""type"": ""button"", ""label"": ""OK"" }");

			_buttonBuilderMock
				.Setup(x => x.BuildAsync(json, _root, CancellationToken.None))
				.Returns(UniTask.FromResult<GameObject>(null));

			await _factory.BuildAsync(json, _root);

			_buttonBuilderMock.Verify(x => x.BuildAsync(json, _root, CancellationToken.None), Times.Once);
			_textBuilderMock.Verify(x => x.BuildAsync(It.IsAny<JObject>(), It.IsAny<Transform>(), CancellationToken.None), Times.Never);
		});

		[UnityTest]
		public IEnumerator Build_ReturnsFallback_WhenTypeIsMissing() => UniTask.ToCoroutine(async () =>
		{
			var json = JObject.Parse(@"{ ""label"": ""no type here"" }");
			var go   = await _factory.BuildAsync(json, _root);

			Assert.IsNotNull(go);
			Object.DestroyImmediate(go);
		});

		[UnityTest]
		public IEnumerator Build_ReturnsFallback_WhenTypeIsUnknown() => UniTask.ToCoroutine(async () =>
		{
			var json = JObject.Parse(@"{ ""type"": ""unknown_widget"" }");
			var go   = await _factory.BuildAsync(json, _root);

			Assert.IsNotNull(go);
			Object.DestroyImmediate(go);
		});
	}
}