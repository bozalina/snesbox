﻿using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace SnesBox
{
    public class SnesBoxGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Snes _snes = new Snes();
        Texture2D _videoFrame;
        DynamicSoundEffectInstance _audioFrame;

        public SnesBoxGame()
        {
            IsFixedTimeStep = false;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
        }

        protected override void Initialize()
        {
            _audioFrame = new DynamicSoundEffectInstance(32040, AudioChannels.Stereo);
            _audioFrame.Play();

            _snes.VideoUpdated += new VideoUpdatedEventHandler(Snes_VideoUpdated);
            _snes.AudioUpdated += new AudioUpdatedEventHandler(Snes_AudioUpdated);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            using (FileStream fs = new FileStream("SMW.smc", FileMode.Open))
            {
                var rom = new byte[fs.Length];
                fs.Read(rom, 0, (int)fs.Length);
                _snes.LoadCartridge(new NormalCartridge() { RomData = rom });
            }
        }

        protected override void Update(GameTime gameTime)
        {
            _snes.RunToFrame();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            var vp = GraphicsDevice.Viewport;
            spriteBatch.Begin();
            spriteBatch.Draw(_videoFrame, new Rectangle(0, 0, vp.Width, vp.Height), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        void Snes_AudioUpdated(object sender, AudioUpdatedEventArgs e)
        {
            var audioBuffer = new byte[e.SampleCount * 4];
            int bufferIndex = 0;

            for (int i = 0; i < e.AudioBuffer.Length; i++)
            {
                var samples = BitConverter.GetBytes(e.AudioBuffer[i]);
                audioBuffer[bufferIndex++] = samples[0];
                audioBuffer[bufferIndex++] = samples[1];
                audioBuffer[bufferIndex++] = samples[2];
                audioBuffer[bufferIndex++] = samples[3];
            }

            if (audioBuffer.Length > 0)
            {
                _audioFrame.SubmitBuffer(audioBuffer, 0, audioBuffer.Length);
            }
        }

        void Snes_VideoUpdated(object sender, VideoUpdatedEventArgs e)
        {
            if (ReferenceEquals(_videoFrame, null))
            {
                _videoFrame = new Texture2D(GraphicsDevice, e.Width, e.Height, false, SurfaceFormat.Color);
            }

            var videoBuffer = new uint[e.Width * e.Height];
            bool interlace = (e.Height >= 240);
            uint pitch = interlace ? 1024U : 2048U;
            pitch >>= 1;

            _videoFrame.GetData<uint>(videoBuffer);

            for (int y = 0; y < e.Height; y++)
            {
                for (int x = 0; x < e.Width; x++)
                {
                    ushort color = e.VideoBuffer.Array[e.VideoBuffer.Offset + (y * pitch) + x];
                    int b;

                    b = ((color >> 10) & 31) * 8;
                    var red = (byte)(b + b / 35);
                    b = ((color >> 5) & 31) * 8;
                    var green = (byte)(b + b / 35);
                    b = ((color >> 0) & 31) * 8;
                    var blue = (byte)(b + b / 35);
                    var alpha = (byte)255;

                    videoBuffer[y * e.Width + x] = new Color() { R = red, G = green, B = blue, A = alpha }.PackedValue;
                }
            }

            GraphicsDevice.Textures[0] = null;
            _videoFrame.SetData<uint>(videoBuffer);
        }
    }
}
