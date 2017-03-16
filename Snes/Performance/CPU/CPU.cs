#if PERFORMANCE
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    partial class CPU : CPUCore, IPPUCounter, IProcessor, IMMIO
    {
        public static CPU cpu = new CPU();

        public Collection<IProcessor> coprocessors = new Collection<IProcessor>();

        public void step(uint clocks)
        {
            SMP.smp.Processor.clock -= (long)(clocks * (ulong)SMP.smp.Processor.frequency);
            PPU.ppu.Processor.clock -= clocks;
            for (uint i = 0; i < coprocessors.Count; i++)
            {
                IProcessor chip = coprocessors[(int)i];
                chip.Processor.clock -= (long)(clocks * (ulong)chip.Processor.frequency);
            }
        }

        public async Task synchronize_smp()
        {
            if (SMP.smp.Processor.clock < 0)
            {
                await Libco.Switch(SMP.smp.Processor.thread);
            }
        }

        public async Task synchronize_ppu()
        {
            if (PPU.ppu.Processor.clock < 0)
            {
                await Libco.Switch(PPU.ppu.Processor.thread);
            }
        }

        public async Task synchronize_coprocessor()
        {
            for (uint i = 0; i < coprocessors.Count; i++)
            {
                IProcessor chip = coprocessors[(int)i];
                if (chip.Processor.clock < 0)
                {
                    await Libco.Switch(chip.Processor.thread);
                }
            }
        }

        public byte pio()
        {
            return status.pio;
        }

        public bool joylatch()
        {
            return status.joypad_strobe_latch;
        }

        public override bool interrupt_pending()
        {
            return false;
        }

        public byte port_read(byte port)
        {
            return port_data[port & 3];
        }

        public void port_write(byte port, byte data)
        {
            port_data[port & 3] = data;
        }

        public async Task<byte> mmio_read(uint addr)
        {
            if ((addr & 0xffc0) == 0x2140)
            {
                await synchronize_smp();
                return SMP.smp.port_read(new uint2(addr & 3));
            }

            switch (addr & 0xffff)
            {
                case 0x2180:
                    {
                        byte result = await Bus.bus.read(new uint24(0x7e0000 | status.wram_addr));
                        status.wram_addr = (status.wram_addr + 1) & 0x01ffff;
                        return result;
                    }
                case 0x4016:
                    {
                        byte result = (byte)(regs.mdr & 0xfc);
                        result |= (byte)(Input.input.port_read(Convert.ToBoolean(0)) & 3);
                        return result;
                    }
                case 0x4017:
                    {
                        byte result = (byte)((regs.mdr & 0xe0) | 0x1c);
                        result |= (byte)(Input.input.port_read(Convert.ToBoolean(1)) & 3);
                        return result;
                    }
                case 0x4210:
                    {
                        byte result = (byte)(regs.mdr & 0x70);
                        result |= (byte)(Convert.ToInt32(status.nmi_line) << 7);
                        result |= 0x02;  //CPU revision
                        status.nmi_line = false;
                        return result;
                    }
                case 0x4211:
                    {
                        byte result = (byte)(regs.mdr & 0x7f);
                        result |= (byte)(Convert.ToInt32(status.irq_line) << 7);
                        status.irq_line = false;
                        return result;
                    }
                case 0x4212:
                    {
                        byte result = (byte)(regs.mdr & 0x3e);
                        uint vbstart = PPU.ppu.overscan() == false ? 225U : 240U;

                        if (PPUCounter.vcounter() >= vbstart && PPUCounter.vcounter() <= vbstart + 2)
                        {
                            result |= 0x01;
                        }
                        if (PPUCounter.hcounter() <= 2 || PPUCounter.hcounter() >= 1096)
                        {
                            result |= 0x40;
                        }
                        if (PPUCounter.vcounter() >= vbstart)
                        {
                            result |= 0x80;
                        }

                        return result;
                    }
                case 0x4213:
                    return status.pio;
                case 0x4214:
                    return (byte)(status.rddiv >> 0);
                case 0x4215:
                    return (byte)(status.rddiv >> 8);
                case 0x4216:
                    return (byte)(status.rdmpy >> 0);
                case 0x4217:
                    return (byte)(status.rdmpy >> 8);
                case 0x4218:
                    return status.joy1l;
                case 0x4219:
                    return status.joy1h;
                case 0x421a:
                    return status.joy2l;
                case 0x421b:
                    return status.joy2h;
                case 0x421c:
                    return status.joy3l;
                case 0x421d:
                    return status.joy3h;
                case 0x421e:
                    return status.joy4l;
                case 0x421f:
                    return status.joy4h;
            }

            if ((addr & 0xff80) == 0x4300)
            {
                uint i = (addr >> 4) & 7;
                switch (addr & 0xff8f)
                {
                    case 0x4300:
                        {
                            return (byte)((Convert.ToInt32(channel[i].direction) << 7)
                                 | (Convert.ToInt32(channel[i].indirect) << 6)
                                 | (Convert.ToInt32(channel[i].unused) << 5)
                                 | (Convert.ToInt32(channel[i].reverse_transfer) << 4)
                                 | (Convert.ToInt32(channel[i].fixed_transfer) << 3)
                                 | (channel[i].transfer_mode << 0));
                        }

                    case 0x4301:
                        return channel[i].dest_addr;
                    case 0x4302:
                        return (byte)(channel[i].source_addr >> 0);
                    case 0x4303:
                        return (byte)(channel[i].source_addr >> 8);
                    case 0x4304:
                        return channel[i].source_bank;
                    case 0x4305:
                        return (byte)(channel[i].union.transfer_size >> 0);
                    case 0x4306:
                        return (byte)(channel[i].union.transfer_size >> 8);
                    case 0x4307:
                        return channel[i].indirect_bank;
                    case 0x4308:
                        return (byte)(channel[i].hdma_addr >> 0);
                    case 0x4309:
                        return (byte)(channel[i].hdma_addr >> 8);
                    case 0x430a:
                        return channel[i].line_counter;
                    case 0x430b:
                    case 0x430f:
                        return channel[i].unknown;
                }
            }

            return regs.mdr;
        }

        public async Task mmio_write(uint addr, byte data)
        {
            if ((addr & 0xffc0) == 0x2140)
            {
                await synchronize_smp();
                port_write((byte)(addr & 3), data);
                return;
            }

            switch (addr & 0xffff)
            {
                case 0x2180:
                    {
                        await Bus.bus.write(new uint24(0x7e0000 | status.wram_addr), data);
                        status.wram_addr = (status.wram_addr + 1) & 0x01ffff;
                        return;
                    }
                case 0x2181:
                    {
                        status.wram_addr = (status.wram_addr & 0x01ff00) | (uint)(data << 0);
                        return;
                    }
                case 0x2182:
                    {
                        status.wram_addr = (status.wram_addr & 0x0100ff) | (uint)(data << 8);
                        return;
                    }
                case 0x2183:
                    {
                        status.wram_addr = (status.wram_addr & 0x00ffff) | (uint)((data & 1) << 16);
                        return;
                    }
                case 0x4016:
                    {
                        bool old_latch = status.joypad_strobe_latch;
                        bool new_latch = Convert.ToBoolean(data & 1);
                        status.joypad_strobe_latch = new_latch;
                        if (old_latch != new_latch)
                        {
                            Input.input.poll();
                        }
                        return;
                    }
                case 0x4200:
                    {
                        bool nmi_enabled = status.nmi_enabled;
                        bool virq_enabled = status.virq_enabled;
                        bool hirq_enabled = status.hirq_enabled;

                        status.nmi_enabled = Convert.ToBoolean(data & 0x80);
                        status.virq_enabled = Convert.ToBoolean(data & 0x20);
                        status.hirq_enabled = Convert.ToBoolean(data & 0x10);
                        status.auto_joypad_poll_enabled = Convert.ToBoolean(data & 0x01);

                        if (!nmi_enabled && status.nmi_enabled && status.nmi_line)
                        {
                            status.nmi_transition = true;
                        }

                        if (status.virq_enabled && !status.hirq_enabled && status.irq_line)
                        {
                            status.irq_transition = true;
                        }

                        if (!status.virq_enabled && !status.hirq_enabled)
                        {
                            status.irq_line = false;
                            status.irq_transition = false;
                        }

                        status.irq_lock = true;
                        return;
                    }
                case 0x4201:
                    {
                        if (Convert.ToBoolean(status.pio & 0x80) && !Convert.ToBoolean(data & 0x80))
                        {
                            PPU.ppu.latch_counters();
                        }
                        status.pio = data;
                        goto case 0x4202;
                    }
                case 0x4202:
                    {
                        status.wrmpya = data;
                        return;
                    }
                case 0x4203:
                    {
                        status.wrmpyb = data;
                        status.rdmpy = (ushort)(status.wrmpya * status.wrmpyb);
                        return;
                    }
                case 0x4204:
                    {
                        status.wrdiva = (ushort)((status.wrdiva & 0xff00) | (data << 0));
                        return;
                    }
                case 0x4205:
                    {
                        status.wrdiva = (ushort)((data << 8) | (status.wrdiva & 0x00ff));
                        return;
                    }
                case 0x4206:
                    {
                        status.wrdivb = data;
                        status.rddiv = (ushort)(Convert.ToBoolean(status.wrdivb) ? status.wrdiva / status.wrdivb : 0xffff);
                        status.rdmpy = (ushort)(Convert.ToBoolean(status.wrdivb) ? status.wrdiva % status.wrdivb : status.wrdiva);
                        return;
                    }
                case 0x4207:
                    {
                        status.htime = (ushort)((status.htime & 0x0100) | (data << 0));
                        return;
                    }
                case 0x4208:
                    {
                        status.htime = (ushort)(((data & 1) << 8) | (status.htime & 0x00ff));
                        return;
                    }
                case 0x4209:
                    {
                        status.vtime = (ushort)((status.vtime & 0x0100) | (data << 0));
                        return;
                    }
                case 0x420a:
                    {
                        status.vtime = (ushort)(((data & 1) << 8) | (status.vtime & 0x00ff));
                        return;
                    }
                case 0x420b:
                    {
                        for (uint i = 0; i < 8; i++)
                        {
                            channel[i].dma_enabled = Convert.ToBoolean(data & (1 << (int)i));
                        }
                        if (Convert.ToBoolean(data))
                        {
                            await dma_run();
                        }
                        return;
                    }
                case 0x420c:
                    {
                        for (uint i = 0; i < 8; i++)
                        {
                            channel[i].hdma_enabled = Convert.ToBoolean(data & (1 << (int)i));
                        }
                        return;
                    }
                case 0x420d:
                    {
                        status.rom_speed = Convert.ToBoolean(data & 1) ? 6U : 8U;
                        return;
                    }
            }

            if ((addr & 0xff80) == 0x4300)
            {
                uint i = (addr >> 4) & 7;
                switch (addr & 0xff8f)
                {
                    case 0x4300:
                        {
                            channel[i].direction = Convert.ToBoolean(data & 0x80);
                            channel[i].indirect = Convert.ToBoolean(data & 0x40);
                            channel[i].unused = Convert.ToBoolean(data & 0x20);
                            channel[i].reverse_transfer = Convert.ToBoolean(data & 0x10);
                            channel[i].fixed_transfer = Convert.ToBoolean(data & 0x08);
                            channel[i].transfer_mode = (byte)(data & 0x07);
                            return;
                        }
                    case 0x4301:
                        {
                            channel[i].dest_addr = data;
                            return;
                        }
                    case 0x4302:
                        {
                            channel[i].source_addr = (ushort)((channel[i].source_addr & 0xff00) | (data << 0));
                            return;
                        }
                    case 0x4303:
                        {
                            channel[i].source_addr = (ushort)((data << 8) | (channel[i].source_addr & 0x00ff));
                            return;
                        }
                    case 0x4304:
                        {
                            channel[i].source_bank = data;
                            return;
                        }
                    case 0x4305:
                        {
                            channel[i].union.transfer_size = (ushort)((channel[i].union.transfer_size & 0xff00) | (data << 0));
                            return;
                        }
                    case 0x4306:
                        {
                            channel[i].union.transfer_size = (ushort)((data << 8) | (channel[i].union.transfer_size & 0x00ff));
                            return;
                        }
                    case 0x4307:
                        {
                            channel[i].indirect_bank = data;
                            return;
                        }
                    case 0x4308:
                        {
                            channel[i].hdma_addr = (ushort)((channel[i].hdma_addr & 0xff00) | (data << 0));
                            return;
                        }
                    case 0x4309:
                        {
                            channel[i].hdma_addr = (ushort)((data << 8) | (channel[i].hdma_addr & 0x00ff));
                            return;
                        }
                    case 0x430a:
                        {
                            channel[i].line_counter = data;
                            return;
                        }
                    case 0x430b:
                    case 0x430f:
                        {
                            channel[i].unknown = data;
                            return;
                        }
                }
            }
        }

        public override async Task op_io()
        {
            await add_clocks(6);
        }

        public override async Task<byte> op_read(uint addr)
        {
            regs.mdr = await Bus.bus.read(new uint24(addr));
            await add_clocks(speed(addr));
            return regs.mdr;
        }

        public override async Task op_write(uint addr, byte data)
        {
            await add_clocks(speed(addr));
            await Bus.bus.write(new uint24(addr), regs.mdr = data);
        }

        public async Task enter(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Scheduler.scheduler.sync == Scheduler.SynchronizeMode.CPU)
                {
                    Scheduler.scheduler.sync = Scheduler.SynchronizeMode.All;
                    await Scheduler.scheduler.exit(Scheduler.ExitReason.SynchronizeEvent);
                }

                if (status.nmi_pending)
                {
                    status.nmi_pending = false;
                    await op_irq((ushort)(regs.e == false ? 0xffea : 0xfffa));
                }

                if (status.irq_pending)
                {
                    status.irq_pending = false;
                    await op_irq((ushort)(regs.e == false ? 0xffee : 0xfffe));
                }

                await op_step();
            }
        }

        public async Task power()
        {
            regs.a.Assign(0x0000);
            regs.x.Assign(0x0000);
            regs.y.Assign(0x0000);
            regs.s.Assign(0x01ff);

            await reset();
        }

        public async Task reset()
        {
            Processor.create(Enter, System.system.cpu_frequency);
            coprocessors.Clear();
            PPUCounter.reset();

            regs.pc.Assign(0x000000);
            regs.x.h = 0x00;
            regs.y.h = 0x00;
            regs.s.h = 0x01;
            regs.d.Assign(0x0000);
            regs.db = 0x00;
            regs.p.Assign(0x34);
            regs.e = Convert.ToBoolean(1);
            regs.mdr = 0x00;
            regs.wai = false;
            update_table();

            regs.pc.l = await Bus.bus.read(new uint24(0xfffc));
            regs.pc.h = await Bus.bus.read(new uint24(0xfffd));
            regs.pc.b = 0x00;

            status.nmi_valid = false;
            status.nmi_line = false;
            status.nmi_transition = false;
            status.nmi_pending = false;

            status.irq_valid = false;
            status.irq_line = false;
            status.irq_transition = false;
            status.irq_pending = false;

            status.irq_lock = false;
            status.hdma_pending = false;

            status.wram_addr = 0x000000;

            status.joypad_strobe_latch = Convert.ToBoolean(0);

            status.nmi_enabled = false;
            status.virq_enabled = false;
            status.hirq_enabled = false;
            status.auto_joypad_poll_enabled = false;

            status.pio = 0xff;

            status.htime = 0x0000;
            status.vtime = 0x0000;

            status.rom_speed = 8;

            status.joy1l = status.joy1h = 0x00;
            status.joy2l = status.joy2h = 0x00;
            status.joy3l = status.joy3h = 0x00;
            status.joy4l = status.joy4h = 0x00;

            dma_reset();
        }

        public void serialize(Serializer s)
        {
            Processor.serialize(s);
            base.core_serialize(s);
            PPUCounter.serialize(s);

            queue.serialize(s);
            s.array(port_data, "port_data");

            for (uint i = 0; i < 8; i++)
            {
                s.integer(channel[i].dma_enabled, "channel[i].dma_enabled");
                s.integer(channel[i].hdma_enabled, "channel[i].hdma_enabled");

                s.integer(channel[i].direction, "channel[i].direction");
                s.integer(channel[i].indirect, "channel[i].indirect");
                s.integer(channel[i].unused, "channel[i].unused");
                s.integer(channel[i].reverse_transfer, "channel[i].reverse_transfer");
                s.integer(channel[i].fixed_transfer, "channel[i].fixed_transfer");
                s.integer(channel[i].transfer_mode, "channel[i].transfer_mode");

                s.integer(channel[i].dest_addr, "channel[i].dest_addr");
                s.integer(channel[i].source_addr, "channel[i].source_addr");
                s.integer(channel[i].source_bank, "channel[i].source_bank");

                s.integer(channel[i].union.transfer_size, "channel[i].transfer_size");

                s.integer(channel[i].indirect_bank, "channel[i].indirect_bank");
                s.integer(channel[i].hdma_addr, "channel[i].hdma_addr");
                s.integer(channel[i].line_counter, "channel[i].line_counter");
                s.integer(channel[i].unknown, "channel[i].unknown");

                s.integer(channel[i].hdma_completed, "channel[i].hdma_completed");
                s.integer(channel[i].hdma_do_transfer, "channel[i].hdma_do_transfer");
            }

            s.integer(status.nmi_valid, "status.nmi_valid");
            s.integer(status.nmi_line, "status.nmi_line");
            s.integer(status.nmi_transition, "status.nmi_transition");
            s.integer(status.nmi_pending, "status.nmi_pending");

            s.integer(status.irq_valid, "status.irq_valid");
            s.integer(status.irq_line, "status.irq_line");
            s.integer(status.irq_transition, "status.irq_transition");
            s.integer(status.irq_pending, "status.irq_pending");

            s.integer(status.irq_lock, "status.irq_lock");
            s.integer(status.hdma_pending, "status.hdma_pending");

            s.integer(status.wram_addr, "status.wram_addr");

            s.integer(status.joypad_strobe_latch, "status.joypad_strobe_latch");

            s.integer(status.nmi_enabled, "status.nmi_enabled");
            s.integer(status.virq_enabled, "status.virq_enabled");
            s.integer(status.hirq_enabled, "status.hirq_enabled");
            s.integer(status.auto_joypad_poll_enabled, "status.auto_joypad_poll_enabled");

            s.integer(status.pio, "status.pio");

            s.integer(status.wrmpya, "status.wrmpya");
            s.integer(status.wrmpyb, "status.wrmpyb");
            s.integer(status.wrdiva, "status.wrdiva");
            s.integer(status.wrdivb, "status.wrdivb");

            s.integer(status.htime, "status.htime");
            s.integer(status.vtime, "status.vtime");

            s.integer(status.rom_speed, "status.rom_speed");

            s.integer(status.rddiv, "status.rddiv");
            s.integer(status.rdmpy, "status.rdmpy");

            s.integer(status.joy1l, "status.joy1l");
            s.integer(status.joy1h, "status.joy1h");
            s.integer(status.joy2l, "status.joy2l");
            s.integer(status.joy2h, "status.joy2h");
            s.integer(status.joy3l, "status.joy3l");
            s.integer(status.joy3h, "status.joy3h");
            s.integer(status.joy4l, "status.joy4l");
            s.integer(status.joy4h, "status.joy4h");
        }

        public CPU()
        {
            Utility.InstantiateArrayElements(channel);

            queue = new PriorityQueue(512, this.queue_event);
            PPUCounter.Scanline = this.scanline;
        }

        //cpu
        private static async Task Enter(PauseToken pauseToken, CancellationToken cancellationToken)
        {
	        await pauseToken.WaitWhilePausedAsync();
            await cpu.enter(cancellationToken);
        }

        private async Task op_step()
        {
            await opcode_table.Array[opcode_table.Offset + await op_readpc()].Invoke();
        }

        private async Task op_irq(ushort vector)
        {
            await op_read(regs.pc.d);
            await op_io();
            if (!regs.e)
            {
                await op_writestack(regs.pc.b);
            }
			await op_writestack(regs.pc.h);
			await op_writestack(regs.pc.l);
			await op_writestack(regs.e ? (byte)((uint)regs.p & ~0x10) : (byte)regs.p);
            rd.l = await op_read(vector + 0U);
            regs.pc.b = 0x00;
            regs.p.i = Convert.ToBoolean(1);
            regs.p.d = Convert.ToBoolean(0);
            rd.h = await op_read(vector + 1U);
            regs.pc.w = rd.w;
        }

        //timing
        private enum QueueEvent : uint { DramRefresh, HdmaRun, ControllerLatch, }

        private PriorityQueue queue;

        private async Task queue_event(uint id)
        {
            switch ((QueueEvent)id)
            {
                case QueueEvent.DramRefresh:
                    {
                        await add_clocks(40);
                        return;
                    }
                case QueueEvent.HdmaRun:
                    {
                        await hdma_run();
                        return;
                    }
                case QueueEvent.ControllerLatch:
                    {
                        PPU.ppu.latch_counters();
                        return;
                    }
            }
        }

        public override void last_cycle()
        {
            if (status.irq_lock)
            {
                status.irq_lock = false;
                return;
            }

            if (status.nmi_transition)
            {
                regs.wai = false;
                status.nmi_transition = false;
                status.nmi_pending = true;
            }

            if (status.irq_transition || regs.irq)
            {
                regs.wai = false;
                status.irq_transition = false;
                status.irq_pending = !regs.p.i;
            }
        }

        private async Task add_clocks(uint clocks)
        {
            if (status.hirq_enabled)
            {
                if (status.virq_enabled)
                {
                    uint cpu_time = PPUCounter.vcounter() * 1364U + PPUCounter.hcounter();
                    uint irq_time = status.vtime * 1364U + status.htime * 4U;
                    uint framelines = (System.system.region == System.Region.NTSC ? 262U : 312U) + Convert.ToUInt32(PPUCounter.field());
                    if (cpu_time > irq_time)
                    {
                        irq_time += framelines * 1364;
                    }
                    bool irq_valid = status.irq_valid;
                    status.irq_valid = cpu_time <= irq_time && cpu_time + clocks > irq_time;
                    if (!irq_valid && status.irq_valid)
                    {
                        status.irq_line = true;
                    }
                }
                else
                {
                    uint irq_time = status.htime * 4U;
                    if (PPUCounter.hcounter() > irq_time)
                    {
                        irq_time += 1364;
                    }
                    bool irq_valid = status.irq_valid;
                    status.irq_valid = PPUCounter.hcounter() <= irq_time && PPUCounter.hcounter() + clocks > irq_time;
                    if (!irq_valid && status.irq_valid)
                    {
                        status.irq_line = true;
                    }
                }
                if (status.irq_line)
                {
                    status.irq_transition = true;
                }
            }
            else if (status.virq_enabled)
            {
                bool irq_valid = status.irq_valid;
                status.irq_valid = PPUCounter.vcounter() == status.vtime;
                if (!irq_valid && status.irq_valid)
                {
                    status.irq_line = true;
                }
                if (status.irq_line)
                {
                    status.irq_transition = true;
                }
            }
            else
            {
                status.irq_valid = false;
            }

            await PPUCounter.tick(clocks);
            await queue.tick(clocks);
            step(clocks);
        }

        private async Task scanline()
        {
            await synchronize_smp();
            await synchronize_ppu();
            await synchronize_coprocessor();
            await System.system.scanline();

            if (PPUCounter.vcounter() == 0)
            {
                await hdma_init();
            }

            queue.enqueue(534, (uint)QueueEvent.DramRefresh);

            if (PPUCounter.vcounter() <= (PPU.ppu.overscan() == false ? 224 : 239))
            {
                queue.enqueue(1104 + 8, (uint)QueueEvent.HdmaRun);
            }

            if (PPUCounter.vcounter() == Input.input.latchy)
            {
                queue.enqueue((uint)Input.input.latchx, (uint)QueueEvent.ControllerLatch);
            }

            bool nmi_valid = status.nmi_valid;
            status.nmi_valid = PPUCounter.vcounter() >= (PPU.ppu.overscan() == false ? 225 : 240);
            if (!nmi_valid && status.nmi_valid)
            {
                status.nmi_line = true;
                if (status.nmi_enabled)
                {
                    status.nmi_transition = true;
                }
            }
            else if (nmi_valid && !status.nmi_valid)
            {
                status.nmi_line = false;
            }

            if (status.auto_joypad_poll_enabled && PPUCounter.vcounter() == (PPU.ppu.overscan() == false ? 227 : 242))
            {
                Input.input.poll();
                run_auto_joypad_poll();
            }
        }

        private void run_auto_joypad_poll()
        {
            ushort joy1 = 0, joy2 = 0, joy3 = 0, joy4 = 0;
            for (uint i = 0; i < 16; i++)
            {
                byte port0 = Input.input.port_read(Convert.ToBoolean(0));
                byte port1 = Input.input.port_read(Convert.ToBoolean(1));

                joy1 |= (ushort)(Convert.ToBoolean(port0 & 1) ? (0x8000 >> (int)i) : 0);
                joy2 |= (ushort)(Convert.ToBoolean(port1 & 1) ? (0x8000 >> (int)i) : 0);
                joy3 |= (ushort)(Convert.ToBoolean(port0 & 2) ? (0x8000 >> (int)i) : 0);
                joy4 |= (ushort)(Convert.ToBoolean(port1 & 2) ? (0x8000 >> (int)i) : 0);
            }

            status.joy1l = (byte)joy1;
            status.joy1h = (byte)(joy1 >> 8);

            status.joy2l = (byte)joy2;
            status.joy2h = (byte)(joy2 >> 8);

            status.joy3l = (byte)joy3;
            status.joy3h = (byte)(joy3 >> 8);

            status.joy4l = (byte)joy4;
            status.joy4h = (byte)(joy4 >> 8);
        }

        //memory
        private uint speed(uint addr)
        {
            if (Convert.ToBoolean(addr & 0x408000))
            {
                if (Convert.ToBoolean(addr & 0x800000))
                {
                    return status.rom_speed;
                }
                return 8;
            }
            if (Convert.ToBoolean((addr + 0x6000) & 0x4000))
            {
                return 8;
            }
            if (Convert.ToBoolean((addr - 0x4000) & 0x7e00))
            {
                return 6;
            }
            return 12;
        }

        //dma
        private bool dma_transfer_valid(byte bbus, uint abus)
        {   //transfers from WRAM to WRAM are invalid; chip only has one address bus
            if (bbus == 0x80 && ((abus & 0xfe0000) == 0x7e0000 || (abus & 0x40e000) == 0x0000))
            {
                return false;
            }
            return true;
        }

        private bool dma_addr_valid(uint abus)
        {   //A-bus access to B-bus or S-CPU registers are invalid
            if ((abus & 0x40ff00) == 0x2100)
            {
                return false;  //$[00-3f|80-bf]:[2100-21ff]
            }
            if ((abus & 0x40fe00) == 0x4000)
            {
                return false;  //$[00-3f|80-bf]:[4000-41ff]
            }
            if ((abus & 0x40ffe0) == 0x4200)
            {
                return false;  //$[00-3f|80-bf]:[4200-421f]
            }
            if ((abus & 0x40ff80) == 0x4300)
            {
                return false;  //$[00-3f|80-bf]:[4300-437f]
            }
            return true;
        }

        private async Task<byte> dma_read(uint abus)
        {
            if (dma_addr_valid(abus) == false)
            {
                return 0x00;
            }
            return await Bus.bus.read(new uint24(abus));
        }

        private async Task dma_write(bool valid, uint addr, byte data)
        {
            if (valid)
            {
                await Bus.bus.write(new uint24(addr), data);
            }
        }

        private async Task dma_transfer(bool direction, byte bbus, uint abus)
        {
            if (Convert.ToInt32(direction) == 0)
            {
                byte data = await dma_read(abus);
                await add_clocks(8);
                await dma_write(dma_transfer_valid(bbus, abus), (uint)(0x2100 | bbus), data);
            }
            else
            {
                byte data = dma_transfer_valid(bbus, abus) ? await Bus.bus.read(new uint24((uint)(0x2100 | bbus))) : (byte)0x00;
                await add_clocks(8);
                await dma_write(dma_addr_valid(abus), abus, data);
            }
        }

        private byte dma_bbus(uint i, uint index)
        {
            switch (channel[i].transfer_mode)
            {
                default:
                case 0:
                    return (channel[i].dest_addr);                       //0
                case 1:
                    return (byte)(channel[i].dest_addr + (index & 1));         //0,1
                case 2:
                    return (channel[i].dest_addr);                       //0,0
                case 3:
                    return (byte)(channel[i].dest_addr + ((index >> 1) & 1));  //0,0,1,1
                case 4:
                    return (byte)(channel[i].dest_addr + (index & 3));         //0,1,2,3
                case 5:
                    return (byte)(channel[i].dest_addr + (index & 1));         //0,1,0,1
                case 6:
                    return (channel[i].dest_addr);                       //0,0     [2]
                case 7:
                    return (byte)(channel[i].dest_addr + ((index >> 1) & 1));  //0,0,1,1 [3]
            }
        }

        private uint dma_addr(uint i)
        {
            uint result = (uint)((channel[i].source_bank << 16) | (channel[i].source_addr));

            if (channel[i].fixed_transfer == false)
            {
                if (channel[i].reverse_transfer == false)
                {
                    channel[i].source_addr++;
                }
                else
                {
                    channel[i].source_addr--;
                }
            }

            return result;
        }

        private uint hdma_addr(uint i)
        {
            return (uint)((channel[i].source_bank << 16) | (channel[i].hdma_addr++));
        }

        private uint hdma_iaddr(uint i)
        {
            return (uint)((channel[i].indirect_bank << 16) | (channel[i].union.indirect_addr++));
        }

        private async Task dma_run()
        {
            await add_clocks(16);

            for (uint i = 0; i < 8; i++)
            {
                if (channel[i].dma_enabled == false)
                {
                    continue;
                }
                await add_clocks(8);

                uint index = 0;
                do
                {
                    await dma_transfer(channel[i].direction, dma_bbus(i, index++), dma_addr(i));
                }
                while (channel[i].dma_enabled && Convert.ToBoolean(--channel[i].union.transfer_size));

                channel[i].dma_enabled = false;
            }

            status.irq_lock = true;
        }

        private bool hdma_active_after(uint i)
        {
            for (uint n = i + 1; i < 8; i++)
            {
                if (channel[i].hdma_enabled && !channel[i].hdma_completed)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task hdma_update(uint i)
        {
            if ((channel[i].line_counter & 0x7f) == 0)
            {
                channel[i].line_counter = await dma_read(hdma_addr(i));
                channel[i].hdma_completed = (channel[i].line_counter == 0);
                channel[i].hdma_do_transfer = !channel[i].hdma_completed;
                await add_clocks(8);

                if (channel[i].indirect)
                {
                    channel[i].union.indirect_addr = (ushort)(await dma_read(hdma_addr(i)) << 8);
                    await add_clocks(8);

                    //emulating this glitch causes a slight slowdown; only enable if needed
                    //if(!channel[i].hdma_completed || hdma_active_after(i)) {
                    channel[i].union.indirect_addr >>= 8;
                    channel[i].union.indirect_addr |= (ushort)(await dma_read(hdma_addr(i)) << 8);
                    await add_clocks(8);
                    //}
                }
            }
        }

        private static readonly uint[] transfer_length = { 1, 2, 2, 4, 4, 4, 2, 4 };
        private async Task hdma_run()
        {
            uint channels = 0;
            for (uint i = 0; i < 8; i++)
            {
                if (channel[i].hdma_enabled)
                {
                    channels++;
                }
            }
            if (channels == 0)
            {
                return;
            }

            await add_clocks(16);
            for (uint i = 0; i < 8; i++)
            {
                if (channel[i].hdma_enabled == false || channel[i].hdma_completed == true)
                {
                    continue;
                }
                channel[i].dma_enabled = false;

                if (channel[i].hdma_do_transfer)
                {
                    uint length = transfer_length[channel[i].transfer_mode];
                    for (uint index = 0; index < length; index++)
                    {
                        uint addr = channel[i].indirect == false ? hdma_addr(i) : hdma_iaddr(i);
                        await dma_transfer(channel[i].direction, dma_bbus(i, index), addr);
                    }
                }
            }

            for (uint i = 0; i < 8; i++)
            {
                if (channel[i].hdma_enabled == false || channel[i].hdma_completed == true)
                {
                    continue;
                }

                channel[i].line_counter--;
                channel[i].hdma_do_transfer = Convert.ToBoolean(channel[i].line_counter & 0x80);
                await hdma_update(i);
            }

            status.irq_lock = true;
        }

        private async Task hdma_init()
        {
            uint channels = 0;
            for (uint i = 0; i < 8; i++)
            {
                channel[i].hdma_completed = false;
                channel[i].hdma_do_transfer = false;
                if (channel[i].hdma_enabled)
                {
                    channels++;
                }
            }
            if (channels == 0)
            {
                return;
            }

            await add_clocks(16);
            for (uint i = 0; i < 8; i++)
            {
                if (!channel[i].hdma_enabled)
                {
                    continue;
                }
                channel[i].dma_enabled = false;

                channel[i].hdma_addr = channel[i].source_addr;
                channel[i].line_counter = 0;
                await hdma_update(i);
            }

            status.irq_lock = true;
        }

        private void dma_reset()
        {
            for (uint i = 0; i < 8; i++)
            {
                channel[i].dma_enabled = false;
                channel[i].hdma_enabled = false;

                channel[i].direction = Convert.ToBoolean(1);
                channel[i].indirect = true;
                channel[i].unused = true;
                channel[i].reverse_transfer = true;
                channel[i].fixed_transfer = true;
                channel[i].transfer_mode = 0x07;

                channel[i].dest_addr = 0xff;
                channel[i].source_addr = 0xffff;
                channel[i].source_bank = 0xff;

                channel[i].union.transfer_size = 0xffff;
                channel[i].union.indirect_addr = 0xffff;

                channel[i].indirect_bank = 0xff;
                channel[i].hdma_addr = 0xff;
                channel[i].line_counter = 0xff;
                channel[i].unknown = 0xff;

                channel[i].hdma_completed = false;
                channel[i].hdma_do_transfer = false;
            }
        }

        //registers
        private byte[] port_data = new byte[4];

        private Channel[] channel = new Channel[8];

        private Status status = new Status();

        private PPUCounter _ppuCounter = new PPUCounter();
        public PPUCounter PPUCounter
        {
            get
            {
                return _ppuCounter;
            }
        }

        private Processor _processor = new Processor();
        public Processor Processor
        {
            get
            {
                return _processor;
            }
        }
    }
}
#endif
