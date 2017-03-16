
using System.Threading.Tasks;

namespace Snes
{
    interface IMMIO
    {
		Task<byte> mmio_read(uint addr);
		Task mmio_write(uint addr, byte data);
    }
}
