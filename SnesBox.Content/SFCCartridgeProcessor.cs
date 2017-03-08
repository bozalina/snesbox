using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace SnesBox.Content
{
	[ContentProcessor(DisplayName = "SFC Cartridge Processor")]
	public sealed class SFCCartridgeProcessor : ContentProcessor<byte[], SFCCartridge>
	{
		public override SFCCartridge Process(byte[] input, ContentProcessorContext context)
		{
			return new SFCCartridge(input);
		}
	}
}
