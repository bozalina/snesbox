using System;
using System.Threading.Tasks;

namespace Snes
{
    class CC1BWRAM : Memory
    {
        public static CC1BWRAM cc1bwram = new CC1BWRAM();

        public override uint size() { throw new NotImplementedException(); }
        public override Task<byte> read(uint addr) { throw new NotImplementedException(); }
        public override Task write(uint addr, byte data) { throw new NotImplementedException(); }
        public bool dma;
    }
}
