using System.Threading.Tasks;

namespace Nall
{
	public struct PauseToken
	{
		private readonly PauseTokenSource m_source;
		internal PauseToken(PauseTokenSource source) { m_source = source; }

		public bool IsPaused => m_source != null && m_source.IsPaused;

		public Task WaitWhilePausedAsync()
		{
			return IsPaused ?
				m_source.WaitWhilePausedAsync() :
				PauseTokenSource.CompletedTask;
		}
	}
}