#if PERFORMANCE
using System;
using System.Threading;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    partial class PPU : IProcessor, IPPUCounter, IMMIO
    {
        public static PPU ppu = new PPU();

        public void step(uint clocks)
        {
            Processor.clock += clocks;
        }

        public async Task synchronize_cpu()
        {
            if (Processor.clock >= 0 && Scheduler.scheduler.sync != Scheduler.SynchronizeMode.All)
            {
                await Libco.Switch(CPU.cpu.Processor.thread);
            }
        }

        public void latch_counters()
        {
            regs.hcounter = CPU.cpu.PPUCounter.hdot();
            regs.vcounter = CPU.cpu.PPUCounter.vcounter();
            regs.counters_latched = true;
        }

        public bool interlace()
        {
            return display.interlace;
        }

        public bool overscan()
        {
            return display.overscan;
        }

        public bool hires()
        {
            return regs.pseudo_hires || regs.bgmode == 5 || regs.bgmode == 6;
        }

        public async Task enter(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Scheduler.scheduler.sync == Scheduler.SynchronizeMode.All)
                {
                    await Scheduler.scheduler.exit(Scheduler.ExitReason.SynchronizeEvent);
                }

                scanline();
                if (PPUCounter.vcounter() < display.height && Convert.ToBoolean(PPUCounter.vcounter()))
                {
                    await add_clocks(512);
                    render_scanline();
                    await add_clocks(PPUCounter.lineclocks() - 512U);
                }
                else
                {
                    await add_clocks(PPUCounter.lineclocks());
                }
            }
        }

        public void power()
        {
            StaticRAM.vram.data().Initialize();
            StaticRAM.oam.data().Initialize();
            StaticRAM.cgram.data().Initialize();
            reset();
        }

        public void reset()
        {
            Processor.create(Enter, System.system.cpu_frequency);
            PPUCounter.reset();
            surface = new ushort[512 * 512];
            mmio_reset();
            display.interlace = false;
            display.overscan = false;
        }

        public void scanline()
        {
            display.width = !hires() ? 256U : 512U;
            display.height = !overscan() ? 225U : 240U;
            if (PPUCounter.vcounter() == 0)
            {
                frame();
            }
            if (PPUCounter.vcounter() == display.height && regs.display_disable == false)
            {
                oam.address_reset();
            }
        }

        public void frame()
        {
            oam.frame();
            System.system.frame();
            display.interlace = regs.interlace;
            display.overscan = regs.overscan;
            display.framecounter = display.frameskip == 0 ? 0 : (display.framecounter + 1) % display.frameskip;
        }

        public void layer_enable(uint layer, uint priority, bool enable)
        {
            switch (layer * 4 + priority)
            {
                case 0:
                    bg1.priority0_enable = enable;
                    break;
                case 1:
                    bg1.priority1_enable = enable;
                    break;
                case 4:
                    bg2.priority0_enable = enable;
                    break;
                case 5:
                    bg2.priority1_enable = enable;
                    break;
                case 8:
                    bg3.priority0_enable = enable;
                    break;
                case 9:
                    bg3.priority1_enable = enable;
                    break;
                case 12:
                    bg4.priority0_enable = enable;
                    break;
                case 13:
                    bg4.priority1_enable = enable;
                    break;
                case 16:
                    oam.priority0_enable = enable;
                    break;
                case 17:
                    oam.priority1_enable = enable;
                    break;
                case 18:
                    oam.priority2_enable = enable;
                    break;
                case 19:
                    oam.priority3_enable = enable;
                    break;
            }
        }

        public void set_frameskip(uint frameskip)
        {
            display.frameskip = frameskip;
            display.framecounter = 0;
        }

        public void serialize(Serializer s)
        {
            Processor.serialize(s);
            PPUCounter.serialize(s);

            cache.serialize(s);
            bg1.serialize(s);
            bg2.serialize(s);
            bg3.serialize(s);
            bg4.serialize(s);
            oam.serialize(s);
            screen.serialize(s);

            s.integer(display.interlace, "display.interlace");
            s.integer(display.overscan, "display.overscan");
            s.integer(display.width, "display.width");
            s.integer(display.height, "display.height");

            s.integer(regs.ppu1_mdr, "regs.ppu1_mdr");
            s.integer(regs.ppu2_mdr, "regs.ppu2_mdr");

            s.integer(regs.vram_readbuffer, "regs.vram_readbuffer");
            s.integer(regs.oam_latchdata, "regs.oam_latchdata");
            s.integer(regs.cgram_latchdata, "regs.cgram_latchdata");
            s.integer(regs.bgofs_latchdata, "regs.bgofs_latchdata");
            s.integer(regs.mode7_latchdata, "regs.mode7_latchdata");

            s.integer(regs.counters_latched, "regs.counters_latched");
            s.integer(regs.latch_hcounter, "regs.latch_hcounter");
            s.integer(regs.latch_vcounter, "regs.latch_vcounter");

            s.integer(regs.display_disable, "regs.display_disable");
            s.integer(regs.display_brightness, "regs.display_brightness");

            s.integer(regs.oam_baseaddr, "regs.oam_baseaddr");
            s.integer(regs.oam_addr, "regs.oam_addr");
            s.integer(regs.oam_priority, "regs.oam_priority");

            s.integer(regs.bg3_priority, "regs.bg3_priority");
            s.integer(regs.bgmode, "regs.bgmode");

            s.integer(regs.mode7_hoffset, "regs.mode7_hoffset");

            s.integer(regs.mode7_voffset, "regs.mode7_voffset");

            s.integer(regs.vram_incmode, "regs.vram_incmode");
            s.integer(regs.vram_mapping, "regs.vram_mapping");
            s.integer(regs.vram_incsize, "regs.vram_incsize");

            s.integer(regs.vram_addr, "regs.vram_addr");

            s.integer(regs.mode7_repeat, "regs.mode7_repeat");
            s.integer(regs.mode7_vflip, "regs.mode7_vflip");
            s.integer(regs.mode7_hflip, "regs.mode7_hflip");

            s.integer(regs.m7a, "regs.m7a");
            s.integer(regs.m7b, "regs.m7b");
            s.integer(regs.m7c, "regs.m7c");
            s.integer(regs.m7d, "regs.m7d");
            s.integer(regs.m7x, "regs.m7x");
            s.integer(regs.m7y, "regs.m7y");

            s.integer(regs.cgram_addr, "regs.cgram_addr");

            s.integer(regs.window_one_left, "regs.window_one_left");
            s.integer(regs.window_one_right, "regs.window_one_right");
            s.integer(regs.window_two_left, "regs.window_two_left");
            s.integer(regs.window_two_right, "regs.window_two_right");

            s.integer(regs.mode7_extbg, "regs.mode7_extbg");
            s.integer(regs.pseudo_hires, "regs.pseudo_hires");
            s.integer(regs.overscan, "regs.overscan");
            s.integer(regs.interlace, "regs.interlace");

            s.integer(regs.hcounter, "regs.hcounter");

            s.integer(regs.vcounter, "regs.vcounter");
        }

        public PPU()
        {
            cache = new Cache(this);
            bg1 = new Background(this, (uint)Background.ID.BG1);
            bg2 = new Background(this, (uint)Background.ID.BG2);
            bg3 = new Background(this, (uint)Background.ID.BG3);
            bg4 = new Background(this, (uint)Background.ID.BG4);
            oam = new Sprite(this);
            screen = new Screen(this);
            surface = new ushort[512 * 512];
            output = new ArraySegment<ushort>(surface, 16 * 512, surface.Length - (16 * 512));
            display.width = 256;
            display.height = 224;
            display.frameskip = 0;
            display.framecounter = 0;
        }

        private ushort[] surface;
        public ArraySegment<ushort> output;
        private Regs regs = new Regs();

        private ushort get_vram_addr()
        {
            ushort addr = regs.vram_addr;
            switch (regs.vram_mapping)
            {
                case 0:
                    break;
                case 1:
                    addr = (ushort)((addr & 0xff00) | ((addr & 0x001f) << 3) | ((addr >> 5) & 7));
                    break;
                case 2:
                    addr = (ushort)((addr & 0xfe00) | ((addr & 0x003f) << 3) | ((addr >> 6) & 7));
                    break;
                case 3:
                    addr = (ushort)((addr & 0xfc00) | ((addr & 0x007f) << 3) | ((addr >> 7) & 7));
                    break;
            }
            return (ushort)(addr << 1);
        }

        private byte vram_read(uint addr)
        {
            if (regs.display_disable)
            {
                return StaticRAM.vram[addr];
            }
            if (CPU.cpu.PPUCounter.vcounter() >= display.height)
            {
                return StaticRAM.vram[addr];
            }
            return 0x00;
        }

        private void vram_write(uint addr, byte data)
        {
            if (regs.display_disable || CPU.cpu.PPUCounter.vcounter() >= display.height)
            {
                StaticRAM.vram[addr] = data;
                cache.tilevalid[0][addr >> 4] = Convert.ToByte(false);
                cache.tilevalid[1][addr >> 5] = Convert.ToByte(false);
                cache.tilevalid[2][addr >> 6] = Convert.ToByte(false);
                return;
            }
        }

        private byte oam_read(uint addr)
        {
            if (Convert.ToBoolean(addr & 0x0200))
            {
                addr &= 0x021f;
            }
            if (regs.display_disable)
            {
                return StaticRAM.oam[addr];
            }
            if (CPU.cpu.PPUCounter.vcounter() >= display.height)
            {
                return StaticRAM.oam[addr];
            }
            return StaticRAM.oam[0x0218];
        }

        private void oam_write(uint addr, byte data)
        {
            if (Convert.ToBoolean(addr & 0x0200))
            {
                addr &= 0x021f;
            }
            if (!regs.display_disable && CPU.cpu.PPUCounter.vcounter() < display.height)
            {
                addr = 0x0218;
            }
            StaticRAM.oam[addr] = data;
            oam.update_list(addr, data);
        }

        private byte cgram_read(uint addr)
        {
            return StaticRAM.cgram[addr];
        }

        private void cgram_write(uint addr, byte data)
        {
            StaticRAM.cgram[addr] = data;
        }

        private void mmio_update_video_mode()
        {
            switch (regs.bgmode)
            {
                case 0:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP2;
                        bg1.regs.priority0 = 8;
                        bg1.regs.priority1 = 11;
                        bg2.regs.mode = (uint)Background.Mode.BPP2;
                        bg2.regs.priority0 = 7;
                        bg2.regs.priority1 = 10;
                        bg3.regs.mode = (uint)Background.Mode.BPP2;
                        bg3.regs.priority0 = 2;
                        bg3.regs.priority1 = 5;
                        bg4.regs.mode = (uint)Background.Mode.BPP2;
                        bg4.regs.priority0 = 1;
                        bg4.regs.priority1 = 4;
                        oam.regs.priority0 = 3;
                        oam.regs.priority1 = 6;
                        oam.regs.priority2 = 9;
                        oam.regs.priority3 = 12;
                    } break;

                case 1:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP4;
                        bg2.regs.mode = (uint)Background.Mode.BPP4;
                        bg3.regs.mode = (uint)Background.Mode.BPP2;
                        bg4.regs.mode = (uint)Background.Mode.Inactive;
                        if (regs.bg3_priority)
                        {
                            bg1.regs.priority0 = 5;
                            bg1.regs.priority1 = 8;
                            bg2.regs.priority0 = 4;
                            bg2.regs.priority1 = 7;
                            bg3.regs.priority0 = 1;
                            bg3.regs.priority1 = 10;
                            oam.regs.priority0 = 2;
                            oam.regs.priority1 = 3;
                            oam.regs.priority2 = 6;
                            oam.regs.priority3 = 9;
                        }
                        else
                        {
                            bg1.regs.priority0 = 6;
                            bg1.regs.priority1 = 9;
                            bg2.regs.priority0 = 5;
                            bg2.regs.priority1 = 8;
                            bg3.regs.priority0 = 1;
                            bg3.regs.priority1 = 3;
                            oam.regs.priority0 = 2;
                            oam.regs.priority1 = 4;
                            oam.regs.priority2 = 7;
                            oam.regs.priority3 = 10;
                        }
                    } break;

                case 2:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP4;
                        bg2.regs.mode = (uint)Background.Mode.BPP4;
                        bg3.regs.mode = (uint)Background.Mode.Inactive;
                        bg4.regs.mode = (uint)Background.Mode.Inactive;
                        bg1.regs.priority0 = 3;
                        bg1.regs.priority1 = 7;
                        bg2.regs.priority0 = 1;
                        bg2.regs.priority1 = 5;
                        oam.regs.priority0 = 2;
                        oam.regs.priority1 = 4;
                        oam.regs.priority2 = 6;
                        oam.regs.priority3 = 8;
                    } break;

                case 3:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP8;
                        bg2.regs.mode = (uint)Background.Mode.BPP4;
                        bg3.regs.mode = (uint)Background.Mode.Inactive;
                        bg4.regs.mode = (uint)Background.Mode.Inactive;
                        bg1.regs.priority0 = 3;
                        bg1.regs.priority1 = 7;
                        bg2.regs.priority0 = 1;
                        bg2.regs.priority1 = 5;
                        oam.regs.priority0 = 2;
                        oam.regs.priority1 = 4;
                        oam.regs.priority2 = 6;
                        oam.regs.priority3 = 8;
                    } break;

                case 4:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP8;
                        bg2.regs.mode = (uint)Background.Mode.BPP2;
                        bg3.regs.mode = (uint)Background.Mode.Inactive;
                        bg4.regs.mode = (uint)Background.Mode.Inactive;
                        bg1.regs.priority0 = 3;
                        bg1.regs.priority1 = 7;
                        bg2.regs.priority0 = 1;
                        bg2.regs.priority1 = 5;
                        oam.regs.priority0 = 2;
                        oam.regs.priority1 = 4;
                        oam.regs.priority2 = 6;
                        oam.regs.priority3 = 8;
                    } break;

                case 5:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP4;
                        bg2.regs.mode = (uint)Background.Mode.BPP2;
                        bg3.regs.mode = (uint)Background.Mode.Inactive;
                        bg4.regs.mode = (uint)Background.Mode.Inactive;
                        bg1.regs.priority0 = 3;
                        bg1.regs.priority1 = 7;
                        bg2.regs.priority0 = 1;
                        bg2.regs.priority1 = 5;
                        oam.regs.priority0 = 2;
                        oam.regs.priority1 = 4;
                        oam.regs.priority2 = 6;
                        oam.regs.priority3 = 8;
                    } break;

                case 6:
                    {
                        bg1.regs.mode = (uint)Background.Mode.BPP4;
                        bg2.regs.mode = (uint)Background.Mode.Inactive;
                        bg3.regs.mode = (uint)Background.Mode.Inactive;
                        bg4.regs.mode = (uint)Background.Mode.Inactive;
                        bg1.regs.priority0 = 2;
                        bg1.regs.priority1 = 5;
                        oam.regs.priority0 = 1;
                        oam.regs.priority1 = 3;
                        oam.regs.priority2 = 4;
                        oam.regs.priority3 = 6;
                    } break;

                case 7:
                    {
                        if (regs.mode7_extbg == false)
                        {
                            bg1.regs.mode = (uint)Background.Mode.Mode7;
                            bg2.regs.mode = (uint)Background.Mode.Inactive;
                            bg3.regs.mode = (uint)Background.Mode.Inactive;
                            bg4.regs.mode = (uint)Background.Mode.Inactive;
                            bg1.regs.priority0 = 2;
                            bg1.regs.priority1 = 2;
                            oam.regs.priority0 = 1;
                            oam.regs.priority1 = 3;
                            oam.regs.priority2 = 4;
                            oam.regs.priority3 = 5;
                        }
                        else
                        {
                            bg1.regs.mode = (uint)Background.Mode.Mode7;
                            bg2.regs.mode = (uint)Background.Mode.Mode7;
                            bg3.regs.mode = (uint)Background.Mode.Inactive;
                            bg4.regs.mode = (uint)Background.Mode.Inactive;
                            bg1.regs.priority0 = 3;
                            bg1.regs.priority1 = 3;
                            bg2.regs.priority0 = 1;
                            bg2.regs.priority1 = 5;
                            oam.regs.priority0 = 2;
                            oam.regs.priority1 = 4;
                            oam.regs.priority2 = 6;
                            oam.regs.priority3 = 7;
                        }
                    } break;
            }
        }

        public async Task<byte> mmio_read(uint addr)
        {
            await CPU.cpu.synchronize_ppu();

            switch (addr & 0xffff)
            {
                case 0x2104:
                case 0x2105:
                case 0x2106:
                case 0x2108:
                case 0x2109:
                case 0x210a:
                case 0x2114:
                case 0x2115:
                case 0x2116:
                case 0x2118:
                case 0x2119:
                case 0x211a:
                case 0x2124:
                case 0x2125:
                case 0x2126:
                case 0x2128:
                case 0x2129:
                case 0x212a:
                    {
                        return regs.ppu1_mdr;
                    }
                case 0x2134:
                    {  //MPYL
                        uint result = (uint)((short)regs.m7a * (sbyte)(regs.m7b >> 8));
                        regs.ppu1_mdr = (byte)(result >> 0);
                        return regs.ppu1_mdr;
                    }
                case 0x2135:
                    {  //MPYM
                        uint result = (uint)((short)regs.m7a * (sbyte)(regs.m7b >> 8));
                        regs.ppu1_mdr = (byte)(result >> 8);
                        return regs.ppu1_mdr;
                    }
                case 0x2136:
                    {  //MPYH
                        uint result = (uint)((short)regs.m7a * (sbyte)(regs.m7b >> 8));
                        regs.ppu1_mdr = (byte)(result >> 16);
                        return regs.ppu1_mdr;
                    }
                case 0x2137:
                    {  //SLHV
                        if (Convert.ToBoolean(CPU.cpu.pio() & 0x80))
                        {
                            latch_counters();
                        }
                        return CPU.cpu.regs.mdr;
                    }
                case 0x2138:
                    {  //OAMDATAREAD
                        regs.ppu1_mdr = oam_read(regs.oam_addr);
                        regs.oam_addr = (ushort)((regs.oam_addr + 1) & 0x03ff);
                        oam.set_first();
                        return regs.ppu1_mdr;
                    }
                case 0x2139:
                    {  //VMDATALREAD
                        regs.ppu1_mdr = (byte)(regs.vram_readbuffer >> 0);
                        if (regs.vram_incmode == Convert.ToBoolean(0))
                        {
                            addr = get_vram_addr();
                            regs.vram_readbuffer = (ushort)(vram_read(addr + 0) << 0);
                            regs.vram_readbuffer |= (ushort)(vram_read(addr + 1) << 8);
                            regs.vram_addr += (ushort)(regs.vram_incsize);
                        }
                        return regs.ppu1_mdr;
                    }
                case 0x213a:
                    {  //VMDATAHREAD
                        regs.ppu1_mdr = (byte)(regs.vram_readbuffer >> 8);
                        if (regs.vram_incmode == Convert.ToBoolean(1))
                        {
                            addr = get_vram_addr();
                            regs.vram_readbuffer = (ushort)(vram_read(addr + 0) << 0);
                            regs.vram_readbuffer |= (ushort)(vram_read(addr + 1) << 8);
                            regs.vram_addr += (ushort)(regs.vram_incsize);
                        }
                        return regs.ppu1_mdr;
                    }
                case 0x213b:
                    {  //CGDATAREAD
                        if ((regs.cgram_addr & 1) == 0)
                        {
                            regs.ppu2_mdr = cgram_read(regs.cgram_addr);
                        }
                        else
                        {
                            regs.ppu2_mdr = (byte)((regs.ppu2_mdr & 0x80) | (cgram_read(regs.cgram_addr) & 0x7f));
                        }
                        regs.cgram_addr = (ushort)((regs.cgram_addr + 1) & 0x01ff);
                        return regs.ppu2_mdr;
                    }
                case 0x213c:
                    {  //OPHCT
                        if (regs.latch_hcounter == Convert.ToBoolean(0))
                        {
                            regs.ppu2_mdr = (byte)(regs.hcounter & 0xff);
                        }
                        else
                        {
                            regs.ppu2_mdr = (byte)((regs.ppu2_mdr & 0xfe) | (regs.hcounter >> 8));
                        }
                        regs.latch_hcounter ^= Convert.ToBoolean(1);
                        return regs.ppu2_mdr;
                    }
                case 0x213d:
                    {  //OPVCT
                        if (regs.latch_vcounter == Convert.ToBoolean(0))
                        {
                            regs.ppu2_mdr = (byte)(regs.vcounter & 0xff);
                        }
                        else
                        {
                            regs.ppu2_mdr = (byte)((regs.ppu2_mdr & 0xfe) | (regs.vcounter >> 8));
                        }
                        regs.latch_vcounter ^= Convert.ToBoolean(1);
                        return regs.ppu2_mdr;
                    }
                case 0x213e:
                    {  //STAT77
                        regs.ppu1_mdr &= 0x10;
                        regs.ppu1_mdr |= (byte)(Convert.ToUInt32(oam.regs.time_over) << 7);
                        regs.ppu1_mdr |= (byte)(Convert.ToUInt32(oam.regs.range_over) << 6);
                        regs.ppu1_mdr |= 0x01;  //version
                        return regs.ppu1_mdr;
                    }
                case 0x213f:
                    {  //STAT78
                        regs.latch_hcounter = Convert.ToBoolean(0);
                        regs.latch_vcounter = Convert.ToBoolean(0);

                        regs.ppu2_mdr &= 0x20;
                        regs.ppu2_mdr |= (byte)(Convert.ToUInt32(CPU.cpu.PPUCounter.field()) << 7);
                        if ((CPU.cpu.pio() & 0x80) == 0)
                        {
                            regs.ppu2_mdr |= 0x40;
                        }
                        else if (regs.counters_latched)
                        {
                            regs.ppu2_mdr |= 0x40;
                            regs.counters_latched = false;
                        }
                        regs.ppu2_mdr |= (byte)((System.system.region == System.Region.NTSC ? 0 : 1) << 4);
                        regs.ppu2_mdr |= 0x03;  //version
                        return regs.ppu2_mdr;
                    }
            }

            return CPU.cpu.regs.mdr;
        }

        public async Task mmio_write(uint addr, byte data)
        {
            await CPU.cpu.synchronize_ppu();

            switch (addr & 0xffff)
            {
                case 0x2100:
                    {  //INIDISP
                        if (regs.display_disable && CPU.cpu.PPUCounter.vcounter() == display.height)
                        {
                            oam.address_reset();
                        }
                        regs.display_disable = Convert.ToBoolean(data & 0x80);
                        regs.display_brightness = (uint)(data & 0x0f);
                        return;
                    }
                case 0x2101:
                    {  //OBSEL
                        oam.regs.base_size = (uint)((data >> 5) & 7);
                        oam.regs.nameselect = (uint)((data >> 3) & 3);
                        oam.regs.tiledata_addr = (uint)((data & 3) << 14);
                        oam.list_valid = false;
                        return;
                    }
                case 0x2102:
                    {  //OAMADDL
                        regs.oam_baseaddr = (ushort)((regs.oam_baseaddr & 0x0100) | (data << 0));
                        oam.address_reset();
                        return;
                    }
                case 0x2103:
                    {  //OAMADDH
                        regs.oam_priority = Convert.ToBoolean(data & 0x80);
                        regs.oam_baseaddr = (ushort)(((data & 1) << 8) | (regs.oam_baseaddr & 0x00ff));
                        oam.address_reset();
                        return;
                    }
                case 0x2104:
                    {  //OAMDATA
                        if ((regs.oam_addr & 1) == 0)
                        {
                            regs.oam_latchdata = data;
                        }
                        if (Convert.ToBoolean(regs.oam_addr & 0x0200))
                        {
                            oam_write(regs.oam_addr, data);
                        }
                        else if ((regs.oam_addr & 1) == 1)
                        {
                            oam_write((uint)((regs.oam_addr & ~1) + 0), regs.oam_latchdata);
                            oam_write((uint)((regs.oam_addr & ~1) + 1), data);
                        }
                        regs.oam_addr = (ushort)((regs.oam_addr + 1) & 0x03ff);
                        oam.set_first();
                        return;
                    }
                case 0x2105:
                    {  //BGMODE
                        bg4.regs.tile_size = Convert.ToBoolean(data & 0x80);
                        bg3.regs.tile_size = Convert.ToBoolean(data & 0x40);
                        bg2.regs.tile_size = Convert.ToBoolean(data & 0x20);
                        bg1.regs.tile_size = Convert.ToBoolean(data & 0x10);
                        regs.bg3_priority = Convert.ToBoolean(data & 0x08);
                        regs.bgmode = (uint)(data & 0x07);
                        mmio_update_video_mode();
                        return;
                    }
                case 0x2106:
                    {  //MOSAIC
                        uint mosaic_size = (uint)((data >> 4) & 15);
                        bg4.regs.mosaic = (Convert.ToBoolean(data & 0x08) ? mosaic_size : 0);
                        bg3.regs.mosaic = (Convert.ToBoolean(data & 0x04) ? mosaic_size : 0);
                        bg2.regs.mosaic = (Convert.ToBoolean(data & 0x02) ? mosaic_size : 0);
                        bg1.regs.mosaic = (Convert.ToBoolean(data & 0x01) ? mosaic_size : 0);
                        return;
                    }
                case 0x2107:
                    {  //BG1SC
                        bg1.regs.screen_addr = (uint)((data & 0x7c) << 9);
                        bg1.regs.screen_size = (uint)(data & 3);
                        return;
                    }
                case 0x2108:
                    {  //BG2SC
                        bg2.regs.screen_addr = (uint)((data & 0x7c) << 9);
                        bg2.regs.screen_size = (uint)(data & 3);
                        return;
                    }
                case 0x2109:
                    {  //BG3SC
                        bg3.regs.screen_addr = (uint)((data & 0x7c) << 9);
                        bg3.regs.screen_size = (uint)(data & 3);
                        return;
                    }
                case 0x210a:
                    {  //BG4SC
                        bg4.regs.screen_addr = (uint)((data & 0x7c) << 9);
                        bg4.regs.screen_size = (uint)(data & 3);
                        return;
                    }
                case 0x210b:
                    {  //BG12NBA
                        bg1.regs.tiledata_addr = (uint)((data & 0x07) << 13);
                        bg2.regs.tiledata_addr = (uint)((data & 0x70) << 9);
                        return;
                    }
                case 0x210c:
                    {  //BG34NBA
                        bg3.regs.tiledata_addr = (uint)((data & 0x07) << 13);
                        bg4.regs.tiledata_addr = (uint)((data & 0x70) << 9);
                        return;
                    }
                case 0x210d:
                    {  //BG1HOFS
                        regs.mode7_hoffset = (ushort)((data << 8) | regs.mode7_latchdata);
                        regs.mode7_latchdata = data;

                        bg1.regs.hoffset = (uint)(data << 8) | (uint)(regs.bgofs_latchdata & ~7) | ((bg1.regs.hoffset >> 8) & 7);
                        regs.bgofs_latchdata = data;
                        return;
                    }
                case 0x210e:
                    {  //BG1VOFS
                        regs.mode7_voffset = (ushort)((data << 8) | regs.mode7_latchdata);
                        regs.mode7_latchdata = data;

                        bg1.regs.voffset = (ushort)((data << 8) | regs.bgofs_latchdata);
                        regs.bgofs_latchdata = data;
                        return;
                    }
                case 0x210f:
                    {  //BG2HOFS
                        bg2.regs.hoffset = (uint)(data << 8) | (uint)(regs.bgofs_latchdata & ~7) | ((bg2.regs.hoffset >> 8) & 7);
                        regs.bgofs_latchdata = data;
                        return;
                    }
                case 0x2110:
                    {  //BG2VOFS
                        bg2.regs.voffset = (uint)((data << 8) | regs.bgofs_latchdata);
                        regs.bgofs_latchdata = data;
                        return;
                    }
                case 0x2111:
                    {  //BG3HOFS
                        bg3.regs.hoffset = (uint)(data << 8) | (uint)(regs.bgofs_latchdata & ~7) | ((bg3.regs.hoffset >> 8) & 7);
                        regs.bgofs_latchdata = data;
                        return;
                    }
                case 0x2112:
                    {  //BG3VOFS
                        bg3.regs.voffset = (uint)((data << 8) | regs.bgofs_latchdata);
                        regs.bgofs_latchdata = data;
                        return;
                    }
                case 0x2113:
                    {  //BG4HOFS
                        bg4.regs.hoffset = (uint)(data << 8) | (uint)(regs.bgofs_latchdata & ~7) | ((bg4.regs.hoffset >> 8) & 7);
                        regs.bgofs_latchdata = data;
                        return;
                    }
                case 0x2114:
                    {  //BG4VOFS
                        bg4.regs.voffset = (uint)((data << 8) | regs.bgofs_latchdata);
                        regs.bgofs_latchdata = data;
                        return;
                    }
                case 0x2115:
                    {  //VMAIN
                        regs.vram_incmode = Convert.ToBoolean(data & 0x80);
                        regs.vram_mapping = (uint)((data >> 2) & 3);
                        switch (data & 3)
                        {
                            case 0:
                                regs.vram_incsize = 1;
                                break;
                            case 1:
                                regs.vram_incsize = 32;
                                break;
                            case 2:
                                regs.vram_incsize = 128;
                                break;
                            case 3:
                                regs.vram_incsize = 128;
                                break;
                        }
                        return;
                    }
                case 0x2116:
                    {  //VMADDL
                        regs.vram_addr = (ushort)((regs.vram_addr & 0xff00) | (data << 0));
                        addr = get_vram_addr();
                        regs.vram_readbuffer = (ushort)(vram_read(addr + 0) << 0);
                        regs.vram_readbuffer |= (ushort)(vram_read(addr + 1) << 8);
                        return;
                    }
                case 0x2117:
                    {  //VMADDH
                        regs.vram_addr = (ushort)((data << 8) | (regs.vram_addr & 0x00ff));
                        addr = get_vram_addr();
                        regs.vram_readbuffer = (ushort)(vram_read(addr + 0) << 0);
                        regs.vram_readbuffer |= (ushort)(vram_read(addr + 1) << 8);
                        return;
                    }
                case 0x2118:
                    {  //VMDATAL
                        vram_write(get_vram_addr() + 0U, data);
                        if (regs.vram_incmode == Convert.ToBoolean(0))
                        {
                            regs.vram_addr += (ushort)regs.vram_incsize;
                        }
                        return;
                    }
                case 0x2119:
                    {  //VMDATAH
                        vram_write(get_vram_addr() + 1U, data);
                        if (regs.vram_incmode == Convert.ToBoolean(1))
                        {
                            regs.vram_addr += (ushort)regs.vram_incsize;
                        }
                        return;
                    }
                case 0x211a:
                    {  //M7SEL
                        regs.mode7_repeat = (uint)((data >> 6) & 3);
                        regs.mode7_vflip = Convert.ToBoolean(data & 0x02);
                        regs.mode7_hflip = Convert.ToBoolean(data & 0x01);
                        return;
                    }
                case 0x211b:
                    {  //M7A
                        regs.m7a = (ushort)((data << 8) | regs.mode7_latchdata);
                        regs.mode7_latchdata = data;
                        return;
                    }
                case 0x211c:
                    {  //M7B
                        regs.m7b = (ushort)((data << 8) | regs.mode7_latchdata);
                        regs.mode7_latchdata = data;
                        return;
                    }
                case 0x211d:
                    {  //M7C
                        regs.m7c = (ushort)((data << 8) | regs.mode7_latchdata);
                        regs.mode7_latchdata = data;
                        return;
                    }
                case 0x211e:
                    {  //M7D
                        regs.m7d = (ushort)((data << 8) | regs.mode7_latchdata);
                        regs.mode7_latchdata = data;
                        return;
                    }
                case 0x211f:
                    {  //M7X
                        regs.m7x = (ushort)((data << 8) | regs.mode7_latchdata);
                        regs.mode7_latchdata = data;
                        return;
                    }
                case 0x2120:
                    {  //M7Y
                        regs.m7y = (ushort)((data << 8) | regs.mode7_latchdata);
                        regs.mode7_latchdata = data;
                        return;
                    }
                case 0x2121:
                    {  //CGADD
                        regs.cgram_addr = (ushort)(data << 1);
                        return;
                    }
                case 0x2122:
                    {  //CGDATA
                        if ((regs.cgram_addr & 1) == 0)
                        {
                            regs.cgram_latchdata = data;
                        }
                        else
                        {
                            cgram_write((uint)((regs.cgram_addr & ~1) + 0), regs.cgram_latchdata);
                            cgram_write((uint)((regs.cgram_addr & ~1) + 1), (byte)(data & 0x7f));
                        }
                        regs.cgram_addr = (ushort)((regs.cgram_addr + 1) & 0x01ff);
                        return;
                    }
                case 0x2123:
                    {  //W12SEL
                        bg2.window.two_enable = Convert.ToBoolean(data & 0x80);
                        bg2.window.two_invert = Convert.ToBoolean(data & 0x40);
                        bg2.window.one_enable = Convert.ToBoolean(data & 0x20);
                        bg2.window.one_invert = Convert.ToBoolean(data & 0x10);
                        bg1.window.two_enable = Convert.ToBoolean(data & 0x08);
                        bg1.window.two_invert = Convert.ToBoolean(data & 0x04);
                        bg1.window.one_enable = Convert.ToBoolean(data & 0x02);
                        bg1.window.one_invert = Convert.ToBoolean(data & 0x01);
                        return;
                    }
                case 0x2124:
                    {  //W34SEL
                        bg4.window.two_enable = Convert.ToBoolean(data & 0x80);
                        bg4.window.two_invert = Convert.ToBoolean(data & 0x40);
                        bg4.window.one_enable = Convert.ToBoolean(data & 0x20);
                        bg4.window.one_invert = Convert.ToBoolean(data & 0x10);
                        bg3.window.two_enable = Convert.ToBoolean(data & 0x08);
                        bg3.window.two_invert = Convert.ToBoolean(data & 0x04);
                        bg3.window.one_enable = Convert.ToBoolean(data & 0x02);
                        bg3.window.one_invert = Convert.ToBoolean(data & 0x01);
                        return;
                    }
                case 0x2125:
                    {  //WOBJSEL
                        screen.window.two_enable = Convert.ToBoolean(data & 0x80);
                        screen.window.two_invert = Convert.ToBoolean(data & 0x40);
                        screen.window.one_enable = Convert.ToBoolean(data & 0x20);
                        screen.window.one_invert = Convert.ToBoolean(data & 0x10);
                        oam.window.two_enable = Convert.ToBoolean(data & 0x08);
                        oam.window.two_invert = Convert.ToBoolean(data & 0x04);
                        oam.window.one_enable = Convert.ToBoolean(data & 0x02);
                        oam.window.one_invert = Convert.ToBoolean(data & 0x01);
                        return;
                    }
                case 0x2126:
                    {  //WH0
                        regs.window_one_left = data;
                        return;
                    }
                case 0x2127:
                    {  //WH1
                        regs.window_one_right = data;
                        return;
                    }
                case 0x2128:
                    {  //WH2
                        regs.window_two_left = data;
                        return;
                    }
                case 0x2129:
                    {  //WH3
                        regs.window_two_right = data;
                        return;
                    }
                case 0x212a:
                    {  //WBGLOG
                        bg4.window.mask = (uint)((data >> 6) & 3);
                        bg3.window.mask = (uint)((data >> 4) & 3);
                        bg2.window.mask = (uint)((data >> 2) & 3);
                        bg1.window.mask = (uint)((data >> 0) & 3);
                        return;
                    }
                case 0x212b:
                    {  //WOBJLOG
                        screen.window.mask = (uint)((data >> 2) & 3);
                        oam.window.mask = (uint)((data >> 0) & 3);
                        return;
                    }
                case 0x212c:
                    {  //TM
                        oam.regs.main_enable = Convert.ToBoolean(data & 0x10);
                        bg4.regs.main_enable = Convert.ToBoolean(data & 0x08);
                        bg3.regs.main_enable = Convert.ToBoolean(data & 0x04);
                        bg2.regs.main_enable = Convert.ToBoolean(data & 0x02);
                        bg1.regs.main_enable = Convert.ToBoolean(data & 0x01);
                        return;
                    }
                case 0x212d:
                    {  //TS
                        oam.regs.sub_enable = Convert.ToBoolean(data & 0x10);
                        bg4.regs.sub_enable = Convert.ToBoolean(data & 0x08);
                        bg3.regs.sub_enable = Convert.ToBoolean(data & 0x04);
                        bg2.regs.sub_enable = Convert.ToBoolean(data & 0x02);
                        bg1.regs.sub_enable = Convert.ToBoolean(data & 0x01);
                        return;
                    }
                case 0x212e:
                    {  //TMW
                        oam.window.main_enable = Convert.ToBoolean(data & 0x10);
                        bg4.window.main_enable = Convert.ToBoolean(data & 0x08);
                        bg3.window.main_enable = Convert.ToBoolean(data & 0x04);
                        bg2.window.main_enable = Convert.ToBoolean(data & 0x02);
                        bg1.window.main_enable = Convert.ToBoolean(data & 0x01);
                        return;
                    }
                case 0x212f:
                    {  //TSW
                        oam.window.sub_enable = Convert.ToBoolean(data & 0x10);
                        bg4.window.sub_enable = Convert.ToBoolean(data & 0x08);
                        bg3.window.sub_enable = Convert.ToBoolean(data & 0x04);
                        bg2.window.sub_enable = Convert.ToBoolean(data & 0x02);
                        bg1.window.sub_enable = Convert.ToBoolean(data & 0x01);
                        return;
                    }
                case 0x2130:
                    {  //CGWSEL
                        screen.window.main_mask = (uint)((data >> 6) & 3);
                        screen.window.sub_mask = (uint)((data >> 4) & 3);
                        screen.regs.addsub_mode = Convert.ToBoolean(data & 0x02);
                        screen.regs.direct_color = Convert.ToBoolean(data & 0x01);
                        return;
                    }
                case 0x2131:
                    {  //CGADDSUB
                        screen.regs.color_mode = Convert.ToBoolean(data & 0x80);
                        screen.regs.color_halve = Convert.ToBoolean(data & 0x40);
                        screen.regs.color_enable[6] = Convert.ToBoolean(data & 0x20);
                        screen.regs.color_enable[5] = Convert.ToBoolean(data & 0x10);
                        screen.regs.color_enable[4] = Convert.ToBoolean(data & 0x10);
                        screen.regs.color_enable[3] = Convert.ToBoolean(data & 0x08);
                        screen.regs.color_enable[2] = Convert.ToBoolean(data & 0x04);
                        screen.regs.color_enable[1] = Convert.ToBoolean(data & 0x02);
                        screen.regs.color_enable[0] = Convert.ToBoolean(data & 0x01);
                        return;
                    }
                case 0x2132:
                    {  //COLDATA
                        if (Convert.ToBoolean(data & 0x80))
                        {
                            screen.regs.color_b = (uint)(data & 0x1f);
                        }
                        if (Convert.ToBoolean(data & 0x40))
                        {
                            screen.regs.color_g = (uint)(data & 0x1f);
                        }
                        if (Convert.ToBoolean(data & 0x20))
                        {
                            screen.regs.color_r = (uint)(data & 0x1f);
                        }
                        screen.regs.color = (screen.regs.color_b << 10) | (screen.regs.color_g << 5) | (screen.regs.color_r << 0);
                        return;
                    }
                case 0x2133:
                    {  //SETINI
                        regs.mode7_extbg = Convert.ToBoolean(data & 0x40);
                        regs.pseudo_hires = Convert.ToBoolean(data & 0x08);
                        regs.overscan = Convert.ToBoolean(data & 0x04);
                        oam.regs.interlace = Convert.ToBoolean(data & 0x02);
                        regs.interlace = Convert.ToBoolean(data & 0x01);
                        mmio_update_video_mode();
                        oam.list_valid = false;
                        return;
                    }
            }
        }

        private void mmio_reset()
        {   //internal
            regs.ppu1_mdr = 0;
            regs.ppu2_mdr = 0;

            regs.vram_readbuffer = 0;
            regs.oam_latchdata = 0;
            regs.cgram_latchdata = 0;
            regs.bgofs_latchdata = 0;
            regs.mode7_latchdata = 0;

            regs.counters_latched = Convert.ToBoolean(0);
            regs.latch_hcounter = Convert.ToBoolean(0);
            regs.latch_vcounter = Convert.ToBoolean(0);

            oam.regs.first_sprite = 0;
            oam.list_valid = false;

            //$2100
            regs.display_disable = true;
            regs.display_brightness = 0;

            //$2101
            oam.regs.base_size = 0;
            oam.regs.nameselect = 0;
            oam.regs.tiledata_addr = 0;

            //$2102-$2103
            regs.oam_baseaddr = 0;
            regs.oam_addr = 0;
            regs.oam_priority = Convert.ToBoolean(0);

            //$2105
            bg4.regs.tile_size = Convert.ToBoolean(0);
            bg3.regs.tile_size = Convert.ToBoolean(0);
            bg2.regs.tile_size = Convert.ToBoolean(0);
            bg1.regs.tile_size = Convert.ToBoolean(0);
            regs.bg3_priority = Convert.ToBoolean(0);
            regs.bgmode = 0;

            //$2106
            bg4.regs.mosaic = 0;
            bg3.regs.mosaic = 0;
            bg2.regs.mosaic = 0;
            bg1.regs.mosaic = 0;

            //$2107-$210a
            bg1.regs.screen_addr = 0;
            bg1.regs.screen_size = 0;
            bg2.regs.screen_addr = 0;
            bg2.regs.screen_size = 0;
            bg3.regs.screen_addr = 0;
            bg3.regs.screen_size = 0;
            bg4.regs.screen_addr = 0;
            bg4.regs.screen_size = 0;

            //$210b-$210c
            bg1.regs.tiledata_addr = 0;
            bg2.regs.tiledata_addr = 0;
            bg3.regs.tiledata_addr = 0;
            bg4.regs.tiledata_addr = 0;

            //$210d-$2114
            regs.mode7_hoffset = 0;
            regs.mode7_voffset = 0;
            bg1.regs.hoffset = 0;
            bg1.regs.voffset = 0;
            bg2.regs.hoffset = 0;
            bg2.regs.voffset = 0;
            bg3.regs.hoffset = 0;
            bg3.regs.voffset = 0;
            bg4.regs.hoffset = 0;
            bg4.regs.voffset = 0;

            //$2115
            regs.vram_incmode = Convert.ToBoolean(0);
            regs.vram_mapping = 0;
            regs.vram_incsize = 1;

            //$2116-$2117
            regs.vram_addr = 0;

            //$211a
            regs.mode7_repeat = 0;
            regs.mode7_vflip = Convert.ToBoolean(0);
            regs.mode7_hflip = Convert.ToBoolean(0);

            //$211b-$2120
            regs.m7a = 0;
            regs.m7b = 0;
            regs.m7c = 0;
            regs.m7d = 0;
            regs.m7x = 0;
            regs.m7y = 0;

            //$2121
            regs.cgram_addr = 0;

            //$2123-$2125
            bg1.window.one_enable = Convert.ToBoolean(0);
            bg1.window.one_invert = Convert.ToBoolean(0);
            bg1.window.two_enable = Convert.ToBoolean(0);
            bg1.window.two_invert = Convert.ToBoolean(0);

            bg2.window.one_enable = Convert.ToBoolean(0);
            bg2.window.one_invert = Convert.ToBoolean(0);
            bg2.window.two_enable = Convert.ToBoolean(0);
            bg2.window.two_invert = Convert.ToBoolean(0);

            bg3.window.one_enable = Convert.ToBoolean(0);
            bg3.window.one_invert = Convert.ToBoolean(0);
            bg3.window.two_enable = Convert.ToBoolean(0);
            bg3.window.two_invert = Convert.ToBoolean(0);

            bg4.window.one_enable = Convert.ToBoolean(0);
            bg4.window.one_invert = Convert.ToBoolean(0);
            bg4.window.two_enable = Convert.ToBoolean(0);
            bg4.window.two_invert = Convert.ToBoolean(0);

            oam.window.one_enable = Convert.ToBoolean(0);
            oam.window.one_invert = Convert.ToBoolean(0);
            oam.window.two_enable = Convert.ToBoolean(0);
            oam.window.two_invert = Convert.ToBoolean(0);

            screen.window.one_enable = Convert.ToBoolean(0);
            screen.window.one_invert = Convert.ToBoolean(0);
            screen.window.two_enable = Convert.ToBoolean(0);
            screen.window.two_invert = Convert.ToBoolean(0);

            //$2126-$2129
            regs.window_one_left = 0;
            regs.window_one_right = 0;
            regs.window_two_left = 0;
            regs.window_two_right = 0;

            //$212a-$212b
            bg1.window.mask = 0;
            bg2.window.mask = 0;
            bg3.window.mask = 0;
            bg4.window.mask = 0;
            oam.window.mask = 0;
            screen.window.mask = 0;

            //$212c
            bg1.regs.main_enable = Convert.ToBoolean(0);
            bg2.regs.main_enable = Convert.ToBoolean(0);
            bg3.regs.main_enable = Convert.ToBoolean(0);
            bg4.regs.main_enable = Convert.ToBoolean(0);
            oam.regs.main_enable = Convert.ToBoolean(0);

            //$212d
            bg1.regs.sub_enable = Convert.ToBoolean(0);
            bg2.regs.sub_enable = Convert.ToBoolean(0);
            bg3.regs.sub_enable = Convert.ToBoolean(0);
            bg4.regs.sub_enable = Convert.ToBoolean(0);
            oam.regs.sub_enable = Convert.ToBoolean(0);

            //$212e
            bg1.window.main_enable = Convert.ToBoolean(0);
            bg2.window.main_enable = Convert.ToBoolean(0);
            bg3.window.main_enable = Convert.ToBoolean(0);
            bg4.window.main_enable = Convert.ToBoolean(0);
            oam.window.main_enable = Convert.ToBoolean(0);

            //$212f
            bg1.window.sub_enable = Convert.ToBoolean(0);
            bg2.window.sub_enable = Convert.ToBoolean(0);
            bg3.window.sub_enable = Convert.ToBoolean(0);
            bg4.window.sub_enable = Convert.ToBoolean(0);
            oam.window.sub_enable = Convert.ToBoolean(0);

            //$2130
            screen.window.main_mask = 0;
            screen.window.sub_mask = 0;
            screen.regs.addsub_mode = Convert.ToBoolean(0);
            screen.regs.direct_color = Convert.ToBoolean(0);

            //$2131
            screen.regs.color_mode = Convert.ToBoolean(0);
            screen.regs.color_halve = Convert.ToBoolean(0);
            screen.regs.color_enable[6] = Convert.ToBoolean(0);
            screen.regs.color_enable[5] = Convert.ToBoolean(0);
            screen.regs.color_enable[4] = Convert.ToBoolean(0);
            screen.regs.color_enable[3] = Convert.ToBoolean(0);
            screen.regs.color_enable[2] = Convert.ToBoolean(0);
            screen.regs.color_enable[1] = Convert.ToBoolean(0);
            screen.regs.color_enable[0] = Convert.ToBoolean(0);

            //$2132
            screen.regs.color_b = 0;
            screen.regs.color_g = 0;
            screen.regs.color_r = 0;
            screen.regs.color = 0;

            //$2133
            regs.mode7_extbg = Convert.ToBoolean(0);
            regs.pseudo_hires = Convert.ToBoolean(0);
            regs.overscan = Convert.ToBoolean(0);
            oam.regs.interlace = Convert.ToBoolean(0);
            regs.interlace = Convert.ToBoolean(0);

            //$213e
            oam.regs.time_over = Convert.ToBoolean(0);
            oam.regs.range_over = Convert.ToBoolean(0);

            mmio_update_video_mode();
        }

        private Cache cache;
        private Background bg1;
        private Background bg2;
        private Background bg3;
        private Background bg4;
        private Sprite oam;
        private Screen screen;
        private Display display = new Display();

        private static async Task Enter(PauseToken pauseToken, CancellationToken cancellationToken)
        {
	        await pauseToken.WaitWhilePausedAsync();
            await PPU.ppu.enter(cancellationToken);
        }

        private async Task add_clocks(uint clocks)
        {
            await PPUCounter.tick(clocks);
            step(clocks);
            await synchronize_cpu();
        }

        private void render_scanline()
        {
            if (Convert.ToBoolean(display.framecounter))
            {
                return;  //skip this frame?
            }
            bg1.scanline();
            bg2.scanline();
            bg3.scanline();
            bg4.scanline();
            if (regs.display_disable)
            {
                screen.render_black();
                return;
            }
            screen.scanline();
            bg1.render();
            bg2.render();
            bg3.render();
            bg4.render();
            oam.render();
            screen.render();
        }

        private Processor _processor = new Processor();
        public Processor Processor
        {
            get { return _processor; }
        }

        private PPUCounter _ppuCounter = new PPUCounter();
        public PPUCounter PPUCounter
        {
            get { return _ppuCounter; }
        }
    }
}
#endif