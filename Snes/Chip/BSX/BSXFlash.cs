using System;
using System.Threading.Tasks;

namespace Snes
{
    partial class BSXFlash : Memory
    {
        public static BSXFlash bsxflash = new BSXFlash();

        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public override uint size() { throw new NotImplementedException(); }
        public override Task<byte> read(uint addr) { throw new NotImplementedException(); }
        public override Task write(uint addr, byte data) { throw new NotImplementedException(); }

        private Regs regs;
    }
}
