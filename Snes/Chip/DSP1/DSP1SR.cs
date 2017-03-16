using System;
using System.Threading.Tasks;

namespace Snes
{
    class DSP1SR : Memory
    {
        public static DSP1SR dsp1sr = new DSP1SR();

        public override uint size() { throw new NotImplementedException(); }
        public override Task<byte> read(uint addr) { throw new NotImplementedException(); }
        public override Task write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
