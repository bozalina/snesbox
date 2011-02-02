﻿
namespace Snes
{
    class MMIOAccess : Memory
    {
        public static MMIOAccess mmio = new MMIOAccess();

        public IMMIO handle(uint addr)
        {
            return _mmio[addr & 0x7fff];
        }

        public void map(uint addr, IMMIO access)
        {
            _mmio[addr & 0x7fff] = access;
        }

        public override byte read(uint addr)
        {
            return _mmio[addr & 0x7fff].mmio_read(addr);
        }

        public override void write(uint addr, byte data)
        {
            _mmio[addr & 0x7fff].mmio_write(addr, data);
        }

        public MMIOAccess()
        {
            for (uint i = 0; i < 0x8000; i++)
            {
                _mmio[i] = UnmappedMMIO.mmio_unmapped;
            }
        }

        private IMMIO[] _mmio = new IMMIO[0x8000];
    }
}
