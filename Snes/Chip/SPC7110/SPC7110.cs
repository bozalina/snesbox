using System;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    class SPC7110 : IMMIO
    {
        public static SPC7110 spc7110 = new SPC7110();

        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public uint datarom_addr(uint addr) { throw new NotImplementedException(); }

        public uint data_pointer() { throw new NotImplementedException(); }
        public uint data_adjust() { throw new NotImplementedException(); }
        public uint data_increment() { throw new NotImplementedException(); }
        public void set_data_pointer(uint addr) { throw new NotImplementedException(); }
        public void set_data_adjust(uint addr) { throw new NotImplementedException(); }

        public void update_time() { throw new NotImplementedException(); }
        public void update_time(int offset) { throw new NotImplementedException(); }
        public DateTime create_time() { throw new NotImplementedException(); }

        public Task<byte> mmio_read(uint addr) { throw new NotImplementedException(); }
        public Task mmio_write(uint addr, byte data) { throw new NotImplementedException(); }

        //spc7110decomp
        public void decomp_init() { throw new NotImplementedException(); }
        public byte decomp_read() { throw new NotImplementedException(); }

        public void serialize(Serializer s)
        {
            throw new NotImplementedException();
        }

        public SPC7110() { /*throw new NotImplementedException();*/ }

        //==================
        //decompression unit
        //==================
        private byte r4801;  //compression table low
        private byte r4802;  //compression table high
        private byte r4803;  //compression table bank
        private byte r4804;  //compression table index
        private byte r4805;  //decompression buffer index low
        private byte r4806;  //decompression buffer index high
        private byte r4807;  //???
        private byte r4808;  //???
        private byte r4809;  //compression length low
        private byte r480a;  //compression length high
        private byte r480b;  //decompression control register
        private byte r480c;  //decompression status

        private SPC7110Decomp decomp;

        //==============
        //data port unit
        //==============
        private byte r4811;  //data pointer low
        private byte r4812;  //data pointer high
        private byte r4813;  //data pointer bank
        private byte r4814;  //data adjust low
        private byte r4815;  //data adjust high
        private byte r4816;  //data increment low
        private byte r4817;  //data increment high
        private byte r4818;  //data port control register

        private byte r481x;

        private bool r4814_latch;
        private bool r4815_latch;

        //=========
        //math unit
        //=========
        private byte r4820;  //16-bit multiplicand B0, 32-bit dividend B0
        private byte r4821;  //16-bit multiplicand B1, 32-bit dividend B1
        private byte r4822;  //32-bit dividend B2
        private byte r4823;  //32-bit dividend B3
        private byte r4824;  //16-bit multiplier B0
        private byte r4825;  //16-bit multiplier B1
        private byte r4826;  //16-bit divisor B0
        private byte r4827;  //16-bit divisor B1
        private byte r4828;  //32-bit product B0, 32-bit quotient B0
        private byte r4829;  //32-bit product B1, 32-bit quotient B1
        private byte r482a;  //32-bit product B2, 32-bit quotient B2
        private byte r482b;  //32-bit product B3, 32-bit quotient B3
        private byte r482c;  //16-bit remainder B0
        private byte r482d;  //16-bit remainder B1
        private byte r482e;  //math control register
        private byte r482f;  //math status

        //===================
        //memory mapping unit
        //===================
        private byte r4830;  //SRAM write enable
        private byte r4831;  //$[d0-df]:[0000-ffff] mapping
        private byte r4832;  //$[e0-ef]:[0000-ffff] mapping
        private byte r4833;  //$[f0-ff]:[0000-ffff] mapping
        private byte r4834;  //???

        private uint dx_offset;
        private uint ex_offset;
        private uint fx_offset;

        //====================
        //real-time clock unit
        //====================
        private byte r4840;  //RTC latch
        private byte r4841;  //RTC index/data port
        private byte r4842;  //RTC status

        private enum RTC_State { Inactive, ModeSelect, IndexSelect, Write }
        private enum RTC_Mode { Linear = 0x03, Indexed = 0x0c }
        private uint rtc_state;
        private uint rtc_mode;
        private uint rtc_index;

        private static readonly uint[] months = new uint[12];
    }
}
