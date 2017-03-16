using System;
using System.Threading.Tasks;

namespace Snes
{
    class DSP2DR : Memory
    {
        public static DSP2DR dsp2dr = new DSP2DR();

        public override uint size() { throw new NotImplementedException(); }
        public override Task<byte> read(uint addr) { throw new NotImplementedException(); }
        public override Task write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
