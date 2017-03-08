using System;
using Microsoft.Xna.Framework;
using Snes;
using SnesBox.Components;
using SnesBox.Content;

namespace SnesBox
{
	public sealed class SnesBoxGame : Game
	{
		public SnesBoxGame()
		{
			IsFixedTimeStep = false;
			GraphicsDeviceManager = new GraphicsDeviceManager(this);
			GraphicsDeviceManager.PreferredBackBufferWidth = 800;
			GraphicsDeviceManager.PreferredBackBufferHeight = 600;
			GraphicsDeviceManager.ApplyChanges();

			Content.RootDirectory = "Content";

			Components.Add(new FrameRate(this));
			Components.Add(new Audio(this));
			Components.Add(new Video(this));
			Components.Add(new Input(this));
		}

		private GraphicsDeviceManager GraphicsDeviceManager { get; }
		public string CartridgeName { get; set; }
		public bool Paused { get; set; }

		public Filter Filter
		{
			get
			{
				var video = (IVideoService)Services.GetService(typeof(IVideoService));
				return video.Filter;
			}
			set
			{
				var video = (IVideoService)Services.GetService(typeof(IVideoService));
				video.Filter = value;
			}
		}

		public void LoadCartridge(string cartridgeName)
		{
			var cartridge = Content.Load<SFCCartridge>(cartridgeName);
			LibSnes.LoadCartridgeNormal(null, cartridge.ROM, (uint)cartridge.ROM.Length);
		}

		protected override void Initialize()
		{
			base.Initialize();

			LibSnes.Init();

			if (!string.IsNullOrEmpty(CartridgeName))
			{
				LoadCartridge(CartridgeName);
			}
		}

		protected override void Update(GameTime gameTime)
		{
			if (!Paused)
			{
				base.Update(gameTime);

				LibSnes.Run();

				var frameRate = (IFrameRateService)Services.GetService(typeof(IFrameRateService));
				Window.Title = string.Format("{0:##} FPS", frameRate.FPS);
			}
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			LibSnes.Exit();
		}
	}
}