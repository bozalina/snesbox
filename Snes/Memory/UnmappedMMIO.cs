﻿using System;

namespace Snes
{
    class UnmappedMMIO : IMMIO
    {
        public byte mmio_read(uint addr) { throw new NotImplementedException(); }
        public void mmio_write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
