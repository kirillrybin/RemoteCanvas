using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using SDUI.Core;
using SDUI.Http;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace SDUI.Debug
{
	[UsedImplicitly]
	public class DebugPanelController : MonoBehaviour
	{
		[SerializeField]
		private GameObject _panel;
		[SerializeField]
		private Button _toggleButton;

		[Header("A/B")]
		[SerializeField]
		private Button _groupAButton;
		[SerializeField]
		private Button _groupBButton;

		[Header("Language")]
		[SerializeField]
		private Button _langButtonPrefab;
		[SerializeField]
		private Transform _langButtonContainer;

		[Header("Banners")]
		[SerializeField]
		private Button _bannerSummerButton;
		[SerializeField]
		private Button _bannerHalloweenButton;
		[SerializeField]
		private Button _bannerSeasonButton;

		[Header("Navigation")]
		[SerializeField]
		private Button _navMainMenuButton;
		[SerializeField]
		private Button _navGalleryButton;
		[SerializeField]
		private Button _navNewsButton;
		[SerializeField]
		private Button _navShopButton;
		[SerializeField]
		private Button _reloadButton;

		[Header("Server")]
		[SerializeField]
		private Button _resetServerButton;

		[Inject]
		private ISDUIService _sdui;
		[Inject]
		private SDUINavigator _navigator;
		[Inject]
		private IPlayerProfile _profile;
		[Inject]
		private ISDUIHttpClient _http;
		[Inject]
		private ISDUIDevClient _devClient;
		[Inject]
		private SDUIConfig _config;

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
			_panel.SetActive(false);

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            gameObject.SetActive(false);
            return;
#endif

			_toggleButton.onClick.AddListener(TogglePanel);

			_groupAButton.onClick.AddListener(() => SwitchAbGroup("player_001"));
			_groupBButton.onClick.AddListener(() => SwitchAbGroup("player_002"));

			foreach (var lang in _config.SupportedLanguages)
				CreateLangButton(lang);

			_bannerSummerButton.onClick.AddListener(() => ActivateBanner("summer_sale"));
			_bannerHalloweenButton.onClick.AddListener(() => ActivateBanner("halloween_promo"));
			_bannerSeasonButton.onClick.AddListener(() => ActivateBanner("new_season"));

			_navMainMenuButton.onClick.AddListener(() => Navigate("main_menu"));
			_navGalleryButton.onClick.AddListener(() => Navigate("gallery"));
			_navNewsButton.onClick.AddListener(() => Navigate("news"));
			_navShopButton.onClick.AddListener(() => Navigate("shop"));
			_reloadButton.onClick.AddListener(() => ReloadAsync().Forget());

			_resetServerButton.onClick.AddListener(() => ResetServerAsync().Forget());
		}

		private void TogglePanel() => _panel.SetActive(!_panel.activeSelf);

		private void CreateLangButton(string lang)
		{
			var btn = Instantiate(_langButtonPrefab, _langButtonContainer);
			btn.GetComponentInChildren<TMP_Text>().text = lang.ToUpper();
			btn.onClick.AddListener(() => SwitchLanguage(lang));
		}

		private void SwitchAbGroup(string userId)
		{
			_profile.SetUserId(userId);
			_sdui.InvalidateCache("main_menu");
			_navigator.GoToAsync("main_menu").Forget();
		}

		private void SwitchLanguage(string lang)
		{
			_sdui.ChangeLanguageAsync(lang,
					_navigator.CurrentPage,
					_navigator.Root,
					destroyCancellationToken)
				.Forget();
		}

		private void ActivateBanner(string bannerId) => ActivateBannerAsync(bannerId).Forget();

		private async UniTaskVoid ActivateBannerAsync(string bannerId)
		{
			await _http.PatchAsync($"{_config.BaseUrl}/banners/{bannerId}/activate");
			_sdui.InvalidateCache("main_menu");
			await _navigator.GoToAsync("main_menu");
		}

		private void Navigate(string page) => _navigator.GoToAsync(page).Forget();

		private async UniTaskVoid ResetServerAsync()
		{
			await _devClient.ResetServerAsync();
			_sdui.InvalidateCache();
			await _navigator.GoToAsync("main_menu");
		}
		
		private async UniTaskVoid ReloadAsync()
		{
			_sdui.InvalidateCache(_navigator.CurrentPage);
			await _navigator.GoToAsync(_navigator.CurrentPage);
		}
	}
}