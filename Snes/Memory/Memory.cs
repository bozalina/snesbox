
using System.Threading.Tasks;

namespace Snes
{
    abstract class Memory
    {
        public virtual uint size()
        {
            return 0;
        }

        public abstract Task<byte> read(uint addr);
        public abstract Task write(uint addr, byte data);
    }
}
