using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace SnesBox.Content
{
	[ContentTypeWriter]
	public sealed class SFCCartridgeWriter : ContentTypeWriter<SFCCartridge>
	{
		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return typeof(SFCCartridgeReader).AssemblyQualifiedName;
		}

		public override string GetRuntimeType(TargetPlatform targetPlatform)
		{
			return typeof(SFCCartridge).AssemblyQualifiedName;
		}

		protected override void Write(ContentWriter output, SFCCartridge value)
		{
			output.Write(value.ROM.Length);
			output.Write(value.ROM);
		}
	}
}
