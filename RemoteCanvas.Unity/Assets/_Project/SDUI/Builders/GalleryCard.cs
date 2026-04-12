using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SDUI.Http;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SDUI.Builders
{
	public class GalleryCard : MonoBehaviour
	{
		[SerializeField]
		private Image _thumbnail;
		[SerializeField]
		private TMP_Text _title;
		[SerializeField]
		private TMP_Text _likesCount;
		[SerializeField]
		private Button _likeButton;

		private string _itemId;
		private string _userId;
		private string _baseUrl;
		private int _currentLikes;
		private bool _isLiked;
		private ISDUIHttpClient _http;

		public void Setup(string itemId, string title, int likes,
			string imageUrl, bool isLiked, string userId, string baseUrl, ISDUIHttpClient http)
		{
			_http = http;
			_itemId = itemId;
			_userId = userId;
			_baseUrl = baseUrl;
			_currentLikes = likes;
			_isLiked = isLiked;

			_title.text = title;
			UpdateLikesUI();

			_likeButton.onClick.RemoveAllListeners();
			_likeButton.onClick.AddListener(() => OnLikeClickedAsync().Forget());

			LoadImageAsync(imageUrl, destroyCancellationToken).Forget();
		}

		private async UniTaskVoid OnLikeClickedAsync()
		{
			_likeButton.interactable = false;

			var newLikes = await SendLikeRequestAsync();
			if (newLikes >= 0)
			{
				_currentLikes = newLikes;
				_isLiked = !_isLiked;
				UpdateLikesUI();
			}

			_likeButton.interactable = true;
		}
		
		private async UniTask<int> SendLikeRequestAsync()
		{
			var url = $"{_baseUrl}/gallery_items/{_itemId}/like";

			JObject response;
			if (_isLiked)
				response = await _http.DeleteAsync($"{url}?userId={_userId}");
			else
				response = await _http.PostAsync(url, new JObject { ["userId"] = _userId });

			return response["likes"].Value<int>();
		}

		private void UpdateLikesUI()
		{
			_likesCount.text = _currentLikes.ToString();
			
			var colors = _likeButton.colors;
			colors.normalColor = _isLiked
				? new Color(1f, 0.3f, 0.3f)
				: Color.white;
			_likeButton.colors = colors;
		}

		private async UniTaskVoid LoadImageAsync(string url, CancellationToken ct)
		{
			using var req = UnityWebRequestTexture.GetTexture(url);
			await req.SendWebRequest().WithCancellation(ct);

			if (req.result != UnityWebRequest.Result.Success)
				return;

			var tex = DownloadHandlerTexture.GetContent(req);
			_thumbnail.sprite = Sprite.Create(
				tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
		}
	}
}