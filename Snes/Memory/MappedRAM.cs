using System;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    class MappedRAM : Memory
    {
        public static MappedRAM cartrom = new MappedRAM();
        public static MappedRAM cartram = new MappedRAM();
        public static MappedRAM cartrtc = new MappedRAM();
        public static MappedRAM bsxflash = new MappedRAM();
        public static MappedRAM bsxram = new MappedRAM();
        public static MappedRAM bsxpram = new MappedRAM();
        public static MappedRAM stArom = new MappedRAM();
        public static MappedRAM stAram = new MappedRAM();
        public static MappedRAM stBrom = new MappedRAM();
        public static MappedRAM stBram = new MappedRAM();
        public static MappedRAM gbrom = new MappedRAM();
        public static MappedRAM gbram = new MappedRAM();
        public static MappedRAM gbrtc = new MappedRAM();

        public void reset()
        {
            if (!ReferenceEquals(data_, null))
            {
                data_ = null;
            }

            size_ = Bit.ToUint32(-1);
            write_protect_ = false;
        }

        public void map(byte[] source, uint length)
        {
            reset();
            data_ = source;
            size_ = !ReferenceEquals(data_, null) && length > 0 ? length : Bit.ToUint32(-1);
        }

        public void copy(byte[] data, uint size)
        {
            if (ReferenceEquals(data_, null))
            {
                size_ = (uint)((size & ~255) + (Convert.ToInt32(Convert.ToBoolean(size & 255)) << 8));
                data_ = new byte[size_];
            }
            Array.Copy(data, data_, (int)Math.Min(size_, size));
        }

        public void write_protect(bool status)
        {
            write_protect_ = status;
        }

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
            if (!write_protect_)
            {
                data_[addr] = n;
            }
			return Task.FromResult(false);
		}

		public byte this[uint addr]
        {
            get
            {
                return data_[addr];
            }
        }

        public MappedRAM()
        {
            data_ = null;
            size_ = Bit.ToUint32(-1);
            write_protect_ = false;
        }

        private byte[] data_;
        private uint size_;
        private bool write_protect_;
    }
}
