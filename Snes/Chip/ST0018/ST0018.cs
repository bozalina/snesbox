using System;
using System.Threading.Tasks;

namespace Snes
{
    partial class ST0018 : IMMIO
    {
        public static ST0018 st0018 = new ST0018();

        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public Task<byte> mmio_read(uint addr) { throw new NotImplementedException(); }
        public Task mmio_write(uint addr, byte data) { throw new NotImplementedException(); }

        public enum Mode { Waiting, BoardUpload }
        public Regs regs;

        public enum PieceID { Pawn = 0x00, Lance = 0x04, Knight = 0x08, Silver = 0x0c, Gold = 0x10, Rook = 0x14, Bishop = 0x18, King = 0x1c }
        public enum PieceFlag { PlayerA = 0x20, PlayerB = 0x40 }

        public byte[] board = new byte[9 * 9 + 16];

        private void op_board_upload() { throw new NotImplementedException(); }
        private void op_board_upload(byte data) { throw new NotImplementedException(); }
        private void op_b2() { throw new NotImplementedException(); }
        private void op_b3() { throw new NotImplementedException(); }
        private void op_b4() { throw new NotImplementedException(); }
        private void op_b5() { throw new NotImplementedException(); }
        private void op_query_chip() { throw new NotImplementedException(); }
    }
}
