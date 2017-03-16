using System;
using System.Threading.Tasks;

namespace Snes
{
    partial class BSXCart : IMMIO
    {
        public static BSXCart bsxcart = new BSXCart();

        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public Task<byte> mmio_read(uint addr) { throw new NotImplementedException(); }
        public Task mmio_write(uint addr, byte data) { throw new NotImplementedException(); }

        public BSXCart() { /*throw new NotImplementedException();*/ }

        private Regs regs;

        private void update_memory_map() { throw new NotImplementedException(); }
    }
}
