using System;

namespace SnesBox
{
#if WINDOWS || LINUX || PLAYSTATION4
	/// <summary>
	/// The main class.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			using (var game = new SnesBoxGame { CartridgeName = "Super Mario World (USA)" })
			{
				game.Run();
			}
		}
	}
#endif
}
