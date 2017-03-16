
using System.Threading.Tasks;

namespace Snes
{
    class StaticRAM : Memory
    {
        public static StaticRAM wram = new StaticRAM(128 * 1024);
        public static StaticRAM apuram = new StaticRAM(64 * 1024);
        public static StaticRAM vram = new StaticRAM(64 * 1024);
        public static StaticRAM oam = new StaticRAM(544);
        public static StaticRAM cgram = new StaticRAM(512);

        public byte[] data()
        {
            return data_;
        }

        public override uint size()
        {
            return size_;
        }

        public override Task<byte> read(uint addr)
        {
            return Task.FromResult(data_[addr]);
        }

        public override Task write(uint addr, byte n)
        {
            data_[addr] = n;
			return Task.FromResult(false);
		}

		public byte this[uint addr]
        {
            get
            {
                return data_[addr];
            }
            set
            {
                data_[addr] = value;
            }
        }

        public StaticRAM(uint n)
        {
            size_ = n;
            data_ = new byte[size_];
        }

        private byte[] data_;
        private uint size_;
    }
}
