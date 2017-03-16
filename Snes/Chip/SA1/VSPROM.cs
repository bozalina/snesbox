using System;
using System.Threading.Tasks;

namespace Snes
{
    class VSPROM : Memory
    {
        public static VSPROM vsprom = new VSPROM();

        public override uint size() { throw new NotImplementedException(); }
        public override Task<byte> read(uint addr) { throw new NotImplementedException(); }
        public override Task write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
