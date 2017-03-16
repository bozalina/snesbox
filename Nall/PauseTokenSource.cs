using System.Threading;
using System.Threading.Tasks;

namespace Nall
{
	public sealed class PauseTokenSource
	{
		private volatile TaskCompletionSource<bool> m_paused;

		internal static readonly Task CompletedTask = Task.FromResult(true);

		internal Task WaitWhilePausedAsync()
		{
			TaskCompletionSource<bool> cur = m_paused;
			return cur != null ? cur.Task : CompletedTask;
		}

		public PauseToken Token => new PauseToken(this);

		public bool IsPaused
		{
			get { return m_paused != null; }
			set
			{
				if (value)
				{
					Interlocked.CompareExchange(
						ref m_paused, new TaskCompletionSource<bool>(), null);
				}
				else
				{
					while (true)
					{
						var tcs = m_paused;
						if (tcs == null) return;
						if (Interlocked.CompareExchange(ref m_paused, null, tcs) == tcs)
						{
							tcs.SetResult(true);
							break;
						}
					}
				}
			}
		}
	}
}