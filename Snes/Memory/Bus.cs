using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    partial class Bus
    {
        public static Bus bus = new Bus();

        public uint mirror(uint addr, uint size)
        {
            uint base_ = 0;
            if (Convert.ToBoolean(size))
            {
                uint mask = 1 << 23;
                while (addr >= size)
                {
                    while (!Convert.ToBoolean(addr & mask))
                    {
                        mask >>= 1;
                    }
                    addr -= mask;
                    if (size > mask)
                    {
                        size -= mask;
                        base_ += mask;
                    }
                    mask >>= 1;
                }
                base_ += addr;
            }
            return base_;
        }

        public void map(uint addr, Memory access, uint offset)
        {
            Page p = page[addr >> 8];
            p.access = access;
            p.offset = offset - addr;
        }

        public enum MapMode : uint { Direct, Linear, Shadow }

        public void map(MapMode mode, byte bank_lo, byte bank_hi, ushort addr_lo, ushort addr_hi, Memory access)
        {
            map(mode, bank_lo, bank_hi, addr_lo, addr_hi, access, 0);
        }

        public void map(MapMode mode, byte bank_lo, byte bank_hi, ushort addr_lo, ushort addr_hi, Memory access, uint offset)
        {
            map(mode, bank_lo, bank_hi, addr_lo, addr_hi, access, offset, 0);
        }

        public void map(MapMode mode, byte bank_lo, byte bank_hi, ushort addr_lo, ushort addr_hi, Memory access, uint offset, uint size)
        {
            Debug.Assert(bank_lo <= bank_hi);
            Debug.Assert(addr_lo <= addr_hi);

            if (access.size() == Bit.ToUint32(-1))
            {
                return;
            }

            byte page_lo = (byte)(addr_lo >> 8);
            byte page_hi = (byte)(addr_hi >> 8);
            uint index = 0;

            switch (mode)
            {
                case MapMode.Direct:
                    {
                        for (uint bank = bank_lo; bank <= bank_hi; bank++)
                        {
                            for (uint page = page_lo; page <= page_hi; page++)
                            {
                                map((bank << 16) + (page << 8), access, (bank << 16) + (page << 8));
                            }
                        }
                    }
                    break;
                case MapMode.Linear:
                    {
                        for (uint bank = bank_lo; bank <= bank_hi; bank++)
                        {
                            for (uint page = page_lo; page <= page_hi; page++)
                            {
                                map((bank << 16) + (page << 8), access, mirror(offset + index, access.size()));
                                index += 256;
                                if (Convert.ToBoolean(size))
                                {
                                    index %= size;
                                }
                            }
                        }
                    }
                    break;
                case MapMode.Shadow:
                    {
                        for (uint bank = bank_lo; bank <= bank_hi; bank++)
                        {
                            index += (uint)(page_lo * 256);
                            if (Convert.ToBoolean(size))
                            {
                                index %= size;
                            }

                            for (uint page = page_lo; page <= page_hi; page++)
                            {
                                map((bank << 16) + (page << 8), access, mirror(offset + index, access.size()));
                                index += 256;
                                if (Convert.ToBoolean(size))
                                {
                                    index %= size;
                                }
                            }

                            index += (uint)((255 - page_hi) * 256);
                            if (Convert.ToBoolean(size))
                            {
                                index %= size;
                            }
                        }
                    }
                    break;
            }
        }

        public async Task<byte> read(uint24 addr)
        {
#if CHEAT_SYSTEM
            if (Cheat.cheat.active() && Cheat.cheat.exists((uint)addr))
            {
                byte r;
                if (Cheat.cheat.read((uint)addr, out r))
                {
                    return r;
                }
            }
#endif
            Page p = page[(uint)addr >> 8];
            return await p.access.read(p.offset + (uint)addr);
        }

        public async Task write(uint24 addr, byte data)
        {
            Page p = page[(uint)addr >> 8];
            await p.access.write(p.offset + (uint)addr, data);
        }

        public bool load_cart()
        {
            if (Cartridge.cartridge.loaded == true)
            {
                return false;
            }

            map_reset();
            map_xml();
            map_system();
            return true;
        }

        public void unload_cart() { }

        public void power()
        {
            for (uint i = 0; i < StaticRAM.wram.data().Length; i++)
            {
                StaticRAM.wram[i] = (byte)Configuration.config.cpu.wram_init_value;
            }
        }

        public void reset() { }

        public Page[] page = new Page[65536];

        public void serialize(Serializer s)
        {
            s.array(StaticRAM.wram.data(), StaticRAM.wram.size(), "memory::wram.data()");
            s.array(StaticRAM.apuram.data(), StaticRAM.apuram.size(), "memory::apuram.data()");
            s.array(StaticRAM.vram.data(), StaticRAM.vram.size(), "memory::vram.data()");
            s.array(StaticRAM.oam.data(), StaticRAM.oam.size(), "memory::oam.data()");
            s.array(StaticRAM.cgram.data(), StaticRAM.cgram.size(), "memory::cgram.data()");
        }

        public Bus()
        {
            Utility.InstantiateArrayElements(page);
        }

        private void map_reset()
        {
            map(MapMode.Direct, 0x00, 0xff, 0x0000, 0xffff, UnmappedMemory.memory_unmapped);
            map(MapMode.Direct, 0x00, 0x3f, 0x2000, 0x5fff, MMIOAccess.mmio);
            map(MapMode.Direct, 0x80, 0xbf, 0x2000, 0x5fff, MMIOAccess.mmio);
            for (uint i = 0x2000; i <= 0x5fff; i++)
            {
                MMIOAccess.mmio.map(i, UnmappedMMIO.mmio_unmapped);
            }
        }

        private void map_xml()
        {
            foreach (var m in Cartridge.cartridge.mapping)
            {
                if (!ReferenceEquals(m.memory, null))
                {
                    map(m.mode, (byte)m.banklo, (byte)m.bankhi, (ushort)m.addrlo, (ushort)m.addrhi, m.memory, m.offset, m.size);
                }
                else if (!ReferenceEquals(m.mmio, null))
                {
                    for (uint i = m.addrlo; i <= m.addrhi; i++)
                    {
                        MMIOAccess.mmio.map(i, m.mmio);
                    }
                }
            }
        }

        private void map_system()
        {
            map(MapMode.Linear, 0x00, 0x3f, 0x0000, 0x1fff, StaticRAM.wram, 0x000000, 0x002000);
            map(MapMode.Linear, 0x80, 0xbf, 0x0000, 0x1fff, StaticRAM.wram, 0x000000, 0x002000);
            map(MapMode.Linear, 0x7e, 0x7f, 0x0000, 0xffff, StaticRAM.wram);
        }
    }
}
