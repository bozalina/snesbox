using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Snes
{
	internal sealed class LibSnesInterface : Interface
	{
		public static readonly LibSnesInterface Inter = new LibSnesInterface();

		public void video_refresh(ArraySegment<ushort> data, uint width, uint height)
		{
			if (!ReferenceEquals(pvideo_refresh, null))
			{
				pvideo_refresh(null, Video.GetRefreshEventArgs(data, width, height));
			}

			if (!ReferenceEquals(paudio_sample, null))
			{
				AudioRefreshEventArgs audioArgs = Audio.GetRefreshEventArgs();
				if (audioArgs.Buffer.Length > 0)
				{
					paudio_sample(null, audioArgs);
				}
			}
		}

		public void audio_sample(ushort l_sample, ushort r_sample)
		{
			Audio.AddSample(l_sample, r_sample);
		}

		public void input_poll()
		{
			if (!ReferenceEquals(pinput_poll, null))
			{
				pinput_poll(this, EventArgs.Empty);
			}
		}

		public short input_poll(bool port, Input.Device device, uint index, uint id)
		{
			if (ReferenceEquals(pinput_state, null))
			{
				return 0;
			}
			var args = new InputStateEventArgs((Port)Convert.ToInt32(port), (Device)device, index, id);
			pinput_state(null, args);
			return args.State;
		}

		public event EventHandler<VideoRefreshEventArgs> pvideo_refresh;
		public event EventHandler<AudioRefreshEventArgs> paudio_sample;
		public event EventHandler pinput_poll;
		public event EventHandler<InputStateEventArgs> pinput_state;

		private static class Video
		{
			private static readonly int[] _buffer;

			static Video()
			{
				_buffer = new int[512 * 512 * 3];
			}

			public static VideoRefreshEventArgs GetRefreshEventArgs(ArraySegment<ushort> data,
				uint width,
				uint height)
			{
				bool interlace = height >= 240;
				uint pitch = interlace ? 1024U : 2048U;
				pitch >>= 1;

				for (var y = 0; y < height; y++)
				{
					for (var x = 0; x < width; x++)
					{
						ushort color = data.Array[data.Offset + y * pitch + x];
						int b = ((color >> 10) & 31) * 8;
						int red = b + b / 35;
						b = ((color >> 5) & 31) * 8;
						int green = b + b / 35;
						b = ((color >> 0) & 31) * 8;
						int blue = b + b / 35;

						_buffer[y * 512 + x] = red;
						_buffer[y * 512 + x + 1] = green;
						_buffer[y * 512 + x + 2] = blue;
					}
				}

				return new VideoRefreshEventArgs(_buffer, Tuple.Create(0, 0, (int)width, (int)height));
			}
		}

		private static class Audio
		{
			private static readonly Collection<uint> Buffer;

			static Audio()
			{
				Buffer = new Collection<uint>();
			}

			public static void AddSample(ushort left, ushort right)
			{
				Buffer.Add((uint)((right << 16) | left));
			}

			public static AudioRefreshEventArgs GetRefreshEventArgs()
			{
				var audioBuffer = new byte[Buffer.Count * 4];
				var bufferIndex = 0;

				foreach (byte[] samples in Buffer.Select(BitConverter.GetBytes))
				{
					audioBuffer[bufferIndex++] = samples[0];
					audioBuffer[bufferIndex++] = samples[1];
					audioBuffer[bufferIndex++] = samples[2];
					audioBuffer[bufferIndex++] = samples[3];
				}

				Buffer.Clear();
				return new AudioRefreshEventArgs(audioBuffer);
			}
		}
	}
}