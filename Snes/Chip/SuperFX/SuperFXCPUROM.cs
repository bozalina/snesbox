using System;
using System.Threading.Tasks;

namespace Snes
{
    class SuperFXCPUROM : Memory
    {
        public static SuperFXCPUROM fxrom = new SuperFXCPUROM();

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
