using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace SnesBox.Content
{
	[ContentImporter(".sfc", DefaultProcessor = "SFCCartridgeImpoter", DisplayName = "SFC Cartridge Importer")]
	public sealed class SFCCartridgeImpoter : ContentImporter<byte[]>
	{
		public override byte[] Import(string filename, ContentImporterContext context)
		{
			using (var fs = new FileStream(filename, FileMode.Open))
			{
				var rom = new byte[fs.Length];
				fs.Read(rom, 0, (int)fs.Length);
				return rom;
			}
		}
	}
}