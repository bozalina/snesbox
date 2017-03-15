using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Snes;

namespace SnesBox.Components
{
	public enum Filter
	{
		None,
		HQ2X
	}

	public interface IVideoService
	{
		Filter Filter { get; set; }
	}

	public class Video : DrawableGameComponent, IVideoService
	{
		private readonly Dictionary<Filter, Effect> _effects = new Dictionary<Filter, Effect>();
		private Texture2D _frame;
		private Rectangle _frameRectangle;
		private SpriteBatch _spriteBatch;

		public Video(Game game) : base(game)
		{
			game.Services.AddService(typeof(IVideoService), this);
			LibSnes.VideoRefresh += OnVideoRefresh;
		}

		public Filter Filter { get; set; }

		public override void Initialize()
		{
			_frame = new Texture2D(Game.GraphicsDevice, 512, 512, false, SurfaceFormat.Color);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

#if !PLAYSTATION4
			LoadFilters();
#endif
		}

		private void LoadFilters()
		{
			Viewport viewport = GraphicsDevice.Viewport;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0,
				viewport.Width,
				viewport.Height,
				0,
				0,
				1);
			Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

			foreach (Filter effectType in EnumExtensions.GetEnumValues<Filter>())
			{
				var effect = Game.Content.Load<Effect>(effectType.ToString());
				effect.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection);
				effect.Parameters["TextureSize"]?.SetValue(new Vector2(_frame.Width, _frame.Height));
				_effects.Add(effectType, effect);
			}
		}

		public override void Draw(GameTime gameTime)
		{
			Viewport viewport = GraphicsDevice.Viewport;

			Effect effect = _effects.ContainsKey(Filter) ? _effects[Filter] : null;
			_spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect);
			Game.GraphicsDevice.SamplerStates[0] = new SamplerState { Filter = TextureFilter.Point };
			_spriteBatch.Draw(_frame,
				new Rectangle(0, 0, viewport.Width, viewport.Height),
				_frameRectangle,
				Color.White);
			_spriteBatch.End();
		}

		private readonly Color[] _buffer = new Color[512 * 512];

		private void OnVideoRefresh(object sender, VideoRefreshEventArgs e)
		{
			GraphicsDevice.Textures[0] = null;

			int width = e.Destination.Item3;
			int height = e.Destination.Item4;
			bool interlace = height >= 240;
			uint pitch = interlace ? 1024U : 2048U;
			pitch >>= 1;

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					ushort color = e.Data.Array[e.Data.Offset + y * pitch + x];
					int b = ((color >> 10) & 31) * 8;
					int red = b + b / 35;
					b = ((color >> 5) & 31) * 8;
					int green = b + b / 35;
					b = ((color >> 0) & 31) * 8;
					int blue = b + b / 35;

					_buffer[y * 512 + x] = new Color(red, green, blue);
				}
			}

			_frame.SetData(_buffer);
			_frameRectangle = new Rectangle(e.Destination.Item1, e.Destination.Item2, width, height);
		}
	}
}