﻿using System;

namespace Snes
{
    partial class BSXBase : IMMIO
    {
        public static BSXBase bsxbase = new BSXBase();

        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { /*throw new NotImplementedException();*/ }
        public void power() { /*throw new NotImplementedException();*/ }
        public void reset() { throw new NotImplementedException(); }

        public byte mmio_read(uint addr) { throw new NotImplementedException(); }
        public void mmio_write(uint addr, byte data) { throw new NotImplementedException(); }

        private Regs regs;
    }
}
