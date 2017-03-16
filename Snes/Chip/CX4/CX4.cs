using System;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    class CX4 : Memory
    {
        public static CX4 cx4 = new CX4();

        public override uint size() { throw new NotImplementedException(); }
        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public override Task<byte> read(uint addr) { throw new NotImplementedException(); }
        public override Task write(uint addr, byte data) { throw new NotImplementedException(); }

        public void serialize(Serializer s)
        {
            throw new NotImplementedException();
        }

        private byte[] ram = new byte[0x0c00];
        private byte[] reg = new byte[0x0100];
        private uint r0, r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15;

        private static readonly byte[] immediate_data = new byte[48];
        private static readonly ushort[] wave_data = new ushort[40];
        private static readonly uint[] sin_table = new uint[256];

        private static readonly short[] SinTable = new short[512];
        private static readonly short[] CosTable = new short[512];

        private short C4WFXVal, C4WFYVal, C4WFZVal, C4WFX2Val, C4WFY2Val, C4WFDist, C4WFScale;
        private short C41FXVal, C41FYVal, C41FAngleRes, C41FDist, C41FDistVal;

        private void C4TransfWireFrame() { throw new NotImplementedException(); }
        private void C4TransfWireFrame2() { throw new NotImplementedException(); }
        private void C4CalcWireFrame() { throw new NotImplementedException(); }
        private void C4DrawLine(int X1, int Y1, short Z1, int X2, int Y2, short Z2, byte Color) { throw new NotImplementedException(); }
        private void C4DrawWireFrame() { throw new NotImplementedException(); }
        private void C4DoScaleRotate(int row_padding) { throw new NotImplementedException(); }

        public uint ldr(byte r) { throw new NotImplementedException(); }
        public void str(byte r, uint data) { throw new NotImplementedException(); }
        public void mul(uint x, uint y, ref uint rl, ref uint rh) { throw new NotImplementedException(); }
        public uint sin(uint rx) { throw new NotImplementedException(); }
        public uint cos(uint rx) { throw new NotImplementedException(); }

        public void transfer_data() { throw new NotImplementedException(); }
        public void immediate_reg(uint num) { throw new NotImplementedException(); }

        public void op00_00() { throw new NotImplementedException(); }
        public void op00_03() { throw new NotImplementedException(); }
        public void op00_05() { throw new NotImplementedException(); }
        public void op00_07() { throw new NotImplementedException(); }
        public void op00_08() { throw new NotImplementedException(); }
        public void op00_0b() { throw new NotImplementedException(); }
        public void op00_0c() { throw new NotImplementedException(); }

        public void op00() { throw new NotImplementedException(); }
        public void op01() { throw new NotImplementedException(); }
        public void op05() { throw new NotImplementedException(); }
        public void op0d() { throw new NotImplementedException(); }
        public void op10() { throw new NotImplementedException(); }
        public void op13() { throw new NotImplementedException(); }
        public void op15() { throw new NotImplementedException(); }
        public void op1f() { throw new NotImplementedException(); }
        public void op22() { throw new NotImplementedException(); }
        public void op25() { throw new NotImplementedException(); }
        public void op2d() { throw new NotImplementedException(); }
        public void op40() { throw new NotImplementedException(); }
        public void op54() { throw new NotImplementedException(); }
        public void op5c() { throw new NotImplementedException(); }
        public void op5e() { throw new NotImplementedException(); }
        public void op60() { throw new NotImplementedException(); }
        public void op62() { throw new NotImplementedException(); }
        public void op64() { throw new NotImplementedException(); }
        public void op66() { throw new NotImplementedException(); }
        public void op68() { throw new NotImplementedException(); }
        public void op6a() { throw new NotImplementedException(); }
        public void op6c() { throw new NotImplementedException(); }
        public void op6e() { throw new NotImplementedException(); }
        public void op70() { throw new NotImplementedException(); }
        public void op72() { throw new NotImplementedException(); }
        public void op74() { throw new NotImplementedException(); }
        public void op76() { throw new NotImplementedException(); }
        public void op78() { throw new NotImplementedException(); }
        public void op7a() { throw new NotImplementedException(); }
        public void op7c() { throw new NotImplementedException(); }
        public void op89() { throw new NotImplementedException(); }

        public byte readb(ushort addr) { throw new NotImplementedException(); }
        public ushort readw(ushort addr) { throw new NotImplementedException(); }
        public uint readl(ushort addr) { throw new NotImplementedException(); }

        public void writeb(ushort addr, byte data) { throw new NotImplementedException(); }
        public void writew(ushort addr, ushort data) { throw new NotImplementedException(); }
        public void writel(ushort addr, uint data) { throw new NotImplementedException(); }
    }
}
