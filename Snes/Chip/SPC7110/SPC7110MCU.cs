using System;
using System.Threading.Tasks;

namespace Snes
{
    class SPC7110MCU : Memory
    {
        public static SPC7110MCU spc7110mcu = new SPC7110MCU();

        public override uint size() { throw new NotImplementedException(); }
        public override Task<byte> read(uint addr) { throw new NotImplementedException(); }
        public override Task write(uint addr, byte data) { throw new NotImplementedException(); }
    }
}
