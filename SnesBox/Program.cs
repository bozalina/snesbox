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
		private static void Main(string[] args)
		{
			using (var game = new SnesBoxGame { CartridgeName = args[0] })
			{
				game.Run();
			}
		}
	}
#endif
}
