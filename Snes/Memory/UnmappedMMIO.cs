
using System.Threading.Tasks;

namespace Snes
{
    class UnmappedMMIO : IMMIO
    {
        public static UnmappedMMIO mmio_unmapped = new UnmappedMMIO();

        public Task<byte> mmio_read(uint addr)
        {
            return Task.FromResult(CPU.cpu.regs.mdr);
        }

        public Task mmio_write(uint addr, byte data) { return Task.FromResult(false); }
    }
}
