using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using SDUI.Core;
using System.Threading;
using UnityEngine.Networking;

namespace SDUI.Http
{
	[UsedImplicitly]
	public class ProjectHttpClient : SDUIHttpClient, ISDUIDevClient
	{
		private readonly SDUIConfig _config;

		public ProjectHttpClient(IPlayerProfile profile, SDUIConfig config)
			: base(profile)
		{
			_config = config;
		}

		public async UniTask ResetServerAsync(CancellationToken ct = default)
		{
			using var request = UnityWebRequest.Post(_config.BaseUrl + "/reset", "", "application/json");
			await request.SendWebRequest().WithCancellation(ct);
		}
	}
}