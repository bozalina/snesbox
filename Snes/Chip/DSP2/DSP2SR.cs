using System;
using System.Threading.Tasks;

namespace Snes
{
    class DSP2SR : Memory
    {
        public static DSP2SR dsp2sr = new DSP2SR();

        public override uint size() { throw new NotImplementedException(); }
        public override Task<byte> read(uint addr) { throw new NotImplementedException(); }
        public override Task write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
