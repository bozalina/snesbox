using System;

namespace Snes
{
	public sealed class VideoRefreshEventArgs : EventArgs
	{
		public VideoRefreshEventArgs(int[] buffer, Tuple<int, int, int, int> destination)
		{
			Buffer = buffer;
			Destination = destination;
		}

		public int[] Buffer { get; private set; }
		public Tuple<int, int, int, int> Destination { get; private set; }
	}
}