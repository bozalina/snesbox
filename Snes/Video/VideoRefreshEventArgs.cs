using System;

namespace Snes
{
	public sealed class VideoRefreshEventArgs : EventArgs
	{
		public VideoRefreshEventArgs(ArraySegment<ushort> data, int width, int height)
		{
			Data = data;
			Destination = Tuple.Create(0, 0, width, height);
		}

		public ArraySegment<ushort> Data { get; private set; }
		public Tuple<int, int, int, int> Destination { get; private set; }
	}
}