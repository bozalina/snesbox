using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace SnesBox.Content
{
	public sealed class SFCCartridgeReader : ContentTypeReader<SFCCartridge>
	{
		protected override SFCCartridge Read(ContentReader input, SFCCartridge existingInstance)
		{
			int romLength = input.ReadInt32();
			byte[] rom = input.ReadBytes(romLength);
			return new SFCCartridge(rom);
		}
	}
}
