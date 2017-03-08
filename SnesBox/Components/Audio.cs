using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Snes;

namespace SnesBox.Components
{
    class Audio : GameComponent
    {
        private DynamicSoundEffectInstance _audioFrame = new DynamicSoundEffectInstance(32040, AudioChannels.Stereo);

        public Audio(Game game)
            : base(game)
        {
            LibSnes.AudioRefresh += new EventHandler<AudioRefreshEventArgs>(OnAudioRefresh);
        }

        public override void Initialize()
        {
            _audioFrame.Play();
        }

        private void OnAudioRefresh(object sender, AudioRefreshEventArgs e)
        {
            _audioFrame.SubmitBuffer(e.Buffer, 0, e.Buffer.Length);
        }
    }
}
