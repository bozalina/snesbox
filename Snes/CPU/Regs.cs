﻿using Nall;

namespace Snes
{
    partial class CPUCore
    {
        public class Regs
        {
            public Reg24 pc = new Reg24();
            public Reg16[] r = new Reg16[6];
            public Reg16 a, x, y, z, s, d;
            public Flag p = new Flag();
            public byte db;
            public bool e;

            public bool irq; //IRQ pin (0 = low, 1 = trigger)
            public bool wai; //raised during wai, cleared after interrupt triggered
            public byte mdr; //memory data register

            public Regs()
            {
                Utility.InstantiateArrayElements(r);

                a = r[0];
                x = r[1];
                y = r[2];
                z = r[3];
                s = r[4];
                d = r[5];

                db = 0;
                e = false;
                irq = false;
                wai = false;
                mdr = 0;

                z.Assign(0);
            }
        }
    }
}
