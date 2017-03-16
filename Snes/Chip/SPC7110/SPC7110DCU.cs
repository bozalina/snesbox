using System;
using System.Threading.Tasks;

namespace Snes
{
    class SPC7110DCU : Memory
    {
        public static SPC7110DCU spc7110dcu = new SPC7110DCU();

        public override Task<byte> read(uint addr) { throw new NotImplementedException(); }
        public override Task write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
