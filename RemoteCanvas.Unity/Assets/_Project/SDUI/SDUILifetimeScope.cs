using SDUI.Builders;
using SDUI.Core;
using SDUI.Debug;
using SDUI.Http;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using VContainer.Unity;

namespace SDUI
{
	public class SDUILifetimeScope : LifetimeScope
	{
		[SerializeField]
		private SDUIConfig _config;
		[SerializeField]
		private string _userId = "player_001";

		[Header("Prefabs")]
		[SerializeField]
		private Button _buttonPrefab;
		[SerializeField]
		private TMP_Text _textPrefab;
		[SerializeField]
		private Image _imagePrefab;
		[SerializeField]
		private GameObject _galleryCardPrefab;
		[SerializeField] 
		private GameObject _bannerPrefab;
		[SerializeField] 
		private GameObject _newsFeedItemPrefab;

		protected override void Configure(IContainerBuilder builder)
		{
			// Config
			builder.RegisterInstance(_config);

			builder.RegisterInstance(new PlayerProfile(_userId)).As<IPlayerProfile>();

			// Prefab wrappers
			builder.RegisterInstance(new ButtonPrefab { Value = _buttonPrefab });
			builder.RegisterInstance(new TextPrefab { Value = _textPrefab });
			builder.RegisterInstance(new ImagePrefab { Value = _imagePrefab });
			builder.RegisterInstance(new GalleryCardPrefab { Value = _galleryCardPrefab });
			builder.RegisterInstance(new BannerPrefab { Value = _bannerPrefab });
			builder.RegisterInstance(new NewsFeedItemPrefab { Value = _newsFeedItemPrefab });

			// Builders are collected into IEnumerable<IComponentBuilder> by the container
			builder.Register<ButtonBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<TextBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ImageBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<PanelBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<GalleryBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<SpacerBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<BannerBuilder>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<NewsFeedBuilder>(Lifetime.Singleton).AsImplementedInterfaces();

			// Core services
			builder.Register<ActionDispatcher>(Lifetime.Singleton);
			builder.Register<ComponentFactory>(Lifetime.Singleton);
			builder.Register<UIBuilder>(Lifetime.Singleton).As<IUIBuilder>();
			builder.Register<SDUIService>(Lifetime.Singleton).As<ISDUIService>();
			builder.Register<SDUINavigator>(Lifetime.Singleton);
			builder.Register<ProjectHttpClient>(Lifetime.Singleton).AsImplementedInterfaces();

			builder.RegisterComponentInHierarchy<SDUIEntryPoint>();

#if UNITY_EDITOR
			builder.RegisterComponentInHierarchy<DebugPanelController>();
#endif
		}
	}
}