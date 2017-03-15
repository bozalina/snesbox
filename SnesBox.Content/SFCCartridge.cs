using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SnesBox.Content
{
	public sealed class SFCCartridge
	{
		public byte[] ROM { get; private set; }

		public SFCCartridge(byte[] rom)
		{
			ROM = rom;
		}
	}
}
