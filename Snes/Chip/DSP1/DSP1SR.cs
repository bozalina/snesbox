﻿using System;

namespace Snes.Chip.DSP1
{
    class DSP1SR : Memory.Memory
    {
        public static DSP1SR dsp1sr = new DSP1SR();

        public override uint size() { throw new NotImplementedException(); }
        public override byte read(uint addr) { throw new NotImplementedException(); }
        public override void write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
