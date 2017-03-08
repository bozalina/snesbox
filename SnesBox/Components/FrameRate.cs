using System;
using Microsoft.Xna.Framework;

namespace SnesBox.Components
{
    public interface IFrameRateService
    {
        string FPS { get; }
    }

    public class FrameRate : DrawableGameComponent, IFrameRateService
    {
        int _frameRate = 0;
        int _frameCounter = 0;
        TimeSpan _elapsedTime = TimeSpan.Zero;
        public string FPS { get; private set; }

        public FrameRate(Game game)
            : base(game)
        {
            game.Services.AddService(typeof(IFrameRateService), this);
        }

        public override void Draw(GameTime gameTime)
        {
            _frameCounter++;

            _elapsedTime += gameTime.ElapsedGameTime;

            if (_elapsedTime > TimeSpan.FromSeconds(1))
            {
                _elapsedTime -= TimeSpan.FromSeconds(1);
                _frameRate = _frameCounter;
                _frameCounter = 0;
            }

            FPS = _frameRate.ToString();
        }
    }
}
