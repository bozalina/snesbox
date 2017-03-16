using System;
using System.Threading.Tasks;

namespace Snes
{
    class SPC7110RAM : Memory
    {
        public static SPC7110RAM spc7110ram = new SPC7110RAM();

        public override uint size() { throw new NotImplementedException(); }
        public override Task<byte> read(uint addr) { throw new NotImplementedException(); }
        public override Task write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
