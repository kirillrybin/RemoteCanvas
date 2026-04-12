using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SDUI.Core;
using SDUI.Http;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace SDUI.Builders
{
	public sealed class GalleryCardPrefab
	{
		public GameObject Value;
	}

	[UsedImplicitly]
	public class GalleryBuilder : IComponentBuilder
	{
		public string Type => "gallery";

		private readonly ISDUIHttpClient _http;
		private readonly SDUIConfig _config;
		private readonly GalleryCardPrefab _cardPrefab;
		private readonly IPlayerProfile _playerProfile;
		
		public GalleryBuilder(ISDUIHttpClient http, GalleryCardPrefab cardPrefab, SDUIConfig config, IPlayerProfile playerProfile)
		{
			_http = http;
			_config = config;
			_playerProfile = playerProfile;
			_cardPrefab = cardPrefab;
		}

		public async UniTask<GameObject> BuildAsync(JObject json, Transform parent, CancellationToken ct)
		{
			var scrollObj = new GameObject("GalleryScroll");
			scrollObj.transform.SetParent(parent, false);

			var scrollRt = scrollObj.AddComponent<RectTransform>();
			scrollRt.anchorMin = Vector2.zero;
			scrollRt.anchorMax = Vector2.one;
			scrollRt.sizeDelta = Vector2.zero;
			
			var layoutElement = scrollObj.AddComponent<LayoutElement>();
			layoutElement.flexibleHeight = 1f;
			layoutElement.minHeight = 200f; 
			
			var maskImage = scrollObj.AddComponent<Image>();
			maskImage.color = Color.white;
			
			var mask = scrollObj.AddComponent<Mask>();
			mask.showMaskGraphic = false; 

			var scrollRect = scrollObj.AddComponent<ScrollRect>();
			scrollRect.horizontal = false;

			var gridObj = new GameObject("GalleryGrid");
			gridObj.transform.SetParent(scrollObj.transform, false);

			var gridRt = gridObj.AddComponent<RectTransform>();
			gridRt.anchorMin = new Vector2(0, 1);
			gridRt.anchorMax = new Vector2(1, 1);
			gridRt.pivot = new Vector2(0.5f, 1f);
			gridRt.sizeDelta = Vector2.zero;

			var grid = gridObj.AddComponent<GridLayoutGroup>();
			grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			grid.constraintCount = json["columns"]?.Value<int>() ?? 2;
			grid.cellSize = new Vector2(160f, 200f);
			grid.spacing = new Vector2(12f, 12f);
			grid.padding = new RectOffset(16,
				16,
				16,
				16);

			var cellWidth = json["cellWidth"]?.Value<float>() ?? 160f;
			var cellHeight = json["cellHeight"]?.Value<float>() ?? 200f;

			grid.cellSize = new Vector2(cellWidth, cellHeight);

			var fitter = gridObj.AddComponent<ContentSizeFitter>();
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			scrollRect.content = gridRt;

			var dataUrl = json["dataUrl"].Value<string>();
			var (items, likedIds) = await UniTask.WhenAll(
				FetchGalleryItemsAsync(dataUrl, ct),
				FetchLikedItemsAsync(ct)
			);

			foreach (var item in items)
			{
				var id = item["id"].Value<string>();
				var card = Object.Instantiate(_cardPrefab.Value, gridObj.transform);

				card.GetComponent<GalleryCard>()
				.Setup(itemId: id,
					title: item["title"].Value<string>(),
					likes: item["likes"].Value<int>(),
					imageUrl: item["imageUrl"].Value<string>(),
					isLiked: likedIds.Contains(id),
					userId: _playerProfile.UserId,
					baseUrl: _config.BaseUrl,
					http: _http);
			}

			return scrollObj;
		}

		private async UniTask<JArray> FetchGalleryItemsAsync(string dataUrl, CancellationToken ct)
		{
			var result = await _http.GetAsync($"{_config.BaseUrl}{dataUrl}", ct);
			return (JArray) result;
		}

		private async UniTask<HashSet<string>> FetchLikedItemsAsync(CancellationToken ct)
		{
			var result = await _http.GetAsync($"{_config.BaseUrl}/liked_items?userId={_playerProfile.UserId}", ct);

			var set = new HashSet<string>();
			foreach (var item in (JArray) result)
				set.Add(item["itemId"].Value<string>());

			return set;
		}
	}
}