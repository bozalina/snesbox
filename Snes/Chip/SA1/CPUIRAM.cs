using System;
using System.Threading.Tasks;

namespace Snes
{
    class CPUIRAM : Memory
    {
        public static CPUIRAM cpuiram = new CPUIRAM();

        public override uint size() { throw new NotImplementedException(); }
        public override Task<byte> read(uint addr) { throw new NotImplementedException(); }
        public override Task write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
