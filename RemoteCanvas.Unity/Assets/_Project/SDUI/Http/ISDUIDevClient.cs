using Cysharp.Threading.Tasks;
using System.Threading;

namespace SDUI.Http
{
	public interface ISDUIDevClient
	{
		UniTask ResetServerAsync(CancellationToken ct = default);
	}
}