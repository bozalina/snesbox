using System;
using System.Threading.Tasks;

namespace Snes
{
    class SuperFXCPURAM : Memory
    {
        public static SuperFXCPURAM fxram = new SuperFXCPURAM();

        public override uint size()
        {
            throw new NotImplementedException();
        }

        public override Task<byte> read(uint addr)
        {
            throw new NotImplementedException();
        }

        public override Task write(uint addr, byte data)
        {
            throw new NotImplementedException();
        }
    }
}
