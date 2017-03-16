using System;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    class ST0010 : Memory
    {
        public static ST0010 st0010 = new ST0010();

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

        private byte[] ram = new byte[0x1000];
        private static readonly short[] sin_table = new short[256];
        private static readonly short[] mode7_scale = new short[176];
        private static readonly byte[,] arctan = new byte[32, 32];

        //interfaces to sin table
        private short sin(short theta) { throw new NotImplementedException(); }
        private short cos(short theta) { throw new NotImplementedException(); }

        //interfaces to ram buffer
        private byte readb(ushort addr) { throw new NotImplementedException(); }
        private ushort readw(ushort addr) { throw new NotImplementedException(); }
        private uint readd(ushort addr) { throw new NotImplementedException(); }
        private void writeb(ushort addr, byte data) { throw new NotImplementedException(); }
        private void writew(ushort addr, ushort data) { throw new NotImplementedException(); }
        private void writed(ushort addr, uint data) { throw new NotImplementedException(); }

        //opcodes
        private void op_01() { throw new NotImplementedException(); }
        private void op_02() { throw new NotImplementedException(); }
        private void op_03() { throw new NotImplementedException(); }
        private void op_04() { throw new NotImplementedException(); }
        private void op_05() { throw new NotImplementedException(); }
        private void op_06() { throw new NotImplementedException(); }
        private void op_07() { throw new NotImplementedException(); }
        private void op_08() { throw new NotImplementedException(); }

        private void op_01(short x0, short y0, ref short x1, ref short y1, ref short quadrant, ref short theta) { throw new NotImplementedException(); }
    }
}
