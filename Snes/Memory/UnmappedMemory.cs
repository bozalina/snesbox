
using System.Threading.Tasks;

namespace Snes
{
    class UnmappedMemory : Memory
    {
        public static UnmappedMemory memory_unmapped = new UnmappedMemory();

        public override uint size()
        {
            return 16 * 1024 * 1024;
        }

        public override Task<byte> read(uint addr)
        {
            return Task.FromResult(CPU.cpu.regs.mdr);
        }

        public override Task write(uint addr, byte data)
        {
	        return Task.FromResult(false);
        }
    }
}
