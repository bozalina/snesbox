﻿
namespace Snes
{
    partial class Configuration
    {
        public static Configuration config = new Configuration();

        public Input.Device controller_port1;
        public Input.Device controller_port2;
        public System.ExpansionPortDevice expansion_port;
        public System.Region region;

        public CPU cpu = new CPU();
        public SMP smp = new SMP();
        public PPU1 ppu1 = new PPU1();
        public PPU2 ppu2 = new PPU2();
        public SuperFX superfx = new SuperFX();

        public Configuration()
        {
            controller_port1 = Input.Device.Joypad;
            controller_port2 = Input.Device.Joypad;
            expansion_port = System.ExpansionPortDevice.BSX;
            region = System.Region.Autodetect;

            cpu.version = 2;
            cpu.ntsc_frequency = 21477272;  //315 / 88 * 6000000
            cpu.pal_frequency = 21281370;
            cpu.wram_init_value = 0x55;

            smp.ntsc_frequency = 24607104;   //32040.5 * 768
            smp.pal_frequency = 24607104;

            ppu1.version = 1;
            ppu2.version = 3;

            superfx.speed = 0;  //0 = auto-select, 1 = force 10.74MHz, 2 = force 21.48MHz
        }
    }
}
