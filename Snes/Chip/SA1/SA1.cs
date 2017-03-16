using System;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    partial class SA1 : CPUCore, ICoprocessor, IMMIO
    {
        public static SA1 sa1 = new SA1();

        public DMA dma;

        public void dma_normal() { throw new NotImplementedException(); }
        public void dma_cc1() { throw new NotImplementedException(); }
        public byte dma_cc1_read(uint addr) { throw new NotImplementedException(); }
        public void dma_cc2() { throw new NotImplementedException(); }

        public override Task op_io() { throw new NotImplementedException(); }
        public override Task<byte> op_read(uint addr) { throw new NotImplementedException(); }
        public override Task op_write(uint addr, byte data) { throw new NotImplementedException(); }

        public byte vbr_read(uint addr) { throw new NotImplementedException(); }

        public Task<byte> mmio_read(uint addr) { throw new NotImplementedException(); }
        public Task mmio_write(uint addr, byte data) { throw new NotImplementedException(); }
        public Memory mmio_access(ref uint addr) { throw new NotImplementedException(); }

        public MMIO mmio;

        public void mmio_w2200(byte data) { throw new NotImplementedException(); }  //CCNT
        public void mmio_w2201(byte data) { throw new NotImplementedException(); }  //SIE
        public void mmio_w2202(byte data) { throw new NotImplementedException(); }  //SIC
        public void mmio_w2203(byte data) { throw new NotImplementedException(); }  //CRVL
        public void mmio_w2204(byte data) { throw new NotImplementedException(); }  //CRVH
        public void mmio_w2205(byte data) { throw new NotImplementedException(); }  //CNVL
        public void mmio_w2206(byte data) { throw new NotImplementedException(); }  //CNVH
        public void mmio_w2207(byte data) { throw new NotImplementedException(); }  //CIVL
        public void mmio_w2208(byte data) { throw new NotImplementedException(); }  //CIVH
        public void mmio_w2209(byte data) { throw new NotImplementedException(); }  //SCNT
        public void mmio_w220a(byte data) { throw new NotImplementedException(); }  //CIE
        public void mmio_w220b(byte data) { throw new NotImplementedException(); }  //CIC
        public void mmio_w220c(byte data) { throw new NotImplementedException(); }  //SNVL
        public void mmio_w220d(byte data) { throw new NotImplementedException(); }  //SNVH
        public void mmio_w220e(byte data) { throw new NotImplementedException(); }  //SIVL
        public void mmio_w220f(byte data) { throw new NotImplementedException(); }  //SIVH
        public void mmio_w2210(byte data) { throw new NotImplementedException(); }  //TMC
        public void mmio_w2211(byte data) { throw new NotImplementedException(); }  //CTR
        public void mmio_w2212(byte data) { throw new NotImplementedException(); }  //HCNTL
        public void mmio_w2213(byte data) { throw new NotImplementedException(); }  //HCNTH
        public void mmio_w2214(byte data) { throw new NotImplementedException(); }  //VCNTL
        public void mmio_w2215(byte data) { throw new NotImplementedException(); }  //VCNTH
        public void mmio_w2220(byte data) { throw new NotImplementedException(); }  //CXB
        public void mmio_w2221(byte data) { throw new NotImplementedException(); }  //DXB
        public void mmio_w2222(byte data) { throw new NotImplementedException(); }  //EXB
        public void mmio_w2223(byte data) { throw new NotImplementedException(); }  //FXB
        public void mmio_w2224(byte data) { throw new NotImplementedException(); }  //BMAPS
        public void mmio_w2225(byte data) { throw new NotImplementedException(); }  //BMAP
        public void mmio_w2226(byte data) { throw new NotImplementedException(); }  //SBWE
        public void mmio_w2227(byte data) { throw new NotImplementedException(); }  //CBWE
        public void mmio_w2228(byte data) { throw new NotImplementedException(); }  //BWPA
        public void mmio_w2229(byte data) { throw new NotImplementedException(); }  //SIWP
        public void mmio_w222a(byte data) { throw new NotImplementedException(); }  //CIWP
        public void mmio_w2230(byte data) { throw new NotImplementedException(); }  //DCNT
        public void mmio_w2231(byte data) { throw new NotImplementedException(); }  //CDMA
        public void mmio_w2232(byte data) { throw new NotImplementedException(); }  //SDAL
        public void mmio_w2233(byte data) { throw new NotImplementedException(); }  //SDAH
        public void mmio_w2234(byte data) { throw new NotImplementedException(); }  //SDAB
        public void mmio_w2235(byte data) { throw new NotImplementedException(); }  //DDAL
        public void mmio_w2236(byte data) { throw new NotImplementedException(); }  //DDAH
        public void mmio_w2237(byte data) { throw new NotImplementedException(); }  //DDAB
        public void mmio_w2238(byte data) { throw new NotImplementedException(); }  //DTCL
        public void mmio_w2239(byte data) { throw new NotImplementedException(); }  //DTCH
        public void mmio_w223f(byte data) { throw new NotImplementedException(); }  //BBF
        public void mmio_w2240(byte data) { throw new NotImplementedException(); }  //BRF0
        public void mmio_w2241(byte data) { throw new NotImplementedException(); }  //BRF1
        public void mmio_w2242(byte data) { throw new NotImplementedException(); }  //BRF2
        public void mmio_w2243(byte data) { throw new NotImplementedException(); }  //BRF3
        public void mmio_w2244(byte data) { throw new NotImplementedException(); }  //BRF4
        public void mmio_w2245(byte data) { throw new NotImplementedException(); }  //BRF5
        public void mmio_w2246(byte data) { throw new NotImplementedException(); }  //BRF6
        public void mmio_w2247(byte data) { throw new NotImplementedException(); }  //BRF7
        public void mmio_w2248(byte data) { throw new NotImplementedException(); }  //BRF8
        public void mmio_w2249(byte data) { throw new NotImplementedException(); }  //BRF9
        public void mmio_w224a(byte data) { throw new NotImplementedException(); }  //BRFA
        public void mmio_w224b(byte data) { throw new NotImplementedException(); }  //BRFB
        public void mmio_w224c(byte data) { throw new NotImplementedException(); }  //BRFC
        public void mmio_w224d(byte data) { throw new NotImplementedException(); }  //BRFD
        public void mmio_w224e(byte data) { throw new NotImplementedException(); }  //BRFE
        public void mmio_w224f(byte data) { throw new NotImplementedException(); }  //BRFF
        public void mmio_w2250(byte data) { throw new NotImplementedException(); }  //MCNT
        public void mmio_w2251(byte data) { throw new NotImplementedException(); }  //MAL
        public void mmio_w2252(byte data) { throw new NotImplementedException(); }  //MAH
        public void mmio_w2253(byte data) { throw new NotImplementedException(); }  //MBL
        public void mmio_w2254(byte data) { throw new NotImplementedException(); }  //MBH
        public void mmio_w2258(byte data) { throw new NotImplementedException(); }  //VBD
        public void mmio_w2259(byte data) { throw new NotImplementedException(); }  //VDAL
        public void mmio_w225a(byte data) { throw new NotImplementedException(); }  //VDAH
        public void mmio_w225b(byte data) { throw new NotImplementedException(); }  //VDAB

        public byte mmio_r2300() { throw new NotImplementedException(); }  //SFR
        public byte mmio_r2301() { throw new NotImplementedException(); }  //CFR
        public byte mmio_r2302() { throw new NotImplementedException(); }  //HCRL
        public byte mmio_r2303() { throw new NotImplementedException(); }  //HCRH
        public byte mmio_r2304() { throw new NotImplementedException(); }  //VCRL
        public byte mmio_r2305() { throw new NotImplementedException(); }  //VCRH
        public byte mmio_r2306() { throw new NotImplementedException(); }  //MR [00-07]
        public byte mmio_r2307() { throw new NotImplementedException(); }  //MR [08-15]
        public byte mmio_r2308() { throw new NotImplementedException(); }  //MR [16-23]
        public byte mmio_r2309() { throw new NotImplementedException(); }  //MR [24-31]
        public byte mmio_r230a() { throw new NotImplementedException(); }  //MR [32-40]
        public byte mmio_r230b() { throw new NotImplementedException(); }  //OF
        public byte mmio_r230c() { throw new NotImplementedException(); }  //VDPL
        public byte mmio_r230d() { throw new NotImplementedException(); }  //VDPH
        public byte mmio_r230e() { throw new NotImplementedException(); }  //VC

        public Status status;

        public static void Enter() { throw new NotImplementedException(); }
        public void enter() { throw new NotImplementedException(); }
        public void interrupt(ushort vector) { throw new NotImplementedException(); }
        public void tick() { throw new NotImplementedException(); }

        public void trigger_irq() { throw new NotImplementedException(); }
        public override void last_cycle() { throw new NotImplementedException(); }
        public override bool interrupt_pending() { throw new NotImplementedException(); }

        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public void serialize(Serializer s)
        {
            throw new NotImplementedException();
        }

        public SA1() { /*throw new NotImplementedException();*/ }

        public Coprocessor Coprocessor
        {
            get { throw new NotImplementedException(); }
        }
    }
}
