using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    class System
    {
        public static System system = new System();

        public enum Region : uint { NTSC = 0, PAL = 1, Autodetect = 2 }
        public enum ExpansionPortDevice : uint { None = 0, BSX = 1 }

        public async Task run()
        {
            Scheduler.scheduler.sync = Scheduler.SynchronizeMode.None;

            await Scheduler.scheduler.enter();
            if (Scheduler.scheduler.exit_reason == Scheduler.ExitReason.FrameEvent)
            {
                Input.input.update();
                Video.video.update();
            }
        }

        public async Task runtosave()
        {
            Scheduler.scheduler.sync = Scheduler.SynchronizeMode.CPU;
            await runthreadtosave();

            Scheduler.scheduler.thread = SMP.smp.Processor.thread;
			await runthreadtosave();

            Scheduler.scheduler.thread = PPU.ppu.Processor.thread;
			await runthreadtosave();

#if ACCURACY
            Scheduler.scheduler.thread = DSP.dsp.Processor.thread;
            runthreadtosave();
#endif

            for (uint i = 0; i < CPU.cpu.coprocessors.Count; i++)
            {
                IProcessor chip = CPU.cpu.coprocessors[(int)i];
                Scheduler.scheduler.thread = chip.Processor.thread;
				await runthreadtosave();
            }
        }

        public static int _stateFileCount = 0;
        public static void WriteStateToFile()
        {
            //if (_stateFileCount >= 1)
            //{
            Serializer s = system.serialize();
            var file = global::System.IO.File.Create(@"..\..\..\..\..\states\SnesBox\" + _stateFileCount + ".bst");
            file.Write(s.data(), 0, (int)s.size());
            file.Close();
            //}
            _stateFileCount++;
        }

        public void init(Interface interface_)
        {
            inter = interface_;
            Debug.Assert(!ReferenceEquals(inter, null));

            SuperGameBoy.supergameboy.init();
            SuperFX.superfx.init();
            SA1.sa1.init();
            BSXBase.bsxbase.init();
            BSXCart.bsxcart.init();
            BSXFlash.bsxflash.init();
            SRTC.srtc.init();
            SDD1.sdd1.init();
            SPC7110.spc7110.init();
            CX4.cx4.init();
            DSP1.dsp1.init();
            DSP2.dsp2.init();
            DSP3.dsp3.init();
            DSP4.dsp4.init();
            OBC1.obc1.init();
            ST0010.st0010.init();
            ST0011.st0011.init();
            ST0018.st0018.init();
            MSU1.msu1.init();
            Serial.serial.init();

            Video.video.init();
            Audio.audio.init();
            Input.input.init();

            Input.input.port_set_device(Convert.ToBoolean(0), Configuration.config.controller_port1);
            Input.input.port_set_device(Convert.ToBoolean(1), Configuration.config.controller_port2);
        }

        public void term()
        {
        }

        public async Task power()
        {
            region = Configuration.config.region;
            expansion = Configuration.config.expansion_port;
            if (region == Region.Autodetect)
            {
                region = (Cartridge.cartridge.region == Cartridge.Region.NTSC ? Region.NTSC : Region.PAL);
            }

            cpu_frequency = region == Region.NTSC ? Configuration.config.cpu.ntsc_frequency : Configuration.config.cpu.pal_frequency;
            apu_frequency = region == Region.NTSC ? Configuration.config.smp.ntsc_frequency : Configuration.config.smp.pal_frequency;

            Bus.bus.power();
            for (uint i = 0x2100; i <= 0x213f; i++)
            {
                MMIOAccess.mmio.map(i, PPU.ppu);
            }
            for (uint i = 0x2140; i <= 0x217f; i++)
            {
                MMIOAccess.mmio.map(i, CPU.cpu);
            }
            for (uint i = 0x2180; i <= 0x2183; i++)
            {
                MMIOAccess.mmio.map(i, CPU.cpu);
            }
            for (uint i = 0x4016; i <= 0x4017; i++)
            {
                MMIOAccess.mmio.map(i, CPU.cpu);
            }
            for (uint i = 0x4200; i <= 0x421f; i++)
            {
                MMIOAccess.mmio.map(i, CPU.cpu);
            }
            for (uint i = 0x4300; i <= 0x437f; i++)
            {
                MMIOAccess.mmio.map(i, CPU.cpu);
            }

            Audio.audio.coprocessor_enable(false);
            if (expansion == ExpansionPortDevice.BSX)
            {
                BSXBase.bsxbase.enable();
            }
            if (!ReferenceEquals(MappedRAM.bsxflash.data(), null))
            {
                BSXFlash.bsxflash.enable();
            }
            if (Cartridge.cartridge.mode == Cartridge.Mode.Bsx)
            {
                BSXCart.bsxcart.enable();
            }
            if (Cartridge.cartridge.mode == Cartridge.Mode.SuperGameBoy)
            {
                SuperGameBoy.supergameboy.enable();
            }

            if (Cartridge.cartridge.has_superfx)
            {
                SuperFX.superfx.enable();
            }
            if (Cartridge.cartridge.has_sa1)
            {
                SA1.sa1.enable();
            }
            if (Cartridge.cartridge.has_srtc)
            {
                SRTC.srtc.enable();
            }
            if (Cartridge.cartridge.has_sdd1)
            {
                SDD1.sdd1.enable();
            }
            if (Cartridge.cartridge.has_spc7110)
            {
                SPC7110.spc7110.enable();
            }
            if (Cartridge.cartridge.has_cx4)
            {
                CX4.cx4.enable();
            }
            if (Cartridge.cartridge.has_dsp1)
            {
                DSP1.dsp1.enable();
            }
            if (Cartridge.cartridge.has_dsp2)
            {
                DSP2.dsp2.enable();
            }
            if (Cartridge.cartridge.has_dsp3)
            {
                DSP3.dsp3.enable();
            }
            if (Cartridge.cartridge.has_dsp4)
            {
                DSP4.dsp4.enable();
            }
            if (Cartridge.cartridge.has_obc1)
            {
                OBC1.obc1.enable();
            }
            if (Cartridge.cartridge.has_st0010)
            {
                ST0010.st0010.enable();
            }
            if (Cartridge.cartridge.has_st0011)
            {
                ST0011.st0011.enable();
            }
            if (Cartridge.cartridge.has_st0018)
            {
                ST0018.st0018.enable();
            }
            if (Cartridge.cartridge.has_msu1)
            {
                MSU1.msu1.enable();
            }
            if (Cartridge.cartridge.has_serial)
            {
                Serial.serial.enable();
            }

            await CPU.cpu.power();
            await SMP.smp.power();
            DSP.dsp.power();
            PPU.ppu.power();

            if (expansion == ExpansionPortDevice.BSX)
            {
                BSXBase.bsxbase.power();
            }
            if (!ReferenceEquals(MappedRAM.bsxflash.data(), null))
            {
                BSXFlash.bsxflash.power();
            }
            if (Cartridge.cartridge.mode == Cartridge.Mode.Bsx)
            {
                BSXCart.bsxcart.power();
            }
            if (Cartridge.cartridge.mode == Cartridge.Mode.SuperGameBoy)
            {
                SuperGameBoy.supergameboy.power();
            }

            if (Cartridge.cartridge.has_superfx)
            {
                SuperFX.superfx.power();
            }
            if (Cartridge.cartridge.has_sa1)
            {
                SA1.sa1.power();
            }
            if (Cartridge.cartridge.has_srtc)
            {
                SRTC.srtc.power();
            }
            if (Cartridge.cartridge.has_sdd1)
            {
                SDD1.sdd1.power();
            }
            if (Cartridge.cartridge.has_spc7110)
            {
                SPC7110.spc7110.power();
            }
            if (Cartridge.cartridge.has_cx4)
            {
                CX4.cx4.power();
            }
            if (Cartridge.cartridge.has_dsp1)
            {
                DSP1.dsp1.power();
            }
            if (Cartridge.cartridge.has_dsp2)
            {
                DSP2.dsp2.power();
            }
            if (Cartridge.cartridge.has_dsp3)
            {
                DSP3.dsp3.power();
            }
            if (Cartridge.cartridge.has_dsp4)
            {
                DSP4.dsp4.power();
            }
            if (Cartridge.cartridge.has_obc1)
            {
                OBC1.obc1.power();
            }
            if (Cartridge.cartridge.has_st0010)
            {
                ST0010.st0010.power();
            }
            if (Cartridge.cartridge.has_st0011)
            {
                ST0011.st0011.power();
            }
            if (Cartridge.cartridge.has_st0018)
            {
                ST0018.st0018.power();
            }
            if (Cartridge.cartridge.has_msu1)
            {
                MSU1.msu1.power();
            }
            if (Cartridge.cartridge.has_serial)
            {
                Serial.serial.power();
            }

            if (Cartridge.cartridge.mode == Cartridge.Mode.SuperGameBoy)
            {
                CPU.cpu.coprocessors.Add(SuperGameBoy.supergameboy.Coprocessor);
            }
            if (Cartridge.cartridge.has_superfx)
            {
                CPU.cpu.coprocessors.Add(SuperFX.superfx.Coprocessor);
            }
            if (Cartridge.cartridge.has_sa1)
            {
                CPU.cpu.coprocessors.Add(SA1.sa1.Coprocessor);
            }
            if (Cartridge.cartridge.has_msu1)
            {
                CPU.cpu.coprocessors.Add(MSU1.msu1.Coprocessor);
            }
            if (Cartridge.cartridge.has_serial)
            {
                CPU.cpu.coprocessors.Add(Serial.serial.Coprocessor);
            }

            Scheduler.scheduler.init();

            Input.input.update();
        }

        public async Task reset()
        {
            Bus.bus.reset();
            await CPU.cpu.reset();
            await SMP.smp.reset();
            DSP.dsp.reset();
            PPU.ppu.reset();

            if (expansion == ExpansionPortDevice.BSX)
            {
                BSXBase.bsxbase.reset();
            }
            if (!ReferenceEquals(MappedRAM.bsxflash.data(), null))
            {
                BSXFlash.bsxflash.reset();
            }
            if (Cartridge.cartridge.mode == Cartridge.Mode.Bsx)
            {
                BSXCart.bsxcart.reset();
            }
            if (Cartridge.cartridge.mode == Cartridge.Mode.SuperGameBoy)
            {
                SuperGameBoy.supergameboy.reset();
            }

            if (Cartridge.cartridge.has_superfx)
            {
                SuperFX.superfx.reset();
            }
            if (Cartridge.cartridge.has_sa1)
            {
                SA1.sa1.reset();
            }
            if (Cartridge.cartridge.has_srtc)
            {
                SRTC.srtc.reset();
            }
            if (Cartridge.cartridge.has_sdd1)
            {
                SDD1.sdd1.reset();
            }
            if (Cartridge.cartridge.has_spc7110)
            {
                SPC7110.spc7110.reset();
            }
            if (Cartridge.cartridge.has_cx4)
            {
                CX4.cx4.reset();
            }
            if (Cartridge.cartridge.has_dsp1)
            {
                DSP1.dsp1.reset();
            }
            if (Cartridge.cartridge.has_dsp2)
            {
                DSP2.dsp2.reset();
            }
            if (Cartridge.cartridge.has_dsp3)
            {
                DSP3.dsp3.reset();
            }
            if (Cartridge.cartridge.has_dsp4)
            {
                DSP4.dsp4.reset();
            }
            if (Cartridge.cartridge.has_obc1)
            {
                OBC1.obc1.reset();
            }
            if (Cartridge.cartridge.has_st0010)
            {
                ST0010.st0010.reset();
            }
            if (Cartridge.cartridge.has_st0011)
            {
                ST0011.st0011.reset();
            }
            if (Cartridge.cartridge.has_st0018)
            {
                ST0018.st0018.reset();
            }
            if (Cartridge.cartridge.has_msu1)
            {
                MSU1.msu1.reset();
            }
            if (Cartridge.cartridge.has_serial)
            {
                Serial.serial.reset();
            }

            if (Cartridge.cartridge.mode == Cartridge.Mode.SuperGameBoy)
            {
                CPU.cpu.coprocessors.Add(SuperGameBoy.supergameboy.Coprocessor);
            }
            if (Cartridge.cartridge.has_superfx)
            {
                CPU.cpu.coprocessors.Add(SuperFX.superfx.Coprocessor);
            }
            if (Cartridge.cartridge.has_sa1)
            {
                CPU.cpu.coprocessors.Add(SA1.sa1.Coprocessor);
            }
            if (Cartridge.cartridge.has_msu1)
            {
                CPU.cpu.coprocessors.Add(MSU1.msu1.Coprocessor);
            }
            if (Cartridge.cartridge.has_serial)
            {
                CPU.cpu.coprocessors.Add(Serial.serial.Coprocessor);
            }

            Scheduler.scheduler.init();

            Input.input.port_set_device(Convert.ToBoolean(0), Configuration.config.controller_port1);
            Input.input.port_set_device(Convert.ToBoolean(1), Configuration.config.controller_port2);
            Input.input.update();
            //video.update();
        }

        public void unload()
        {
            if (Cartridge.cartridge.mode == Cartridge.Mode.SuperGameBoy)
            {
                SuperGameBoy.supergameboy.unload();
            }
        }

        public void frame()
        {
        }

        public async Task scanline()
        {
            Video.video.scanline();
            if (CPU.cpu.PPUCounter.vcounter() == 241)
            {
                await Scheduler.scheduler.exit(Scheduler.ExitReason.FrameEvent);
            }
        }

        //return *active* system information (settings are cached upon power-on)
        public Region region { get; private set; }
        public ExpansionPortDevice expansion { get; private set; }
        public uint cpu_frequency { get; private set; }
        public uint apu_frequency { get; private set; }
        public uint serialize_size { get; private set; }

        public Serializer serialize()
        {
            Serializer s = new Serializer(serialize_size);

            uint signature = 0x31545342, version = Info.SerializerVersion, crc32 = Cartridge.cartridge.crc32;
            byte[] profile = new byte[16], description = new byte[512];
            new UTF8Encoding().GetBytes(Info.Profile).CopyTo(profile, 0);

            s.integer(signature, "signature");
            s.integer(version, "version");
            s.integer(crc32, "crc32");
            s.array(profile, "profile");
            s.array(description, "description");

            serialize_all(s);
            return s;
        }

        public async Task<bool> unserialize(Serializer s)
        {
            uint signature = 0, version = 0, crc32 = 0;
            byte[] profile = new byte[16], description = new byte[512];

            s.integer(signature, "signature");
            s.integer(version, "version");
            s.integer(crc32, "crc32");
            s.array(profile, "profile");
            s.array(description, "description");

            if (signature != 0x31545342)
            {
                return false;
            }
            if (version != Info.SerializerVersion)
            {
                return false;
            }
            if (new UTF8Encoding().GetString(profile, 0, profile.Length) != Info.Profile)
            {
                return false;
            }

            await reset();
            serialize_all(s);
            return true;
        }

        public System()
        {
            inter = null;
            region = Region.Autodetect;
            expansion = ExpansionPortDevice.None;
        }

        private Interface inter;
        public Interface Interface
        {
            get { return inter; }
        }

        private async Task runthreadtosave()
        {
            while (true)
            {
                await Scheduler.scheduler.enter();
                if (Scheduler.scheduler.exit_reason == Scheduler.ExitReason.SynchronizeEvent)
                {
                    break;
                }
                if (Scheduler.scheduler.exit_reason == Scheduler.ExitReason.FrameEvent)
                {
                    Input.input.update();
                    Video.video.update();
                }
            }
        }

        private void serialize(Serializer s)
        {
            s.integer((uint)region, "(unsigned&)region");
            s.integer((uint)expansion, "(unsigned&)expansion");
        }

        private void serialize_all(Serializer s)
        {
            Bus.bus.serialize(s);
            Cartridge.cartridge.serialize(s);
            System.system.serialize(s);
            CPU.cpu.serialize(s);
            SMP.smp.serialize(s);
            PPU.ppu.serialize(s);
            DSP.dsp.serialize(s);

            if (Cartridge.cartridge.mode == Cartridge.Mode.SuperGameBoy)
            {
                SuperGameBoy.supergameboy.serialize(s);
            }
            if (Cartridge.cartridge.has_superfx)
            {
                SuperFX.superfx.serialize(s);
            }
            if (Cartridge.cartridge.has_sa1)
            {
                SA1.sa1.serialize(s);
            }
            if (Cartridge.cartridge.has_srtc)
            {
                SRTC.srtc.serialize(s);
            }
            if (Cartridge.cartridge.has_sdd1)
            {
                SDD1.sdd1.serialize(s);
            }
            if (Cartridge.cartridge.has_spc7110)
            {
                SPC7110.spc7110.serialize(s);
            }
            if (Cartridge.cartridge.has_cx4)
            {
                CX4.cx4.serialize(s);
            }
            if (Cartridge.cartridge.has_dsp1)
            {
                DSP1.dsp1.serialize(s);
            }
            if (Cartridge.cartridge.has_dsp2)
            {
                DSP2.dsp2.serialize(s);
            }
            if (Cartridge.cartridge.has_obc1)
            {
                OBC1.obc1.serialize(s);
            }
            if (Cartridge.cartridge.has_st0010)
            {
                ST0010.st0010.serialize(s);
            }
            if (Cartridge.cartridge.has_msu1)
            {
                MSU1.msu1.serialize(s);
            }
            if (Cartridge.cartridge.has_serial)
            {
                Serial.serial.serialize(s);
            }
        }

        //called once upon cartridge load event: perform dry-run state save.
        //determines exactly how many bytes are needed to save state for this cartridge,
        //as amount varies per game (eg different RAM sizes, special chips, etc.)
        public void serialize_init()
        {
            Serializer s = new Serializer();

            uint signature = 0, version = 0, crc32 = 0;
            byte[] profile = new byte[16], description = new byte[512];

            s.integer(signature, "signature");
            s.integer(version, "version");
            s.integer(crc32, "crc32");
            s.array(profile, "profile");
            s.array(description, "description");

            serialize_all(s);
            serialize_size = s.size();
        }
    }
}
