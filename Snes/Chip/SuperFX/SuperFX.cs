using System;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    partial class SuperFX : ICoprocessor, IMMIO
    {
        public static SuperFX superfx = new SuperFX();

        public Regs regs;
        public Cache cache;
        public PixelCache[] pixelcache = new PixelCache[2];

        public byte color(byte source) { throw new NotImplementedException(); }
        public void plot(byte x, byte y) { throw new NotImplementedException(); }
        public byte rpix(byte x, byte y) { throw new NotImplementedException(); }
        public void pixelcache_flush(PixelCache cache) { throw new NotImplementedException(); }

        public Delegate[] opcode_table = new Delegate[1024];
        public void initialize_opcode_table() { throw new NotImplementedException(); }

        public void op_adc_i(int n) { throw new NotImplementedException(); }
        public void op_adc_r(int n) { throw new NotImplementedException(); }
        public void op_add_i(int n) { throw new NotImplementedException(); }
        public void op_add_r(int n) { throw new NotImplementedException(); }
        public void op_alt1() { throw new NotImplementedException(); }
        public void op_alt2() { throw new NotImplementedException(); }
        public void op_alt3() { throw new NotImplementedException(); }
        public void op_and_i(int n) { throw new NotImplementedException(); }
        public void op_and_r(int n) { throw new NotImplementedException(); }
        public void op_asr() { throw new NotImplementedException(); }
        public void op_bge() { throw new NotImplementedException(); }
        public void op_bcc() { throw new NotImplementedException(); }
        public void op_bcs() { throw new NotImplementedException(); }
        public void op_beq() { throw new NotImplementedException(); }
        public void op_bic_i(int n) { throw new NotImplementedException(); }
        public void op_bic_r(int n) { throw new NotImplementedException(); }
        public void op_blt() { throw new NotImplementedException(); }
        public void op_bmi() { throw new NotImplementedException(); }
        public void op_bne() { throw new NotImplementedException(); }
        public void op_bpl() { throw new NotImplementedException(); }
        public void op_bra() { throw new NotImplementedException(); }
        public void op_bvc() { throw new NotImplementedException(); }
        public void op_bvs() { throw new NotImplementedException(); }
        public void op_cache() { throw new NotImplementedException(); }
        public void op_cmode() { throw new NotImplementedException(); }
        public void op_cmp_r(int n) { throw new NotImplementedException(); }
        public void op_color() { throw new NotImplementedException(); }
        public void op_dec_r(int n) { throw new NotImplementedException(); }
        public void op_div2() { throw new NotImplementedException(); }
        public void op_fmult() { throw new NotImplementedException(); }
        public void op_from_r(int n) { throw new NotImplementedException(); }
        public void op_getb() { throw new NotImplementedException(); }
        public void op_getbl() { throw new NotImplementedException(); }
        public void op_getbh() { throw new NotImplementedException(); }
        public void op_getbs() { throw new NotImplementedException(); }
        public void op_getc() { throw new NotImplementedException(); }
        public void op_hib() { throw new NotImplementedException(); }
        public void op_ibt_r(int n) { throw new NotImplementedException(); }
        public void op_inc_r(int n) { throw new NotImplementedException(); }
        public void op_iwt_r(int n) { throw new NotImplementedException(); }
        public void op_jmp_r(int n) { throw new NotImplementedException(); }
        public void op_ldb_ir(int n) { throw new NotImplementedException(); }
        public void op_ldw_ir(int n) { throw new NotImplementedException(); }
        public void op_link(int n) { throw new NotImplementedException(); }
        public void op_ljmp_r(int n) { throw new NotImplementedException(); }
        public void op_lm_r(int n) { throw new NotImplementedException(); }
        public void op_lms_r(int n) { throw new NotImplementedException(); }
        public void op_lmult() { throw new NotImplementedException(); }
        public void op_lob() { throw new NotImplementedException(); }
        public void op_loop() { throw new NotImplementedException(); }
        public void op_lsr() { throw new NotImplementedException(); }
        public void op_merge() { throw new NotImplementedException(); }
        public void op_mult_i(int n) { throw new NotImplementedException(); }
        public void op_mult_r(int n) { throw new NotImplementedException(); }
        public void op_nop() { throw new NotImplementedException(); }
        public void op_not() { throw new NotImplementedException(); }
        public void op_or_i(int n) { throw new NotImplementedException(); }
        public void op_or_r(int n) { throw new NotImplementedException(); }
        public void op_plot() { throw new NotImplementedException(); }
        public void op_ramb() { throw new NotImplementedException(); }
        public void op_rol() { throw new NotImplementedException(); }
        public void op_romb() { throw new NotImplementedException(); }
        public void op_ror() { throw new NotImplementedException(); }
        public void op_rpix() { throw new NotImplementedException(); }
        public void op_sbc_r(int n) { throw new NotImplementedException(); }
        public void op_sbk() { throw new NotImplementedException(); }
        public void op_sex() { throw new NotImplementedException(); }
        public void op_sm_r(int n) { throw new NotImplementedException(); }
        public void op_sms_r(int n) { throw new NotImplementedException(); }
        public void op_stb_ir(int n) { throw new NotImplementedException(); }
        public void op_stop() { throw new NotImplementedException(); }
        public void op_stw_ir(int n) { throw new NotImplementedException(); }
        public void op_sub_i(int n) { throw new NotImplementedException(); }
        public void op_sub_r(int n) { throw new NotImplementedException(); }
        public void op_swap() { throw new NotImplementedException(); }
        public void op_to_r(int n) { throw new NotImplementedException(); }
        public void op_umult_i(int n) { throw new NotImplementedException(); }
        public void op_umult_r(int n) { throw new NotImplementedException(); }
        public void op_with_r(int n) { throw new NotImplementedException(); }
        public void op_xor_i(int n) { throw new NotImplementedException(); }
        public void op_xor_r(int n) { throw new NotImplementedException(); }

        public byte op_read(ushort addr) { throw new NotImplementedException(); }
        public byte peekpipe() { throw new NotImplementedException(); }
        public byte pipe() { throw new NotImplementedException(); }

        public void cache_flush() { throw new NotImplementedException(); }
        public byte cache_mmio_read(ushort addr) { throw new NotImplementedException(); }
        public void cache_mmio_write(ushort addr, byte data) { throw new NotImplementedException(); }

        public void memory_reset() { throw new NotImplementedException(); }

        public Task<byte> mmio_read(uint addr) { throw new NotImplementedException(); }
        public Task mmio_write(uint addr, byte data) { throw new NotImplementedException(); }

        public uint cache_access_speed;
        public uint memory_access_speed;
        public bool r15_modified;

        public void add_clocks(uint clocks) { throw new NotImplementedException(); }

        public void rombuffer_sync() { throw new NotImplementedException(); }
        public void rombuffer_update() { throw new NotImplementedException(); }
        public byte rombuffer_read() { throw new NotImplementedException(); }

        public void rambuffer_sync() { throw new NotImplementedException(); }
        public byte rambuffer_read(ushort addr) { throw new NotImplementedException(); }
        public void rambuffer_write(ushort addr, byte data) { throw new NotImplementedException(); }

        public void r14_modify(ushort data) { throw new NotImplementedException(); }
        public void r15_modify(ushort data) { throw new NotImplementedException(); }

        public void update_speed() { throw new NotImplementedException(); }
        public void timing_reset() { throw new NotImplementedException(); }

        public void disassemble_opcode(byte[] output) { throw new NotImplementedException(); }
        public void disassemble_alt0(byte[] output) { throw new NotImplementedException(); }
        public void disassemble_alt1(byte[] output) { throw new NotImplementedException(); }
        public void disassemble_alt2(byte[] output) { throw new NotImplementedException(); }
        public void disassemble_alt3(byte[] output) { throw new NotImplementedException(); }

        public static void Enter() { throw new NotImplementedException(); }
        public void enter() { throw new NotImplementedException(); }
        public void init() {  /*throw new NotImplementedException();*/  }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public void serialize(Serializer s)
        {
            throw new NotImplementedException();
        }

        private uint clockmode;
        private uint instruction_counter;

        public Coprocessor Coprocessor
        {
            get { throw new NotImplementedException(); }
        }
    }
}
