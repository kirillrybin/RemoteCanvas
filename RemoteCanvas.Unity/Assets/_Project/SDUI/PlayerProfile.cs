using SDUI.Core;

namespace SDUI
{
	public class PlayerProfile : IPlayerProfile
	{
		private const string FallbackLanguage = "en";

		public string UserId { get; private set; }
		public string Language { get; private set; }

		public PlayerProfile(string userId, string language = FallbackLanguage)
		{
			UserId = userId;
			Language = string.IsNullOrWhiteSpace(language) ? FallbackLanguage : language;
		}

		public void SetUserId(string userId)
		{
			UserId = userId;
		}

		public void SetLanguage(string languageCode)
		{
			Language = string.IsNullOrWhiteSpace(languageCode) ? FallbackLanguage : languageCode;
		}
	}
}