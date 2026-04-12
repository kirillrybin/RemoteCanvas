using Cysharp.Threading.Tasks;
using SDUI.Core;
using System;
using System.Threading;
using UnityEngine;
using VContainer;

namespace SDUI
{
	public class SDUIEntryPoint : MonoBehaviour
	{
		[Inject] 
		private ISDUIService _sduiService;
		[Inject]
		private SDUINavigator _navigator;
		[Inject]
		private ActionDispatcher _dispatcher;

		[SerializeField]
		private Transform _contentRoot;
		[SerializeField] 
		private GameObject _loadingOverlay;
		
		private CancellationTokenSource _cts;

		private void Start()
		{
			_cts = new CancellationTokenSource();
			_navigator.Initialize(_contentRoot, _cts.Token);
			
			_sduiService.OnLoadingStarted  += OnLoadingStarted;
			_sduiService.OnLoadingFinished += OnLoadingFinished;
			_sduiService.OnLoadingFailed   -= OnLoadingFailed;

			_dispatcher.Register("open_shop", _ => _navigator.GoToAsync("shop").Forget());
			_dispatcher.Register("open_event", _ => _navigator.GoToAsync("halloween_event").Forget());
			_dispatcher.Register("open_daily_bonus", _ => _navigator.GoToAsync("daily_bonus").Forget());
			_dispatcher.Register("back", _ => _navigator.BackAsync().Forget());
			_dispatcher.Register("open_gallery", _ => _navigator.GoToAsync("gallery").Forget());
			_dispatcher.Register("open_news", _ => _navigator.GoToAsync("news").Forget());

			_dispatcher.Register("start_game", _ => UnityEngine.Debug.Log("start_game"));
			_dispatcher.Register("claim_reward", p => UnityEngine.Debug.Log($"claim_reward: {p}"));
			_dispatcher.Register("purchase", p => UnityEngine.Debug.Log($"purchase: {p}"));
			_dispatcher.Register("close", _ => _navigator.BackAsync().Forget());

			_navigator.GoToAsync("main_menu").Forget();
		}

		private void OnDestroy()
		{
			_sduiService.OnLoadingStarted  -= OnLoadingStarted;
			_sduiService.OnLoadingFinished -= OnLoadingFinished;			
			_sduiService.OnLoadingFailed -= OnLoadingFailed;			
			
			_cts?.Cancel();
			_cts?.Dispose();
		}

		private void OnLoadingStarted()  => _loadingOverlay.SetActive(true);
		private void OnLoadingFinished() => _loadingOverlay.SetActive(false);
		private void OnLoadingFailed(Exception e) => UnityEngine.Debug.LogError($"[SDUI] {e.Message}");
	}
}