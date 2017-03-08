using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Snes;

namespace SnesBox.Components
{
    public enum Filter { None, HQ2X }

    public interface IVideoService
    {
        Filter Filter { get; set; }
    }

    public class Video : DrawableGameComponent, IVideoService
    {
        private Texture2D _frame;
        private Rectangle _frameRectangle;
        private SpriteBatch _spriteBatch;
        public Filter Filter { get; set; }

        private Dictionary<Filter, Effect> _effects = new Dictionary<Filter, Effect>();

        public Video(Game game)
            : base(game)
        {
            game.Services.AddService(typeof(IVideoService), this);
            LibSnes.VideoRefresh += new EventHandler<VideoRefreshEventArgs>(OnVideoRefresh);
        }

        public override void Initialize()
        {
            _frame = new Texture2D(Game.GraphicsDevice, 512, 512, false, SurfaceFormat.Color);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            LoadFilters();
        }

        private void LoadFilters()
        {
            var viewport = GraphicsDevice.Viewport;
            var projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            var halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            foreach (var effectType in EnumExtensions.GetEnumValues<Filter>())
            {
                var effect = Game.Content.Load<Effect>(effectType.ToString());
                effect.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection);
                effect.Parameters["TextureSize"]?.SetValue(new Vector2(_frame.Width, _frame.Height));
                _effects.Add((Filter)effectType, effect);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var viewport = GraphicsDevice.Viewport;

            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, _effects[Filter]);
            Game.GraphicsDevice.SamplerStates[0] = new SamplerState() { Filter = TextureFilter.Point };
            _spriteBatch.Draw(_frame, new Rectangle(0, 0, viewport.Width, viewport.Height), _frameRectangle, Color.White);
            _spriteBatch.End();
        }

        void OnVideoRefresh(object sender, VideoRefreshEventArgs e)
        {
            GraphicsDevice.Textures[0] = null;
            _frame.SetData<Color>(e.Buffer);
            _frameRectangle = e.Destination;
        }
    }
}
