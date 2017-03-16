using System;
using System.Threading.Tasks;

namespace Snes
{
    class SA1BWRAM : Memory
    {
        public static SA1BWRAM sa1bwram = new SA1BWRAM();

        public override uint size() { throw new NotImplementedException(); }
        public override Task<byte> read(uint addr) { throw new NotImplementedException(); }
        public override Task write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
