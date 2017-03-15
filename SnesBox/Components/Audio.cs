using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Snes;

namespace SnesBox.Components
{
	internal sealed class Audio : GameComponent
	{
#if !PLAYSTATION4
		private readonly DynamicSoundEffectInstance _audioFrame = new DynamicSoundEffectInstance(32040,
			AudioChannels.Stereo);
#endif

		public Audio(Game game) : base(game)
		{
			LibSnes.AudioRefresh += OnAudioRefresh;
		}

		public override void Initialize()
		{
#if !PLAYSTATION4
			_audioFrame.Play();
#endif
		}

		private void OnAudioRefresh(object sender, AudioRefreshEventArgs e)
		{
#if !PLAYSTATION4
			_audioFrame.SubmitBuffer(e.Buffer, 0, e.Buffer.Length);
#endif
		}
	}
}