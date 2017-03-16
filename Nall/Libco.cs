using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nall
{
	public static class Libco
	{
		private static readonly PauseTokenSource MainPauseToken = new PauseTokenSource();

		public static Task Create(Func<PauseToken, CancellationToken, Task> entryPoint)
		{
			var pauseToken = new PauseTokenSource { IsPaused = true };
			var cancellationToken = new CancellationTokenSource();
			Task task = Task.Run(() => entryPoint(pauseToken.Token, cancellationToken.Token), cancellationToken.Token);
			PauseTokens.Add(task, pauseToken);
			CancellationTokens.Add(task, cancellationToken);
			return task;
		}

		public static void Delete(Task task)
		{
			if (CancellationTokens.ContainsKey(task))
			{
				CancellationTokens[task].Cancel();
			}
		}

		public static void Exit()
		{
			foreach (CancellationTokenSource tokenSource in CancellationTokens.Values)
			{
				tokenSource.Cancel();
			}
		}

		private static readonly Dictionary<Task, PauseTokenSource> PauseTokens = new Dictionary<Task, PauseTokenSource>();
		private static readonly Dictionary<Task, CancellationTokenSource> CancellationTokens = new Dictionary<Task, CancellationTokenSource>();
		private static Task s_active;

		public static async Task Switch(Task task)
		{
			Task previousTask = s_active;
			s_active = task;

			PauseTokenSource tokenToPause = previousTask != null ? PauseTokens[previousTask] : MainPauseToken;
			PauseTokenSource tokenToUnpause = task != null ? PauseTokens[task] : MainPauseToken;

			tokenToPause.IsPaused = true;
			tokenToUnpause.IsPaused = false;

			await tokenToPause.WaitWhilePausedAsync();
		}
	}
}