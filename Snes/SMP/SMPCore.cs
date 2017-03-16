using System;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    abstract partial class SMPCore
    {
        public async Task<byte> op_readpc()
        {
            return await op_read(regs.pc++);
        }

        public async Task<byte> op_readstack()
        {
            return await op_read((ushort)(0x0100 | ++regs.sp.Array[regs.sp.Offset]));
        }

        public async Task op_writestack(byte data)
        {
            await op_write((ushort)(0x0100 | regs.sp.Array[regs.sp.Offset]--), data);
        }

        public async Task<byte> op_readaddr(ushort addr)
        {
            return await op_read(addr);
        }

        public async Task op_writeaddr(ushort addr, byte data)
        {
            await op_write(addr, data);
        }

        public async Task<byte> op_readdp(byte addr)
        {
            return await op_read((ushort)((Convert.ToUInt32(regs.p.p) << 8) + addr));
        }

        public async Task op_writedp(byte addr, byte data)
        {
            await op_write((ushort)((Convert.ToUInt32(regs.p.p) << 8) + addr), data);
        }

        public void disassemble_opcode(out string s, ushort addr)
        {
            byte opcode_table, op0, op1;
            ushort opw, opdp0, opdp1;

            s = string.Format("..{0:X4} ", addr);

            opcode_table = disassemble_read((ushort)(addr + 0));
            op0 = disassemble_read((ushort)(addr + 1));
            op1 = disassemble_read((ushort)(addr + 2));
            opw = (ushort)((op0) | (op1 << 8));
            opdp0 = (ushort)((Convert.ToUInt32(regs.p.p) << 8) + op0);
            opdp1 = (ushort)((Convert.ToUInt32(regs.p.p) << 8) + op1);

            s += "                       ";

            switch (opcode_table)
            {
                case 0x00:
                    s += string.Format("nop");
                    break;
                case 0x01:
                    s += string.Format("tcall 0");
                    break;
                case 0x02:
                    s += string.Format("set0  ${0:X3}", opdp0);
                    break;
                case 0x03:
                    s += string.Format("bbs0  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0x04:
                    s += string.Format("or    a,${0:X3}", opdp0);
                    break;
                case 0x05:
                    s += string.Format("or    a,${0:X4}", opw);
                    break;
                case 0x06:
                    s += string.Format("or    a,(x)");
                    break;
                case 0x07:
                    s += string.Format("or    a,(${0:X3}+x)", opdp0);
                    break;
                case 0x08:
                    s += string.Format("or    a,#${0:X2}", op0);
                    break;
                case 0x09:
                    s += string.Format("or    ${0:X3},${1:X3}", opdp1, opdp0);
                    break;
                case 0x0a:
                    s += string.Format("or1   c,${0:X4}:%d", opw & 0x1fff, opw >> 13);
                    break;
                case 0x0b:
                    s += string.Format("asl   ${0:X3}", opdp0);
                    break;
                case 0x0c:
                    s += string.Format("asl   ${0:X4}", opw);
                    break;
                case 0x0d:
                    s += string.Format("push  p");
                    break;
                case 0x0e:
                    s += string.Format("tset  ${0:X4},a", opw);
                    break;
                case 0x0f:
                    s += string.Format("brk");
                    break;
                case 0x10:
                    s += string.Format("bpl   ${0:X4}", relb(op0, 2));
                    break;
                case 0x11:
                    s += string.Format("tcall 1");
                    break;
                case 0x12:
                    s += string.Format("clr0  ${0:X3}", opdp0);
                    break;
                case 0x13:
                    s += string.Format("bbc0  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0x14:
                    s += string.Format("or    a,${0:X3}+x", opdp0);
                    break;
                case 0x15:
                    s += string.Format("or    a,${0:X4}+x", opw);
                    break;
                case 0x16:
                    s += string.Format("or    a,${0:X4}+y", opw);
                    break;
                case 0x17:
                    s += string.Format("or    a,(${0:X3})+y", opdp0);
                    break;
                case 0x18:
                    s += string.Format("or    ${0:X3},#${1:X2}", opdp1, op0);
                    break;
                case 0x19:
                    s += string.Format("or    (x),(y)");
                    break;
                case 0x1a:
                    s += string.Format("decw  ${0:X3}", opdp0);
                    break;
                case 0x1b:
                    s += string.Format("asl   ${0:X3}+x", opdp0);
                    break;
                case 0x1c:
                    s += string.Format("asl   a");
                    break;
                case 0x1d:
                    s += string.Format("dec   x");
                    break;
                case 0x1e:
                    s += string.Format("cmp   x,${0:X4}", opw);
                    break;
                case 0x1f:
                    s += string.Format("jmp   (${0:X4}+x)", opw);
                    break;
                case 0x20:
                    s += string.Format("clrp");
                    break;
                case 0x21:
                    s += string.Format("tcall 2");
                    break;
                case 0x22:
                    s += string.Format("set1  ${0:X3}", opdp0);
                    break;
                case 0x23:
                    s += string.Format("bbs1  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0x24:
                    s += string.Format("and   a,${0:X3}", opdp0);
                    break;
                case 0x25:
                    s += string.Format("and   a,${0:X4}", opw);
                    break;
                case 0x26:
                    s += string.Format("and   a,(x)");
                    break;
                case 0x27:
                    s += string.Format("and   a,(${0:X3}+x)", opdp0);
                    break;
                case 0x28:
                    s += string.Format("and   a,#${0:X2}", op0);
                    break;
                case 0x29:
                    s += string.Format("and   ${0:X3},${1:X3}", opdp1, opdp0);
                    break;
                case 0x2a:
                    s += string.Format("or1   c,!${0:X4}:%d", opw & 0x1fff, opw >> 13);
                    break;
                case 0x2b:
                    s += string.Format("rol   ${0:X3}", opdp0);
                    break;
                case 0x2c:
                    s += string.Format("rol   ${0:X4}", opw);
                    break;
                case 0x2d:
                    s += string.Format("push  a");
                    break;
                case 0x2e:
                    s += string.Format("cbne  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0x2f:
                    s += string.Format("bra   ${0:X4}", relb(op0, 2));
                    break;
                case 0x30:
                    s += string.Format("bmi   ${0:X4}", relb(op0, 2));
                    break;
                case 0x31:
                    s += string.Format("tcall 3");
                    break;
                case 0x32:
                    s += string.Format("clr1  ${0:X3}", opdp0);
                    break;
                case 0x33:
                    s += string.Format("bbc1  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0x34:
                    s += string.Format("and   a,${0:X3}+x", opdp0);
                    break;
                case 0x35:
                    s += string.Format("and   a,${0:X4}+x", opw);
                    break;
                case 0x36:
                    s += string.Format("and   a,${0:X4}+y", opw);
                    break;
                case 0x37:
                    s += string.Format("and   a,(${0:X3})+y", opdp0);
                    break;
                case 0x38:
                    s += string.Format("and   ${0:X3},#${1:X2}", opdp1, op0);
                    break;
                case 0x39:
                    s += string.Format("and   (x),(y)");
                    break;
                case 0x3a:
                    s += string.Format("incw  ${0:X3}", opdp0);
                    break;
                case 0x3b:
                    s += string.Format("rol   ${0:X3}+x", opdp0);
                    break;
                case 0x3c:
                    s += string.Format("rol   a");
                    break;
                case 0x3d:
                    s += string.Format("inc   x");
                    break;
                case 0x3e:
                    s += string.Format("cmp   x,${0:X3}", opdp0);
                    break;
                case 0x3f:
                    s += string.Format("call  ${0:X4}", opw);
                    break;
                case 0x40:
                    s += string.Format("setp");
                    break;
                case 0x41:
                    s += string.Format("tcall 4");
                    break;
                case 0x42:
                    s += string.Format("set2  ${0:X3}", opdp0);
                    break;
                case 0x43:
                    s += string.Format("bbs2  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0x44:
                    s += string.Format("eor   a,${0:X3}", opdp0);
                    break;
                case 0x45:
                    s += string.Format("eor   a,${0:X4}", opw);
                    break;
                case 0x46:
                    s += string.Format("eor   a,(x)");
                    break;
                case 0x47:
                    s += string.Format("eor   a,(${0:X3}+x)", opdp0);
                    break;
                case 0x48:
                    s += string.Format("eor   a,#${0:X2}", op0);
                    break;
                case 0x49:
                    s += string.Format("eor   ${0:X3},${1:X3}", opdp1, opdp0);
                    break;
                case 0x4a:
                    s += string.Format("and1  c,${0:X4}:%d", opw & 0x1fff, opw >> 13);
                    break;
                case 0x4b:
                    s += string.Format("lsr   ${0:X3}", opdp0);
                    break;
                case 0x4c:
                    s += string.Format("lsr   ${0:X4}", opw);
                    break;
                case 0x4d:
                    s += string.Format("push  x");
                    break;
                case 0x4e:
                    s += string.Format("tclr  ${0:X4},a", opw);
                    break;
                case 0x4f:
                    s += string.Format("pcall $ff{0:X2}", op0);
                    break;
                case 0x50:
                    s += string.Format("bvc   ${0:X4}", relb(op0, 2));
                    break;
                case 0x51:
                    s += string.Format("tcall 5");
                    break;
                case 0x52:
                    s += string.Format("clr2  ${0:X3}", opdp0);
                    break;
                case 0x53:
                    s += string.Format("bbc2  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0x54:
                    s += string.Format("eor   a,${0:X3}+x", opdp0);
                    break;
                case 0x55:
                    s += string.Format("eor   a,${0:X4}+x", opw);
                    break;
                case 0x56:
                    s += string.Format("eor   a,${0:X4}+y", opw);
                    break;
                case 0x57:
                    s += string.Format("eor   a,(${0:X3})+y", opdp0);
                    break;
                case 0x58:
                    s += string.Format("eor   ${0:X3},#${1:X2}", opdp1, op0);
                    break;
                case 0x59:
                    s += string.Format("eor   (x),(y)");
                    break;
                case 0x5a:
                    s += string.Format("cmpw  ya,${0:X3}", opdp0);
                    break;
                case 0x5b:
                    s += string.Format("lsr   ${0:X3}+x", opdp0);
                    break;
                case 0x5c:
                    s += string.Format("lsr   a");
                    break;
                case 0x5d:
                    s += string.Format("mov   x,a");
                    break;
                case 0x5e:
                    s += string.Format("cmp   y,${0:X4}", opw);
                    break;
                case 0x5f:
                    s += string.Format("jmp   ${0:X4}", opw);
                    break;
                case 0x60:
                    s += string.Format("clrc");
                    break;
                case 0x61:
                    s += string.Format("tcall 6");
                    break;
                case 0x62:
                    s += string.Format("set3  ${0:X3}", opdp0);
                    break;
                case 0x63:
                    s += string.Format("bbs3  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0x64:
                    s += string.Format("cmp   a,${0:X3}", opdp0);
                    break;
                case 0x65:
                    s += string.Format("cmp   a,${0:X4}", opw);
                    break;
                case 0x66:
                    s += string.Format("cmp   a,(x)");
                    break;
                case 0x67:
                    s += string.Format("cmp   a,(${0:X3}+x)", opdp0);
                    break;
                case 0x68:
                    s += string.Format("cmp   a,#${0:X2}", op0);
                    break;
                case 0x69:
                    s += string.Format("cmp   ${0:X3},${1:X3}", opdp1, opdp0);
                    break;
                case 0x6a:
                    s += string.Format("and1  c,!${0:X4}:%d", opw & 0x1fff, opw >> 13);
                    break;
                case 0x6b:
                    s += string.Format("ror   ${0:X3}", opdp0);
                    break;
                case 0x6c:
                    s += string.Format("ror   ${0:X4}", opw);
                    break;
                case 0x6d:
                    s += string.Format("push  y");
                    break;
                case 0x6e:
                    s += string.Format("dbnz  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0x6f:
                    s += string.Format("ret");
                    break;
                case 0x70:
                    s += string.Format("bvs   ${0:X4}", relb(op0, 2));
                    break;
                case 0x71:
                    s += string.Format("tcall 7");
                    break;
                case 0x72:
                    s += string.Format("clr3  ${0:X3}", opdp0);
                    break;
                case 0x73:
                    s += string.Format("bbc3  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0x74:
                    s += string.Format("cmp   a,${0:X3}+x", opdp0);
                    break;
                case 0x75:
                    s += string.Format("cmp   a,${0:X4}+x", opw);
                    break;
                case 0x76:
                    s += string.Format("cmp   a,${0:X4}+y", opw);
                    break;
                case 0x77:
                    s += string.Format("cmp   a,(${0:X3})+y", opdp0);
                    break;
                case 0x78:
                    s += string.Format("cmp   ${0:X3},#${1:X2}", opdp1, op0);
                    break;
                case 0x79:
                    s += string.Format("cmp   (x),(y)");
                    break;
                case 0x7a:
                    s += string.Format("addw  ya,${0:X3}", opdp0);
                    break;
                case 0x7b:
                    s += string.Format("ror   ${0:X3}+x", opdp0);
                    break;
                case 0x7c:
                    s += string.Format("ror   a");
                    break;
                case 0x7d:
                    s += string.Format("mov   a,x");
                    break;
                case 0x7e:
                    s += string.Format("cmp   y,${0:X3}", opdp0);
                    break;
                case 0x7f:
                    s += string.Format("reti");
                    break;
                case 0x80:
                    s += string.Format("setc");
                    break;
                case 0x81:
                    s += string.Format("tcall 8");
                    break;
                case 0x82:
                    s += string.Format("set4  ${0:X3}", opdp0);
                    break;
                case 0x83:
                    s += string.Format("bbs4  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0x84:
                    s += string.Format("adc   a,${0:X3}", opdp0);
                    break;
                case 0x85:
                    s += string.Format("adc   a,${0:X4}", opw);
                    break;
                case 0x86:
                    s += string.Format("adc   a,(x)");
                    break;
                case 0x87:
                    s += string.Format("adc   a,(${0:X3}+x)", opdp0);
                    break;
                case 0x88:
                    s += string.Format("adc   a,#${0:X2}", op0);
                    break;
                case 0x89:
                    s += string.Format("adc   ${0:X3},${1:X3}", opdp1, opdp0);
                    break;
                case 0x8a:
                    s += string.Format("eor1  c,${0:X4}:%d", opw & 0x1fff, opw >> 13);
                    break;
                case 0x8b:
                    s += string.Format("dec   ${0:X3}", opdp0);
                    break;
                case 0x8c:
                    s += string.Format("dec   ${0:X4}", opw);
                    break;
                case 0x8d:
                    s += string.Format("mov   y,#${0:X2}", op0);
                    break;
                case 0x8e:
                    s += string.Format("pop   p");
                    break;
                case 0x8f:
                    s += string.Format("mov   ${0:X3},#${1:X2}", opdp1, op0);
                    break;
                case 0x90:
                    s += string.Format("bcc   ${0:X4}", relb(op0, 2));
                    break;
                case 0x91:
                    s += string.Format("tcall 9");
                    break;
                case 0x92:
                    s += string.Format("clr4  ${0:X3}", opdp0);
                    break;
                case 0x93:
                    s += string.Format("bbc4  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0x94:
                    s += string.Format("adc   a,${0:X3}+x", opdp0);
                    break;
                case 0x95:
                    s += string.Format("adc   a,${0:X4}+x", opw);
                    break;
                case 0x96:
                    s += string.Format("adc   a,${0:X4}+y", opw);
                    break;
                case 0x97:
                    s += string.Format("adc   a,(${0:X3})+y", opdp0);
                    break;
                case 0x98:
                    s += string.Format("adc   ${0:X3},#${1:X2}", opdp1, op0);
                    break;
                case 0x99:
                    s += string.Format("adc   (x),(y)");
                    break;
                case 0x9a:
                    s += string.Format("subw  ya,${0:X3}", opdp0);
                    break;
                case 0x9b:
                    s += string.Format("dec   ${0:X3}+x", opdp0);
                    break;
                case 0x9c:
                    s += string.Format("dec   a");
                    break;
                case 0x9d:
                    s += string.Format("mov   x,sp");
                    break;
                case 0x9e:
                    s += string.Format("div   ya,x");
                    break;
                case 0x9f:
                    s += string.Format("xcn   a");
                    break;
                case 0xa0:
                    s += string.Format("ei");
                    break;
                case 0xa1:
                    s += string.Format("tcall 10");
                    break;
                case 0xa2:
                    s += string.Format("set5  ${0:X3}", opdp0);
                    break;
                case 0xa3:
                    s += string.Format("bbs5  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0xa4:
                    s += string.Format("sbc   a,${0:X3}", opdp0);
                    break;
                case 0xa5:
                    s += string.Format("sbc   a,${0:X4}", opw);
                    break;
                case 0xa6:
                    s += string.Format("sbc   a,(x)");
                    break;
                case 0xa7:
                    s += string.Format("sbc   a,(${0:X3}+x)", opdp0);
                    break;
                case 0xa8:
                    s += string.Format("sbc   a,#${0:X2}", op0);
                    break;
                case 0xa9:
                    s += string.Format("sbc   ${0:X3},${1:X3}", opdp1, opdp0);
                    break;
                case 0xaa:
                    s += string.Format("mov1  c,${0:X4}:%d", opw & 0x1fff, opw >> 13);
                    break;
                case 0xab:
                    s += string.Format("inc   ${0:X3}", opdp0);
                    break;
                case 0xac:
                    s += string.Format("inc   ${0:X4}", opw);
                    break;
                case 0xad:
                    s += string.Format("cmp   y,#${0:X2}", op0);
                    break;
                case 0xae:
                    s += string.Format("pop   a");
                    break;
                case 0xaf:
                    s += string.Format("mov   (x)+,a");
                    break;
                case 0xb0:
                    s += string.Format("bcs   ${0:X4}", relb(op0, 2));
                    break;
                case 0xb1:
                    s += string.Format("tcall 11");
                    break;
                case 0xb2:
                    s += string.Format("clr5  ${0:X3}", opdp0);
                    break;
                case 0xb3:
                    s += string.Format("bbc5  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0xb4:
                    s += string.Format("sbc   a,${0:X3}+x", opdp0);
                    break;
                case 0xb5:
                    s += string.Format("sbc   a,${0:X4}+x", opw);
                    break;
                case 0xb6:
                    s += string.Format("sbc   a,${0:X4}+y", opw);
                    break;
                case 0xb7:
                    s += string.Format("sbc   a,(${0:X3})+y", opdp0);
                    break;
                case 0xb8:
                    s += string.Format("sbc   ${0:X3},#${1:X2}", opdp1, op0);
                    break;
                case 0xb9:
                    s += string.Format("sbc   (x),(y)");
                    break;
                case 0xba:
                    s += string.Format("movw  ya,${0:X3}", opdp0);
                    break;
                case 0xbb:
                    s += string.Format("inc   ${0:X3}+x", opdp0);
                    break;
                case 0xbc:
                    s += string.Format("inc   a");
                    break;
                case 0xbd:
                    s += string.Format("mov   sp,x");
                    break;
                case 0xbe:
                    s += string.Format("das   a");
                    break;
                case 0xbf:
                    s += string.Format("mov   a,(x)+");
                    break;
                case 0xc0:
                    s += string.Format("di");
                    break;
                case 0xc1:
                    s += string.Format("tcall 12");
                    break;
                case 0xc2:
                    s += string.Format("set6  ${0:X3}", opdp0);
                    break;
                case 0xc3:
                    s += string.Format("bbs6  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0xc4:
                    s += string.Format("mov   ${0:X3},a", opdp0);
                    break;
                case 0xc5:
                    s += string.Format("mov   ${0:X4},a", opw);
                    break;
                case 0xc6:
                    s += string.Format("mov   (x),a");
                    break;
                case 0xc7:
                    s += string.Format("mov   (${0:X3}+x),a", opdp0);
                    break;
                case 0xc8:
                    s += string.Format("cmp   x,#${0:X2}", op0);
                    break;
                case 0xc9:
                    s += string.Format("mov   ${0:X4},x", opw);
                    break;
                case 0xca:
                    s += string.Format("mov1  ${0:X4}:%d,c", opw & 0x1fff, opw >> 13);
                    break;
                case 0xcb:
                    s += string.Format("mov   ${0:X3},y", opdp0);
                    break;
                case 0xcc:
                    s += string.Format("mov   ${0:X4},y", opw);
                    break;
                case 0xcd:
                    s += string.Format("mov   x,#${0:X2}", op0);
                    break;
                case 0xce:
                    s += string.Format("pop   x");
                    break;
                case 0xcf:
                    s += string.Format("mul   ya");
                    break;
                case 0xd0:
                    s += string.Format("bne   ${0:X4}", relb(op0, 2));
                    break;
                case 0xd1:
                    s += string.Format("tcall 13");
                    break;
                case 0xd2:
                    s += string.Format("clr6  ${0:X3}", opdp0);
                    break;
                case 0xd3:
                    s += string.Format("bbc6  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0xd4:
                    s += string.Format("mov   ${0:X3}+x,a", opdp0);
                    break;
                case 0xd5:
                    s += string.Format("mov   ${0:X4}+x,a", opw);
                    break;
                case 0xd6:
                    s += string.Format("mov   ${0:X4}+y,a", opw);
                    break;
                case 0xd7:
                    s += string.Format("mov   (${0:X3})+y,a", opdp0);
                    break;
                case 0xd8:
                    s += string.Format("mov   ${0:X3},x", opdp0);
                    break;
                case 0xd9:
                    s += string.Format("mov   ${0:X3}+y,x", opdp0);
                    break;
                case 0xda:
                    s += string.Format("movw  ${0:X3},ya", opdp0);
                    break;
                case 0xdb:
                    s += string.Format("mov   ${0:X3}+x,y", opdp0);
                    break;
                case 0xdc:
                    s += string.Format("dec   y");
                    break;
                case 0xdd:
                    s += string.Format("mov   a,y");
                    break;
                case 0xde:
                    s += string.Format("cbne  ${0:X3}+x,${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0xdf:
                    s += string.Format("daa   a");
                    break;
                case 0xe0:
                    s += string.Format("clrv");
                    break;
                case 0xe1:
                    s += string.Format("tcall 14");
                    break;
                case 0xe2:
                    s += string.Format("set7  ${0:X3}", opdp0);
                    break;
                case 0xe3:
                    s += string.Format("bbs7  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0xe4:
                    s += string.Format("mov   a,${0:X3}", opdp0);
                    break;
                case 0xe5:
                    s += string.Format("mov   a,${0:X4}", opw);
                    break;
                case 0xe6:
                    s += string.Format("mov   a,(x)");
                    break;
                case 0xe7:
                    s += string.Format("mov   a,(${0:X3}+x)", opdp0);
                    break;
                case 0xe8:
                    s += string.Format("mov   a,#${0:X2}", op0);
                    break;
                case 0xe9:
                    s += string.Format("mov   x,${0:X4}", opw);
                    break;
                case 0xea:
                    s += string.Format("not1  c,${0:X4}:%d", opw & 0x1fff, opw >> 13);
                    break;
                case 0xeb:
                    s += string.Format("mov   y,${0:X3}", opdp0);
                    break;
                case 0xec:
                    s += string.Format("mov   y,${0:X4}", opw);
                    break;
                case 0xed:
                    s += string.Format("notc");
                    break;
                case 0xee:
                    s += string.Format("pop   y");
                    break;
                case 0xef:
                    s += string.Format("sleep");
                    break;
                case 0xf0:
                    s += string.Format("beq   ${0:X4}", relb(op0, 2));
                    break;
                case 0xf1:
                    s += string.Format("tcall 15");
                    break;
                case 0xf2:
                    s += string.Format("clr7  ${0:X3}", opdp0);
                    break;
                case 0xf3:
                    s += string.Format("bbc7  ${0:X3},${1:X4}", opdp0, relb(op1, 3));
                    break;
                case 0xf4:
                    s += string.Format("mov   a,${0:X3}+x", opdp0);
                    break;
                case 0xf5:
                    s += string.Format("mov   a,${0:X4}+x", opw);
                    break;
                case 0xf6:
                    s += string.Format("mov   a,${0:X4}+y", opw);
                    break;
                case 0xf7:
                    s += string.Format("mov   a,(${0:X3})+y", opdp0);
                    break;
                case 0xf8:
                    s += string.Format("mov   x,${0:X3}", opdp0);
                    break;
                case 0xf9:
                    s += string.Format("mov   x,${0:X3}+y", opdp0);
                    break;
                case 0xfa:
                    s += string.Format("mov   ${0:X3},${1:X3}", opdp1, opdp0);
                    break;
                case 0xfb:
                    s += string.Format("mov   y,${0:X3}+x", opdp0);
                    break;
                case 0xfc:
                    s += string.Format("inc   y");
                    break;
                case 0xfd:
                    s += string.Format("mov   y,a");
                    break;
                case 0xfe:
                    s += string.Format("dbnz  y,${0:X4}", relb(op0, 2));
                    break;
                case 0xff:
                    s += string.Format("stop");
                    break;
            }

            s += ' ';

            s += string.Format("A:{0:X2} X:{1:X2} Y:{2:X2} SP:01{3:X2} YA:{0:X4} ",
              regs.a.Array[regs.a.Offset], regs.x.Array[regs.x.Offset], regs.y.Array[regs.y.Offset], regs.sp.Array[regs.sp.Offset], (ushort)regs.ya);

            s += string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
              regs.p.n ? 'N' : 'n',
              regs.p.v ? 'V' : 'v',
              regs.p.p ? 'P' : 'p',
              regs.p.b ? 'B' : 'b',
              regs.p.h ? 'H' : 'h',
              regs.p.i ? 'I' : 'i',
              regs.p.z ? 'Z' : 'z',
              regs.p.c ? 'C' : 'c');
        }

        public byte disassemble_read(ushort addr)
        {
            if (addr >= 0xffc0)
            {
                return SMP.Iplrom[addr & 0x3f];
            }
            return StaticRAM.apuram[addr];
        }

        public ushort relb(byte offset, int op_len)
        {
            ushort pc = (ushort)(regs.pc + op_len);
            return (ushort)(pc + offset);
        }

        public Regs regs = new Regs();
        public ushort dp, sp, rd, wr, bit, ya;
        public enum OpCode { A = 0, X = 1, Y = 2, SP = 3 };

        public abstract Task op_io();
        public abstract Task<byte> op_read(ushort addr);
        public abstract Task op_write(ushort addr, byte data);

        public Task<SMPCoreOpResult> op_adc(SMPCoreOpArgument args)
        {
            int r = args.x_byte + args.y_byte + Convert.ToInt32(regs.p.c);
            regs.p.n = Convert.ToBoolean(r & 0x80);
            regs.p.v = Convert.ToBoolean(~(args.x_byte ^ args.y_byte) & (args.x_byte ^ r) & 0x80);
            regs.p.h = Convert.ToBoolean((args.x_byte ^ args.y_byte ^ r) & 0x10);
            regs.p.z = (byte)r == 0;
            regs.p.c = r > 0xff;
            return Task.FromResult(new SMPCoreOpResult() { result_byte = (byte)r });
        }

        public async Task<SMPCoreOpResult> op_addw(SMPCoreOpArgument args)
        {
            ushort r;
            regs.p.c = Convert.ToBoolean(0);
            r = (await op_adc(new SMPCoreOpArgument() { x_byte = (byte)args.x_ushort, y_byte = (byte)args.y_ushort })).result_byte;
            r |= (ushort)((await op_adc(new SMPCoreOpArgument() { x_byte = (byte)(args.x_ushort >> 8), y_byte = (byte)(args.y_ushort >> 8) })).result_byte << 8);
            regs.p.z = r == 0;
            return new SMPCoreOpResult() { result_ushort = r };
        }

        public Task<SMPCoreOpResult> op_and(SMPCoreOpArgument args)
        {
            args.x_byte &= args.y_byte;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            return Task.FromResult(new SMPCoreOpResult() { result_byte = args.x_byte });
        }

        public Task<SMPCoreOpResult> op_cmp(SMPCoreOpArgument args)
        {
            int r = args.x_byte - args.y_byte;
            regs.p.n = Convert.ToBoolean(r & 0x80);
            regs.p.z = (byte)r == 0;
            regs.p.c = r >= 0;
            return Task.FromResult(new SMPCoreOpResult() { result_byte = args.x_byte });
        }

        public Task<SMPCoreOpResult> op_cmpw(SMPCoreOpArgument args)
        {
            int r = args.x_ushort - args.y_ushort;
            regs.p.n = Convert.ToBoolean(r & 0x8000);
            regs.p.z = (ushort)r == 0;
            regs.p.c = r >= 0;
            return Task.FromResult(new SMPCoreOpResult() { result_ushort = args.x_ushort });
        }

        public Task<SMPCoreOpResult> op_eor(SMPCoreOpArgument args)
        {
            args.x_byte ^= args.y_byte;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            return Task.FromResult(new SMPCoreOpResult() { result_byte = args.x_byte });
        }

        public Task<SMPCoreOpResult> op_inc(SMPCoreOpArgument args)
        {
            args.x_byte++;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            return Task.FromResult(new SMPCoreOpResult() { result_byte = args.x_byte });
        }

        public Task<SMPCoreOpResult> op_dec(SMPCoreOpArgument args)
        {
            args.x_byte--;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            return Task.FromResult(new SMPCoreOpResult() { result_byte = args.x_byte });
        }

        public Task<SMPCoreOpResult> op_or(SMPCoreOpArgument args)
        {
            args.x_byte |= args.y_byte;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            return Task.FromResult(new SMPCoreOpResult() { result_byte = args.x_byte });
        }

        public Task<SMPCoreOpResult> op_sbc(SMPCoreOpArgument args)
        {
            int r = args.x_byte - args.y_byte - Convert.ToInt32(!regs.p.c);
            regs.p.n = Convert.ToBoolean(r & 0x80);
            regs.p.v = Convert.ToBoolean((args.x_byte ^ args.y_byte) & (args.x_byte ^ r) & 0x80);
            regs.p.h = !Convert.ToBoolean(((args.x_byte ^ args.y_byte ^ r) & 0x10));
            regs.p.z = (byte)r == 0;
            regs.p.c = r >= 0;
            return Task.FromResult(new SMPCoreOpResult() { result_byte = (byte)r });
        }

        public async Task<SMPCoreOpResult> op_subw(SMPCoreOpArgument args)
        {
            ushort r;
            regs.p.c = Convert.ToBoolean(1);
            r = (await op_sbc(new SMPCoreOpArgument() { x_byte = (byte)args.x_ushort, y_byte = (byte)args.y_ushort })).result_byte;
            r |= (ushort)((await op_sbc(new SMPCoreOpArgument() { x_byte = (byte)(args.x_ushort >> 8), y_byte = (byte)(args.y_ushort >> 8) })).result_byte << 8);
            regs.p.z = r == 0;
            return new SMPCoreOpResult() { result_ushort = r };
        }

        public Task<SMPCoreOpResult> op_asl(SMPCoreOpArgument args)
        {
            regs.p.c = Convert.ToBoolean(args.x_byte & 0x80);
            args.x_byte <<= 1;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            return Task.FromResult(new SMPCoreOpResult() { result_byte = args.x_byte });
        }

        public Task<SMPCoreOpResult> op_lsr(SMPCoreOpArgument args)
        {
            regs.p.c = Convert.ToBoolean(args.x_byte & 0x01);
            args.x_byte >>= 1;
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            return Task.FromResult(new SMPCoreOpResult() { result_byte = args.x_byte });
        }

        public Task<SMPCoreOpResult> op_rol(SMPCoreOpArgument args)
        {
            uint carry = Convert.ToUInt32(regs.p.c);
            regs.p.c = Convert.ToBoolean(args.x_byte & 0x80);
            args.x_byte = (byte)((uint)(args.x_byte << 1) | carry);
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            return Task.FromResult(new SMPCoreOpResult() { result_byte = args.x_byte });
        }

        public Task<SMPCoreOpResult> op_ror(SMPCoreOpArgument args)
        {
            uint carry = Convert.ToUInt32(regs.p.c) << 7;
            regs.p.c = Convert.ToBoolean(args.x_byte & 0x01);
            args.x_byte = (byte)(carry | (uint)(args.x_byte >> 1));
            regs.p.n = Convert.ToBoolean(args.x_byte & 0x80);
            regs.p.z = args.x_byte == 0;
            return Task.FromResult(new SMPCoreOpResult() { result_byte = args.x_byte });
        }

        public async Task<SMPCoreOpResult> op_mov_reg_reg(SMPCoreOpArgument args)
        {
			await op_io();
            regs.r[args.to] = regs.r[args.from];
            regs.p.n = Convert.ToBoolean(regs.r[args.to] & 0x80);
            regs.p.z = (regs.r[args.to] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_sp_x(SMPCoreOpArgument args)
        {
			await op_io();
            regs.sp.Array[regs.sp.Offset] = regs.x.Array[regs.x.Offset];
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_reg_const(SMPCoreOpArgument args)
        {
            regs.r[args.n] = await op_readpc();
            regs.p.n = Convert.ToBoolean(regs.r[args.n] & 0x80);
            regs.p.z = (regs.r[args.n] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_a_ix(SMPCoreOpArgument args)
        {
			await op_io();
            regs.a.Array[regs.a.Offset] = await op_readdp(regs.x.Array[regs.x.Offset]);
            regs.p.n = Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80);
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_a_ixinc(SMPCoreOpArgument args)
        {
			await op_io();
            regs.a.Array[regs.a.Offset] = await op_readdp(regs.x.Array[regs.x.Offset]++);
			await op_io();
            regs.p.n = Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80);
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_reg_dp(SMPCoreOpArgument args)
        {
            sp = await op_readpc();
            regs.r[args.n] = await op_readdp((byte)sp);
            regs.p.n = Convert.ToBoolean(regs.r[args.n] & 0x80);
            regs.p.z = (regs.r[args.n] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_reg_dpr(SMPCoreOpArgument args)
        {
            sp = await op_readpc();
			await op_io();
            regs.r[args.n] = await op_readdp((byte)(sp + regs.r[args.i]));
            regs.p.n = Convert.ToBoolean(regs.r[args.n] & 0x80);
            regs.p.z = (regs.r[args.n] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_reg_addr(SMPCoreOpArgument args)
        {
            sp = (ushort)(await op_readpc() << 0);
            sp |= (ushort)(await op_readpc() << 8);
            regs.r[args.n] = await op_readaddr(sp);
            regs.p.n = Convert.ToBoolean(regs.r[args.n] & 0x80);
            regs.p.z = (regs.r[args.n] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_a_addrr(SMPCoreOpArgument args)
        {
            sp = (ushort)(await op_readpc() << 0);
            sp |= (ushort)(await op_readpc() << 8);
			await op_io();
            regs.a.Array[regs.a.Offset] = await op_readaddr((ushort)(sp + regs.r[args.i]));
            regs.p.n = Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80);
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_a_idpx(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() + regs.x.Array[regs.x.Offset]);
			await op_io();
            sp = (ushort)(await op_readdp((byte)(dp + 0)) << 0);
            sp |= (ushort)(await op_readdp((byte)(dp + 1)) << 8);
            regs.a.Array[regs.a.Offset] = await op_readaddr(sp);
            regs.p.n = Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80);
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_a_idpy(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io();
            sp = (ushort)(await op_readdp((byte)(dp + 0)) << 0);
            sp |= (ushort)(await op_readdp((byte)(dp + 1)) << 8);
            regs.a.Array[regs.a.Offset] = await op_readaddr((ushort)(sp + regs.y.Array[regs.y.Offset]));
            regs.p.n = Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80);
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_dp_dp(SMPCoreOpArgument args)
        {
            sp = await op_readpc();
            rd = await op_readdp((byte)sp);
            dp = await op_readpc();
			await op_writedp((byte)dp, (byte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_dp_const(SMPCoreOpArgument args)
        {
            rd = await op_readpc();
            dp = await op_readpc();
			await op_readdp((byte)dp);
			await op_writedp((byte)dp, (byte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_ix_a(SMPCoreOpArgument args)
        {
			await op_io();
			await op_readdp(regs.x.Array[regs.x.Offset]);
			await op_writedp(regs.x.Array[regs.x.Offset], regs.a.Array[regs.a.Offset]);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_ixinc_a(SMPCoreOpArgument args)
        {
			await op_io();
			await op_io();
			await op_writedp(regs.x.Array[regs.x.Offset]++, regs.a.Array[regs.a.Offset]);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_dp_reg(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_readdp((byte)dp);
			await op_writedp((byte)dp, regs.r[args.n]);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_dpr_reg(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io();
            dp += regs.r[args.i];
			await op_readdp((byte)dp);
			await op_writedp((byte)dp, regs.r[args.n]);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_addr_reg(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() << 0);
            dp |= (ushort)(await op_readpc() << 8);
			await op_readaddr(dp);
			await op_writeaddr(dp, regs.r[args.n]);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_addrr_a(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() << 0);
            dp |= (ushort)(await op_readpc() << 8);
			await op_io();
            dp += regs.r[args.i];
			await op_readaddr(dp);
			await op_writeaddr(dp, regs.a.Array[regs.a.Offset]);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_idpx_a(SMPCoreOpArgument args)
        {
            sp = await op_readpc();
			await op_io();
            sp += regs.x.Array[regs.x.Offset];
            dp = (ushort)(await op_readdp((byte)(sp + 0)) << 0);
            dp |= (ushort)(await op_readdp((byte)(sp + 1)) << 8);
			await op_readaddr(dp);
			await op_writeaddr(dp, regs.a.Array[regs.a.Offset]);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov_idpy_a(SMPCoreOpArgument args)
        {
            sp = await op_readpc();
            dp = (ushort)(await op_readdp((byte)(sp + 0)) << 0);
            dp |= (ushort)(await op_readdp((byte)(sp + 1)) << 8);
			await op_io();
            dp += regs.y.Array[regs.y.Offset];
			await op_readaddr(dp);
			await op_writeaddr(dp, regs.a.Array[regs.a.Offset]);
            return null;
        }

        public async Task<SMPCoreOpResult> op_movw_ya_dp(SMPCoreOpArgument args)
        {
            sp = await op_readpc();
            regs.a.Array[regs.a.Offset] = await op_readdp((byte)(sp + 0));
			await op_io();
            regs.y.Array[regs.y.Offset] = await op_readdp((byte)(sp + 1));
            regs.p.n = Convert.ToBoolean((ushort)regs.ya & 0x8000);
            regs.p.z = ((ushort)regs.ya == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_movw_dp_ya(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_readdp((byte)dp);
			await op_writedp((byte)(dp + 0), regs.a.Array[regs.a.Offset]);
			await op_writedp((byte)(dp + 1), regs.y.Array[regs.y.Offset]);
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov1_c_bit(SMPCoreOpArgument args)
        {
            sp = (ushort)(await op_readpc() << 0);
            sp |= (ushort)(await op_readpc() << 8);
            bit = (ushort)(sp >> 13);
            sp &= 0x1fff;
            rd = await op_readaddr(sp);
            regs.p.c = Convert.ToBoolean(rd & (1 << bit));
            return null;
        }

        public async Task<SMPCoreOpResult> op_mov1_bit_c(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() << 0);
            dp |= (ushort)(await op_readpc() << 8);
            bit = (ushort)(dp >> 13);
            dp &= 0x1fff;
            rd = await op_readaddr(dp);
            if (regs.p.c)
            {
                rd |= (ushort)(1 << bit);
            }
            else
            {
                rd &= (ushort)(~(1 << bit));
            }
			await op_io();
			await op_writeaddr((byte)dp, (byte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_bra(SMPCoreOpArgument args)
        {
            rd = await op_readpc();
			await op_io();
			await op_io();
            regs.pc += (ushort)((sbyte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_branch(SMPCoreOpArgument args)
        {
            rd = await op_readpc();
            if (Convert.ToInt32(Convert.ToBoolean((uint)regs.p & args.flag)) != args.value)
            {
                return null;
            }
			await op_io();
			await op_io();
            regs.pc += (ushort)((sbyte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_bitbranch(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
            sp = await op_readdp((byte)dp);
            rd = await op_readpc();
			await op_io();
            if (Convert.ToInt32(Convert.ToBoolean(sp & args.mask)) != args.value)
            {
                return null;
            }
			await op_io();
			await op_io();
            regs.pc += (ushort)((sbyte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_cbne_dp(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
            sp = await op_readdp((byte)dp);
            rd = await op_readpc();
			await op_io();
            if (regs.a.Array[regs.a.Offset] == sp)
            {
                return null;
            }
			await op_io();
			await op_io();
            regs.pc += (ushort)((sbyte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_cbne_dpx(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io();
            sp = await op_readdp((byte)(dp + regs.x.Array[regs.x.Offset]));
            rd = await op_readpc();
			await op_io();
            if (regs.a.Array[regs.a.Offset] == sp)
            {
                return null;
            }
			await op_io();
			await op_io();
            regs.pc += (ushort)((sbyte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_dbnz_dp(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
            wr = await op_readdp((byte)dp);
			await op_writedp((byte)dp, (byte)--wr);
            rd = await op_readpc();
            if (wr == 0)
            {
                return null;
            }
            await op_io();
			await op_io();
            regs.pc += (ushort)((sbyte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_dbnz_y(SMPCoreOpArgument args)
        {
            rd = await op_readpc();
			await op_io();
            regs.y.Array[regs.y.Offset]--;
			await op_io();
            if (regs.y.Array[regs.y.Offset] == 0)
            {
                return null;
            }
			await op_io();
			await op_io();
            regs.pc += (ushort)((sbyte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_jmp_addr(SMPCoreOpArgument args)
        {
            rd = (ushort)(await op_readpc() << 0);
            rd |= (ushort)(await op_readpc() << 8);
            regs.pc = rd;
            return null;
        }

        public async Task<SMPCoreOpResult> op_jmp_iaddrx(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() << 0);
            dp |= (ushort)(await op_readpc() << 8);
			await op_io();
            dp += regs.x.Array[regs.x.Offset];
            rd = (ushort)(await op_readaddr((ushort)(dp + 0)) << 0);
            rd |= (ushort)(await op_readaddr((ushort)(dp + 1)) << 8);
            regs.pc = rd;
            return null;
        }

        public async Task<SMPCoreOpResult> op_call(SMPCoreOpArgument args)
        {
            rd = (ushort)(await op_readpc() << 0);
            rd |= (ushort)(await op_readpc() << 8);
			await op_io();
			await op_io();
			await op_io();
			await op_writestack((byte)(regs.pc >> 8));
			await op_writestack((byte)(regs.pc >> 0));
            regs.pc = rd;
            return null;
        }

        public async Task<SMPCoreOpResult> op_pcall(SMPCoreOpArgument args)
        {
            rd = await op_readpc();
			await op_io();
			await op_io();
			await op_writestack((byte)(regs.pc >> 8));
			await op_writestack((byte)(regs.pc >> 0));
            regs.pc = (ushort)(0xff00 | rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_tcall(SMPCoreOpArgument args)
        {
            dp = (ushort)(0xffde - (args.n << 1));
            rd = (ushort)(await op_readaddr((ushort)(dp + 0)) << 0);
            rd |= (ushort)(await op_readaddr((ushort)(dp + 1)) << 8);
			await op_io();
			await op_io();
			await op_io();
			await op_writestack((byte)(regs.pc >> 8));
			await op_writestack((byte)(regs.pc >> 0));
            regs.pc = rd;
            return null;
        }

        public async Task<SMPCoreOpResult> op_brk(SMPCoreOpArgument args)
        {
            rd = (ushort)(await op_readaddr(0xffde) << 0);
            rd |= (ushort)(await op_readaddr(0xffdf) << 8);
			await op_io();
			await op_io();
			await op_writestack((byte)(regs.pc >> 8));
			await op_writestack((byte)(regs.pc >> 0));
			await op_writestack((byte)(regs.p));
            regs.pc = rd;
            regs.p.b = Convert.ToBoolean(1);
            regs.p.i = Convert.ToBoolean(0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_ret(SMPCoreOpArgument args)
        {
            rd = (ushort)(await op_readstack() << 0);
            rd |= (ushort)(await op_readstack() << 8);
			await op_io();
			await op_io();
            regs.pc = rd;
            return null;
        }

        public async Task<SMPCoreOpResult> op_reti(SMPCoreOpArgument args)
        {
            regs.p.Assign(await op_readstack());
            rd = (ushort)(await op_readstack() << 0);
            rd |= (ushort)(await op_readstack() << 8);
			await op_io();
			await op_io();
            regs.pc = rd;
            return null;
        }

        public async Task<SMPCoreOpResult> op_read_reg_const(SMPCoreOpArgument args)
        {
            rd = await op_readpc();
            regs.r[args.n] = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = regs.r[args.n], y_byte = (byte)rd })).result_byte;
            return null;
        }

        public async Task<SMPCoreOpResult> op_read_a_ix(SMPCoreOpArgument args)
        {
			await op_io();
            rd = await op_readdp(regs.x.Array[regs.x.Offset]);
            regs.a.Array[regs.a.Offset] = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = regs.a.Array[regs.a.Offset], y_byte = (byte)rd })).result_byte;
            return null;
        }

        public async Task<SMPCoreOpResult> op_read_reg_dp(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
            rd = await op_readdp((byte)dp);
            regs.r[args.n] = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = regs.r[args.n], y_byte = (byte)rd })).result_byte;
            return null;
        }

        public async Task<SMPCoreOpResult> op_read_a_dpx(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io();
            rd = await op_readdp((byte)(dp + regs.x.Array[regs.x.Offset]));
            regs.a.Array[regs.a.Offset] = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = regs.a.Array[regs.a.Offset], y_byte = (byte)rd })).result_byte;
            return null;
        }

        public async Task<SMPCoreOpResult> op_read_reg_addr(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() << 0);
            dp |= (ushort)(await op_readpc() << 8);
            rd = await op_readaddr(dp);
            regs.r[args.n] = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = regs.r[args.n], y_byte = (byte)rd })).result_byte;
            return null;
        }

        public async Task<SMPCoreOpResult> op_read_a_addrr(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() << 0);
            dp |= (ushort)(await op_readpc() << 8);
			await op_io();
            rd = await op_readaddr((ushort)(dp + regs.r[args.i]));
            regs.a.Array[regs.a.Offset] = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = regs.a.Array[regs.a.Offset], y_byte = (byte)rd })).result_byte;
            return null;
        }

        public async Task<SMPCoreOpResult> op_read_a_idpx(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() + regs.x.Array[regs.x.Offset]);
			await op_io();
            sp = (ushort)(await op_readdp((byte)(dp + 0)) << 0);
            sp |= (ushort)(await op_readdp((byte)(dp + 1)) << 8);
            rd = await op_readaddr(sp);
            regs.a.Array[regs.a.Offset] = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = regs.a.Array[regs.a.Offset], y_byte = (byte)rd })).result_byte;
            return null;
        }

        public async Task<SMPCoreOpResult> op_read_a_idpy(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io();
            sp = (ushort)(await op_readdp((byte)(dp + 0)) << 0);
            sp |= (ushort)(await op_readdp((byte)(dp + 1)) << 8);
            rd = await op_readaddr((ushort)(sp + regs.y.Array[regs.y.Offset]));
            regs.a.Array[regs.a.Offset] = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = regs.a.Array[regs.a.Offset], y_byte = (byte)rd })).result_byte;
            return null;
        }

        public async Task<SMPCoreOpResult> op_read_ix_iy(SMPCoreOpArgument args)
        {
			await op_io();
            rd = await op_readdp(regs.y.Array[regs.y.Offset]);
            wr = await op_readdp(regs.x.Array[regs.x.Offset]);
            wr = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = (byte)wr, y_byte = (byte)rd })).result_byte;
            SMPCoreOp cmp = op_cmp;
            if (args.op_func != cmp)
            {
				await op_writedp(regs.x.Array[regs.x.Offset], (byte)wr);
            }
            else
            {
				await op_io();
            }
            return null;
        }

        public async Task<SMPCoreOpResult> op_read_dp_dp(SMPCoreOpArgument args)
        {
            sp = await op_readpc();
            rd = await op_readdp((byte)sp);
            dp = await op_readpc();
            wr = await op_readdp((byte)dp);
            wr = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = (byte)wr, y_byte = (byte)rd })).result_byte;
            SMPCoreOp cmp = op_cmp;
            if (args.op_func != cmp)
            {
				await op_writedp((byte)dp, (byte)wr);
            }
            else
            {
				await op_io();
            }
            return null;
        }

        public async Task<SMPCoreOpResult> op_read_dp_const(SMPCoreOpArgument args)
        {
            rd = await op_readpc();
            dp = await op_readpc();
            wr = await op_readdp((byte)dp);
            wr = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = (byte)wr, y_byte = (byte)rd })).result_byte;
            SMPCoreOp cmp = op_cmp;
            if (args.op_func != cmp)
            {
				await op_writedp((byte)dp, (byte)wr);
            }
            else
            {
				await op_io();
            }
            return null;
        }

        public async Task<SMPCoreOpResult> op_read_ya_dp(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
            rd = (ushort)(await op_readdp((byte)(dp + 0)) << 0);
			await op_io();
            rd |= (ushort)(await op_readdp((byte)(dp + 1)) << 8);
            regs.ya.Assign((await args.op_func.Invoke(new SMPCoreOpArgument() { x_ushort = (ushort)regs.ya, y_ushort = rd })).result_ushort);
            return null;
        }

        public async Task<SMPCoreOpResult> op_cmpw_ya_dp(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
            rd = (ushort)(await op_readdp((byte)(dp + 0)) << 0);
            rd |= (ushort)(await op_readdp((byte)(dp + 1)) << 8);
			await op_cmpw(new SMPCoreOpArgument() { x_ushort = (ushort)regs.ya, y_ushort = rd });
            return null;
        }

        public async Task<SMPCoreOpResult> op_and1_bit(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() << 0);
            dp |= (ushort)(await op_readpc() << 8);
            bit = (ushort)(dp >> 13);
            dp &= 0x1fff;
            rd = await op_readaddr(dp);
            regs.p.c = Convert.ToBoolean(Convert.ToInt32(regs.p.c) & (Convert.ToInt32(Convert.ToBoolean(rd & (1 << bit))) ^ args.op));
            return null;
        }

        public async Task<SMPCoreOpResult> op_eor1_bit(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() << 0);
            dp |= (ushort)(await op_readpc() << 8);
            bit = (ushort)(dp >> 13);
            dp &= 0x1fff;
            rd = await op_readaddr(dp);
			await op_io();
            regs.p.c = Convert.ToBoolean(Convert.ToInt32(regs.p.c) ^ Convert.ToInt32(Convert.ToBoolean(rd & (1 << bit))));
            return null;
        }

        public async Task<SMPCoreOpResult> op_not1_bit(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() << 0);
            dp |= (ushort)(await op_readpc() << 8);
            bit = (ushort)(dp >> 13);
            dp &= 0x1fff;
            rd = await op_readaddr(dp);
            rd ^= (ushort)(1 << bit);
			await op_writeaddr(dp, (byte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_or1_bit(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() << 0);
            dp |= (ushort)(await op_readpc() << 8);
            bit = (ushort)(dp >> 13);
            dp &= 0x1fff;
            rd = await op_readaddr(dp);
			await op_io();
            regs.p.c = Convert.ToBoolean(Convert.ToInt32(regs.p.c) | (Convert.ToInt32(Convert.ToBoolean(rd & (1 << bit))) ^ args.op));
            return null;
        }

        public async Task<SMPCoreOpResult> op_adjust_reg(SMPCoreOpArgument args)
        {
			await op_io();
            regs.r[args.n] = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = regs.r[args.n] })).result_byte;
            return null;
        }

        public async Task<SMPCoreOpResult> op_adjust_dp(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
            rd = await op_readdp((byte)dp);
            rd = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = (byte)rd })).result_byte;
			await op_writedp((byte)dp, (byte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_adjust_dpx(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io();
            rd = await op_readdp((byte)(dp + regs.x.Array[regs.x.Offset]));
            rd = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = (byte)rd })).result_byte;
			await op_writedp((byte)(dp + regs.x.Array[regs.x.Offset]), (byte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_adjust_addr(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() << 0);
            dp |= (ushort)(await op_readpc() << 8);
            rd = await op_readaddr(dp);
            rd = (await args.op_func.Invoke(new SMPCoreOpArgument() { x_byte = (byte)rd })).result_byte;
            await op_writeaddr(dp, (byte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_adjust_addr_a(SMPCoreOpArgument args)
        {
            dp = (ushort)(await op_readpc() << 0);
            dp |= (ushort)(await op_readpc() << 8);
            rd = await op_readaddr(dp);
            regs.p.n = Convert.ToBoolean((regs.a.Array[regs.a.Offset] - rd) & 0x80);
            regs.p.z = ((regs.a.Array[regs.a.Offset] - rd) == 0);
			await op_readaddr(dp);
			await op_writeaddr(dp, (byte)(Convert.ToBoolean(args.op) ? rd | regs.a.Array[regs.a.Offset] : rd & ~regs.a.Array[regs.a.Offset]));
            return null;
        }

        public async Task<SMPCoreOpResult> op_adjustw_dp(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
            rd = (ushort)(await op_readdp((byte)dp) << 0);
            rd += (ushort)args.adjust;
			await op_writedp((byte)(dp++), (byte)rd);
            rd += (ushort)(await op_readdp((byte)dp) << 8);
			await op_writedp((byte)dp, (byte)(rd >> 8));
            regs.p.n = Convert.ToBoolean(rd & 0x8000);
            regs.p.z = (rd == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_nop(SMPCoreOpArgument args)
        {
			await op_io();
            return null;
        }

        public async Task<SMPCoreOpResult> op_wait(SMPCoreOpArgument args)
        {
            while (true)
            {
				await op_io();
				await op_io();
            }
        }

        public async Task<SMPCoreOpResult> op_xcn(SMPCoreOpArgument args)
        {
			await op_io();
			await op_io();
			await op_io();
			await op_io();
            regs.a.Array[regs.a.Offset] = (byte)((regs.a.Array[regs.a.Offset] >> 4) | (regs.a.Array[regs.a.Offset] << 4));
            regs.p.n = Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80);
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_daa(SMPCoreOpArgument args)
        {
			await op_io();
			await op_io();
            if (regs.p.c || (regs.a.Array[regs.a.Offset]) > 0x99)
            {
                regs.a.Array[regs.a.Offset] += 0x60;
                regs.p.c = Convert.ToBoolean(1);
            }
            if (regs.p.h || (regs.a.Array[regs.a.Offset] & 15) > 0x09)
            {
                regs.a.Array[regs.a.Offset] += 0x06;
            }
            regs.p.n = Convert.ToBoolean(Convert.ToInt32(Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80)));
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_das(SMPCoreOpArgument args)
        {
			await op_io();
			await op_io();
            if (!regs.p.c || (regs.a.Array[regs.a.Offset]) > 0x99)
            {
                regs.a.Array[regs.a.Offset] -= 0x60;
                regs.p.c = Convert.ToBoolean(0);
            }
            if (!regs.p.h || (regs.a.Array[regs.a.Offset] & 15) > 0x09)
            {
                regs.a.Array[regs.a.Offset] -= 0x06;
            }
            regs.p.n = Convert.ToBoolean(Convert.ToInt32(Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80)));
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_setbit(SMPCoreOpArgument args)
        {
			await op_io();
            regs.p.Assign((byte)(((uint)regs.p & ~args.mask) | (uint)args.value));
            return null;
        }

        public async Task<SMPCoreOpResult> op_notc(SMPCoreOpArgument args)
        {
			await op_io();
			await op_io();
            regs.p.c = !regs.p.c;
            return null;
        }

        public async Task<SMPCoreOpResult> op_seti(SMPCoreOpArgument args)
        {
			await op_io();
			await op_io();
            regs.p.i = Convert.ToBoolean(args.value);
            return null;
        }

        public async Task<SMPCoreOpResult> op_setbit_dp(SMPCoreOpArgument args)
        {
            dp = await op_readpc();
            rd = await op_readdp((byte)dp);
            rd = (ushort)(Convert.ToBoolean(args.op) ? rd | args.value : rd & ~args.value);
			await op_writedp((byte)dp, (byte)rd);
            return null;
        }

        public async Task<SMPCoreOpResult> op_push_reg(SMPCoreOpArgument args)
        {
			await op_io();
			await op_io();
			await op_writestack(regs.r[args.n]);
            return null;
        }

        public async Task<SMPCoreOpResult> op_push_p(SMPCoreOpArgument args)
        {
			await op_io();
			await op_io();
			await op_writestack((byte)regs.p);
            return null;
        }

        public async Task<SMPCoreOpResult> op_pop_reg(SMPCoreOpArgument args)
        {
			await op_io();
			await op_io();
            regs.r[args.n] = await op_readstack();
            return null;
        }

        public async Task<SMPCoreOpResult> op_pop_p(SMPCoreOpArgument args)
        {
			await op_io();
			await op_io();
            regs.p.Assign(await op_readstack());
            return null;
        }

        public async Task<SMPCoreOpResult> op_mul_ya(SMPCoreOpArgument args)
        {
			await op_io();
			await op_io();
			await op_io();
			await op_io();
			await op_io();
			await op_io();
			await op_io();
			await op_io();
            ya = (ushort)(regs.y.Array[regs.y.Offset] * regs.a.Array[regs.a.Offset]);
            regs.a.Array[regs.a.Offset] = (byte)ya;
            regs.y.Array[regs.y.Offset] = (byte)(ya >> 8);
            //result is set based on y (high-byte) only
            regs.p.n = Convert.ToBoolean(Convert.ToInt32(Convert.ToBoolean(regs.y.Array[regs.y.Offset] & 0x80)));
            regs.p.z = (regs.y.Array[regs.y.Offset] == 0);
            return null;
        }

        public async Task<SMPCoreOpResult> op_div_ya_x(SMPCoreOpArgument args)
        {
			await op_io();
			await op_io();
			await op_io();
			await op_io();
			await op_io();
			await op_io();
			await op_io();
			await op_io();
			await op_io();
			await op_io();
			await op_io();
            ya = (ushort)regs.ya;
            //overflow set if quotient >= 256
            regs.p.v = !!(regs.y.Array[regs.y.Offset] >= regs.x.Array[regs.x.Offset]);
            regs.p.h = !!((regs.y.Array[regs.y.Offset] & 15) >= (regs.x.Array[regs.x.Offset] & 15));
            if (regs.y.Array[regs.y.Offset] < (regs.x.Array[regs.x.Offset] << 1))
            {
                //if quotient is <= 511 (will fit into 9-bit result)
                regs.a.Array[regs.a.Offset] = (byte)(ya / regs.x.Array[regs.x.Offset]);
                regs.y.Array[regs.y.Offset] = (byte)(ya % regs.x.Array[regs.x.Offset]);
            }
            else
            {
                //otherwise, the quotient won't fit into regs.p.v + regs.a
                //this emulates the odd behavior of the S-SMP in this case
                regs.a.Array[regs.a.Offset] = (byte)(255 - (ya - (regs.x.Array[regs.x.Offset] << 9)) / (256 - regs.x.Array[regs.x.Offset]));
                regs.y.Array[regs.y.Offset] = (byte)(regs.x.Array[regs.x.Offset] + (ya - (regs.x.Array[regs.x.Offset] << 9)) % (256 - regs.x.Array[regs.x.Offset]));
            }
            //result is set based on a (quotient) only
            regs.p.n = Convert.ToBoolean(Convert.ToInt32(Convert.ToBoolean(regs.a.Array[regs.a.Offset] & 0x80)));
            regs.p.z = (regs.a.Array[regs.a.Offset] == 0);
            return null;
        }

        public SMPCoreOperation[] opcode_table = new SMPCoreOperation[256];

        public void initialize_opcode_table()
        {
            opcode_table[0x00] = new SMPCoreOperation(op_nop, null);
            opcode_table[0x01] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 0 });
            opcode_table[0x02] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x01 });
            opcode_table[0x03] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x01, value = Convert.ToInt32(true) });
            opcode_table[0x04] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_or, n = (int)OpCode.A });
            opcode_table[0x05] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_or, n = (int)OpCode.A });
            opcode_table[0x06] = new SMPCoreOperation(op_read_a_ix, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x07] = new SMPCoreOperation(op_read_a_idpx, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x08] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_or, n = (int)OpCode.A });
            opcode_table[0x09] = new SMPCoreOperation(op_read_dp_dp, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x0a] = new SMPCoreOperation(op_or1_bit, new SMPCoreOpArgument() { op = 0 });
            opcode_table[0x0b] = new SMPCoreOperation(op_adjust_dp, new SMPCoreOpArgument() { op_func = op_asl });
            opcode_table[0x0c] = new SMPCoreOperation(op_adjust_addr, new SMPCoreOpArgument() { op_func = op_asl });
            opcode_table[0x0d] = new SMPCoreOperation(op_push_p, null);
            opcode_table[0x0e] = new SMPCoreOperation(op_adjust_addr_a, new SMPCoreOpArgument() { op = 1 });
            opcode_table[0x0f] = new SMPCoreOperation(op_brk, null);
            opcode_table[0x10] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x80, value = Convert.ToInt32(false) });
            opcode_table[0x11] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 1 });
            opcode_table[0x12] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x01 });
            opcode_table[0x13] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x01, value = Convert.ToInt32(false) });
            opcode_table[0x14] = new SMPCoreOperation(op_read_a_dpx, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x15] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_or, i = (int)OpCode.X });
            opcode_table[0x16] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_or, i = (int)OpCode.Y });
            opcode_table[0x17] = new SMPCoreOperation(op_read_a_idpy, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x18] = new SMPCoreOperation(op_read_dp_const, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x19] = new SMPCoreOperation(op_read_ix_iy, new SMPCoreOpArgument() { op_func = op_or });
            opcode_table[0x1a] = new SMPCoreOperation(op_adjustw_dp, new SMPCoreOpArgument() { adjust = -1 });
            opcode_table[0x1b] = new SMPCoreOperation(op_adjust_dpx, new SMPCoreOpArgument() { op_func = op_asl });
            opcode_table[0x1c] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_asl, n = (int)OpCode.A });
            opcode_table[0x1d] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_dec, n = (int)OpCode.X });
            opcode_table[0x1e] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.X });
            opcode_table[0x1f] = new SMPCoreOperation(op_jmp_iaddrx, null);
            opcode_table[0x20] = new SMPCoreOperation(op_setbit, new SMPCoreOpArgument() { mask = 0x20, value = 0x00 });
            opcode_table[0x21] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 2 });
            opcode_table[0x22] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x02 });
            opcode_table[0x23] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x02, value = Convert.ToInt32(true) });
            opcode_table[0x24] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_and, n = (int)OpCode.A });
            opcode_table[0x25] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_and, n = (int)OpCode.A });
            opcode_table[0x26] = new SMPCoreOperation(op_read_a_ix, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x27] = new SMPCoreOperation(op_read_a_idpx, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x28] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_and, n = (int)OpCode.A });
            opcode_table[0x29] = new SMPCoreOperation(op_read_dp_dp, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x2a] = new SMPCoreOperation(op_or1_bit, new SMPCoreOpArgument() { op = 1 });
            opcode_table[0x2b] = new SMPCoreOperation(op_adjust_dp, new SMPCoreOpArgument() { op_func = op_rol });
            opcode_table[0x2c] = new SMPCoreOperation(op_adjust_addr, new SMPCoreOpArgument() { op_func = op_rol });
            opcode_table[0x2d] = new SMPCoreOperation(op_push_reg, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0x2e] = new SMPCoreOperation(op_cbne_dp, null);
            opcode_table[0x2f] = new SMPCoreOperation(op_bra, null);
            opcode_table[0x30] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x80, value = Convert.ToInt32(true) });
            opcode_table[0x31] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 3 });
            opcode_table[0x32] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x02 });
            opcode_table[0x33] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x02, value = Convert.ToInt32(false) });
            opcode_table[0x34] = new SMPCoreOperation(op_read_a_dpx, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x35] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_and, i = (int)OpCode.X });
            opcode_table[0x36] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_and, i = (int)OpCode.Y });
            opcode_table[0x37] = new SMPCoreOperation(op_read_a_idpy, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x38] = new SMPCoreOperation(op_read_dp_const, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x39] = new SMPCoreOperation(op_read_ix_iy, new SMPCoreOpArgument() { op_func = op_and });
            opcode_table[0x3a] = new SMPCoreOperation(op_adjustw_dp, new SMPCoreOpArgument() { adjust = +1 });
            opcode_table[0x3b] = new SMPCoreOperation(op_adjust_dpx, new SMPCoreOpArgument() { op_func = op_rol });
            opcode_table[0x3c] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_rol, n = (int)OpCode.A });
            opcode_table[0x3d] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_inc, n = (int)OpCode.X });
            opcode_table[0x3e] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.X });
            opcode_table[0x3f] = new SMPCoreOperation(op_call, null);
            opcode_table[0x40] = new SMPCoreOperation(op_setbit, new SMPCoreOpArgument() { mask = 0x20, value = 0x20 });
            opcode_table[0x41] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 4 });
            opcode_table[0x42] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x04 });
            opcode_table[0x43] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x04, value = Convert.ToInt32(true) });
            opcode_table[0x44] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_eor, n = (int)OpCode.A });
            opcode_table[0x45] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_eor, n = (int)OpCode.A });
            opcode_table[0x46] = new SMPCoreOperation(op_read_a_ix, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x47] = new SMPCoreOperation(op_read_a_idpx, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x48] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_eor, n = (int)OpCode.A });
            opcode_table[0x49] = new SMPCoreOperation(op_read_dp_dp, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x4a] = new SMPCoreOperation(op_and1_bit, new SMPCoreOpArgument() { op = 0 });
            opcode_table[0x4b] = new SMPCoreOperation(op_adjust_dp, new SMPCoreOpArgument() { op_func = op_lsr });
            opcode_table[0x4c] = new SMPCoreOperation(op_adjust_addr, new SMPCoreOpArgument() { op_func = op_lsr });
            opcode_table[0x4d] = new SMPCoreOperation(op_push_reg, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0x4e] = new SMPCoreOperation(op_adjust_addr_a, new SMPCoreOpArgument() { op = 0 });
            opcode_table[0x4f] = new SMPCoreOperation(op_pcall, null);
            opcode_table[0x50] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x40, value = Convert.ToInt32(false) });
            opcode_table[0x51] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 5 });
            opcode_table[0x52] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x04 });
            opcode_table[0x53] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x04, value = Convert.ToInt32(false) });
            opcode_table[0x54] = new SMPCoreOperation(op_read_a_dpx, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x55] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_eor, i = (int)OpCode.X });
            opcode_table[0x56] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_eor, i = (int)OpCode.Y });
            opcode_table[0x57] = new SMPCoreOperation(op_read_a_idpy, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x58] = new SMPCoreOperation(op_read_dp_const, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x59] = new SMPCoreOperation(op_read_ix_iy, new SMPCoreOpArgument() { op_func = op_eor });
            opcode_table[0x5a] = new SMPCoreOperation(op_cmpw_ya_dp, null);
            opcode_table[0x5b] = new SMPCoreOperation(op_adjust_dpx, new SMPCoreOpArgument() { op_func = op_lsr });
            opcode_table[0x5c] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_lsr, n = (int)OpCode.A });
            opcode_table[0x5d] = new SMPCoreOperation(op_mov_reg_reg, new SMPCoreOpArgument() { to = (int)OpCode.X, from = (int)OpCode.A });
            opcode_table[0x5e] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.Y });
            opcode_table[0x5f] = new SMPCoreOperation(op_jmp_addr, null);
            opcode_table[0x60] = new SMPCoreOperation(op_setbit, new SMPCoreOpArgument() { mask = 0x01, value = 0x00 });
            opcode_table[0x61] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 6 });
            opcode_table[0x62] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x08 });
            opcode_table[0x63] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x08, value = Convert.ToInt32(true) });
            opcode_table[0x64] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.A });
            opcode_table[0x65] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.A });
            opcode_table[0x66] = new SMPCoreOperation(op_read_a_ix, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x67] = new SMPCoreOperation(op_read_a_idpx, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x68] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.A });
            opcode_table[0x69] = new SMPCoreOperation(op_read_dp_dp, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x6a] = new SMPCoreOperation(op_and1_bit, new SMPCoreOpArgument() { op = 1 });
            opcode_table[0x6b] = new SMPCoreOperation(op_adjust_dp, new SMPCoreOpArgument() { op_func = op_ror });
            opcode_table[0x6c] = new SMPCoreOperation(op_adjust_addr, new SMPCoreOpArgument() { op_func = op_ror });
            opcode_table[0x6d] = new SMPCoreOperation(op_push_reg, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0x6e] = new SMPCoreOperation(op_dbnz_dp, null);
            opcode_table[0x6f] = new SMPCoreOperation(op_ret, null);
            opcode_table[0x70] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x40, value = Convert.ToInt32(true) });
            opcode_table[0x71] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 7 });
            opcode_table[0x72] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x08 });
            opcode_table[0x73] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x08, value = Convert.ToInt32(false) });
            opcode_table[0x74] = new SMPCoreOperation(op_read_a_dpx, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x75] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_cmp, i = (int)OpCode.X });
            opcode_table[0x76] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_cmp, i = (int)OpCode.Y });
            opcode_table[0x77] = new SMPCoreOperation(op_read_a_idpy, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x78] = new SMPCoreOperation(op_read_dp_const, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x79] = new SMPCoreOperation(op_read_ix_iy, new SMPCoreOpArgument() { op_func = op_cmp });
            opcode_table[0x7a] = new SMPCoreOperation(op_read_ya_dp, new SMPCoreOpArgument() { op_func = op_addw });
            opcode_table[0x7b] = new SMPCoreOperation(op_adjust_dpx, new SMPCoreOpArgument() { op_func = op_ror });
            opcode_table[0x7c] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_ror, n = (int)OpCode.A });
            opcode_table[0x7d] = new SMPCoreOperation(op_mov_reg_reg, new SMPCoreOpArgument() { to = (int)OpCode.A, from = (int)OpCode.X });
            opcode_table[0x7e] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.Y });
            opcode_table[0x7f] = new SMPCoreOperation(op_reti, null);
            opcode_table[0x80] = new SMPCoreOperation(op_setbit, new SMPCoreOpArgument() { mask = 0x01, value = 0x01 });
            opcode_table[0x81] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 8 });
            opcode_table[0x82] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x10 });
            opcode_table[0x83] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x10, value = Convert.ToInt32(true) });
            opcode_table[0x84] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_adc, n = (int)OpCode.A });
            opcode_table[0x85] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_adc, n = (int)OpCode.A });
            opcode_table[0x86] = new SMPCoreOperation(op_read_a_ix, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x87] = new SMPCoreOperation(op_read_a_idpx, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x88] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_adc, n = (int)OpCode.A });
            opcode_table[0x89] = new SMPCoreOperation(op_read_dp_dp, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x8a] = new SMPCoreOperation(op_eor1_bit, null);
            opcode_table[0x8b] = new SMPCoreOperation(op_adjust_dp, new SMPCoreOpArgument() { op_func = op_dec });
            opcode_table[0x8c] = new SMPCoreOperation(op_adjust_addr, new SMPCoreOpArgument() { op_func = op_dec });
            opcode_table[0x8d] = new SMPCoreOperation(op_mov_reg_const, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0x8e] = new SMPCoreOperation(op_pop_p, null);
            opcode_table[0x8f] = new SMPCoreOperation(op_mov_dp_const, null);
            opcode_table[0x90] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x01, value = Convert.ToInt32(false) });
            opcode_table[0x91] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 9 });
            opcode_table[0x92] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x10 });
            opcode_table[0x93] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x10, value = Convert.ToInt32(false) });
            opcode_table[0x94] = new SMPCoreOperation(op_read_a_dpx, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x95] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_adc, i = (int)OpCode.X });
            opcode_table[0x96] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_adc, i = (int)OpCode.Y });
            opcode_table[0x97] = new SMPCoreOperation(op_read_a_idpy, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x98] = new SMPCoreOperation(op_read_dp_const, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x99] = new SMPCoreOperation(op_read_ix_iy, new SMPCoreOpArgument() { op_func = op_adc });
            opcode_table[0x9a] = new SMPCoreOperation(op_read_ya_dp, new SMPCoreOpArgument() { op_func = op_subw });
            opcode_table[0x9b] = new SMPCoreOperation(op_adjust_dpx, new SMPCoreOpArgument() { op_func = op_dec });
            opcode_table[0x9c] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_dec, n = (int)OpCode.A });
            opcode_table[0x9d] = new SMPCoreOperation(op_mov_reg_reg, new SMPCoreOpArgument() { to = (int)OpCode.X, from = (int)OpCode.SP });
            opcode_table[0x9e] = new SMPCoreOperation(op_div_ya_x, null);
            opcode_table[0x9f] = new SMPCoreOperation(op_xcn, null);
            opcode_table[0xa0] = new SMPCoreOperation(op_seti, new SMPCoreOpArgument() { value = 1 });
            opcode_table[0xa1] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 10 });
            opcode_table[0xa2] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x20 });
            opcode_table[0xa3] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x20, value = Convert.ToInt32(true) });
            opcode_table[0xa4] = new SMPCoreOperation(op_read_reg_dp, new SMPCoreOpArgument() { op_func = op_sbc, n = (int)OpCode.A });
            opcode_table[0xa5] = new SMPCoreOperation(op_read_reg_addr, new SMPCoreOpArgument() { op_func = op_sbc, n = (int)OpCode.A });
            opcode_table[0xa6] = new SMPCoreOperation(op_read_a_ix, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xa7] = new SMPCoreOperation(op_read_a_idpx, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xa8] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_sbc, n = (int)OpCode.A });
            opcode_table[0xa9] = new SMPCoreOperation(op_read_dp_dp, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xaa] = new SMPCoreOperation(op_mov1_c_bit, null);
            opcode_table[0xab] = new SMPCoreOperation(op_adjust_dp, new SMPCoreOpArgument() { op_func = op_inc });
            opcode_table[0xac] = new SMPCoreOperation(op_adjust_addr, new SMPCoreOpArgument() { op_func = op_inc });
            opcode_table[0xad] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.Y });
            opcode_table[0xae] = new SMPCoreOperation(op_pop_reg, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0xaf] = new SMPCoreOperation(op_mov_ixinc_a, null);
            opcode_table[0xb0] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x01, value = Convert.ToInt32(true) });
            opcode_table[0xb1] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 11 });
            opcode_table[0xb2] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x20 });
            opcode_table[0xb3] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x20, value = Convert.ToInt32(false) });
            opcode_table[0xb4] = new SMPCoreOperation(op_read_a_dpx, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xb5] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_sbc, i = (int)OpCode.X });
            opcode_table[0xb6] = new SMPCoreOperation(op_read_a_addrr, new SMPCoreOpArgument() { op_func = op_sbc, i = (int)OpCode.Y });
            opcode_table[0xb7] = new SMPCoreOperation(op_read_a_idpy, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xb8] = new SMPCoreOperation(op_read_dp_const, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xb9] = new SMPCoreOperation(op_read_ix_iy, new SMPCoreOpArgument() { op_func = op_sbc });
            opcode_table[0xba] = new SMPCoreOperation(op_movw_ya_dp, null);
            opcode_table[0xbb] = new SMPCoreOperation(op_adjust_dpx, new SMPCoreOpArgument() { op_func = op_inc });
            opcode_table[0xbc] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_inc, n = (int)OpCode.A });
            opcode_table[0xbd] = new SMPCoreOperation(op_mov_sp_x, null);
            opcode_table[0xbe] = new SMPCoreOperation(op_das, null);
            opcode_table[0xbf] = new SMPCoreOperation(op_mov_a_ixinc, null);
            opcode_table[0xc0] = new SMPCoreOperation(op_seti, new SMPCoreOpArgument() { value = 0 });
            opcode_table[0xc1] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 12 });
            opcode_table[0xc2] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x40 });
            opcode_table[0xc3] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x40, value = Convert.ToInt32(true) });
            opcode_table[0xc4] = new SMPCoreOperation(op_mov_dp_reg, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0xc5] = new SMPCoreOperation(op_mov_addr_reg, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0xc6] = new SMPCoreOperation(op_mov_ix_a, null);
            opcode_table[0xc7] = new SMPCoreOperation(op_mov_idpx_a, null);
            opcode_table[0xc8] = new SMPCoreOperation(op_read_reg_const, new SMPCoreOpArgument() { op_func = op_cmp, n = (int)OpCode.X });
            opcode_table[0xc9] = new SMPCoreOperation(op_mov_addr_reg, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0xca] = new SMPCoreOperation(op_mov1_bit_c, null);
            opcode_table[0xcb] = new SMPCoreOperation(op_mov_dp_reg, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0xcc] = new SMPCoreOperation(op_mov_addr_reg, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0xcd] = new SMPCoreOperation(op_mov_reg_const, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0xce] = new SMPCoreOperation(op_pop_reg, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0xcf] = new SMPCoreOperation(op_mul_ya, null);
            opcode_table[0xd0] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x02, value = Convert.ToInt32(false) });
            opcode_table[0xd1] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 13 });
            opcode_table[0xd2] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x40 });
            opcode_table[0xd3] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x40, value = Convert.ToInt32(false) });
            opcode_table[0xd4] = new SMPCoreOperation(op_mov_dpr_reg, new SMPCoreOpArgument() { n = (int)OpCode.A, i = (int)OpCode.X });
            opcode_table[0xd5] = new SMPCoreOperation(op_mov_addrr_a, new SMPCoreOpArgument() { i = (int)OpCode.X });
            opcode_table[0xd6] = new SMPCoreOperation(op_mov_addrr_a, new SMPCoreOpArgument() { i = (int)OpCode.Y });
            opcode_table[0xd7] = new SMPCoreOperation(op_mov_idpy_a, null);
            opcode_table[0xd8] = new SMPCoreOperation(op_mov_dp_reg, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0xd9] = new SMPCoreOperation(op_mov_dpr_reg, new SMPCoreOpArgument() { n = (int)OpCode.X, i = (int)OpCode.Y });
            opcode_table[0xda] = new SMPCoreOperation(op_movw_dp_ya, null);
            opcode_table[0xdb] = new SMPCoreOperation(op_mov_dpr_reg, new SMPCoreOpArgument() { n = (int)OpCode.Y, i = (int)OpCode.X });
            opcode_table[0xdc] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_dec, n = (int)OpCode.Y });
            opcode_table[0xdd] = new SMPCoreOperation(op_mov_reg_reg, new SMPCoreOpArgument() { to = (int)OpCode.A, from = (int)OpCode.Y });
            opcode_table[0xde] = new SMPCoreOperation(op_cbne_dpx, null);
            opcode_table[0xdf] = new SMPCoreOperation(op_daa, null);
            opcode_table[0xe0] = new SMPCoreOperation(op_setbit, new SMPCoreOpArgument() { mask = 0x48, value = 0x00 });
            opcode_table[0xe1] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 14 });
            opcode_table[0xe2] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 1, value = 0x80 });
            opcode_table[0xe3] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x80, value = Convert.ToInt32(true) });
            opcode_table[0xe4] = new SMPCoreOperation(op_mov_reg_dp, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0xe5] = new SMPCoreOperation(op_mov_reg_addr, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0xe6] = new SMPCoreOperation(op_mov_a_ix, null);
            opcode_table[0xe7] = new SMPCoreOperation(op_mov_a_idpx, null);
            opcode_table[0xe8] = new SMPCoreOperation(op_mov_reg_const, new SMPCoreOpArgument() { n = (int)OpCode.A });
            opcode_table[0xe9] = new SMPCoreOperation(op_mov_reg_addr, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0xea] = new SMPCoreOperation(op_not1_bit, null);
            opcode_table[0xeb] = new SMPCoreOperation(op_mov_reg_dp, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0xec] = new SMPCoreOperation(op_mov_reg_addr, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0xed] = new SMPCoreOperation(op_notc, null);
            opcode_table[0xee] = new SMPCoreOperation(op_pop_reg, new SMPCoreOpArgument() { n = (int)OpCode.Y });
            opcode_table[0xef] = new SMPCoreOperation(op_wait, null);
            opcode_table[0xf0] = new SMPCoreOperation(op_branch, new SMPCoreOpArgument() { flag = 0x02, value = Convert.ToInt32(true) });
            opcode_table[0xf1] = new SMPCoreOperation(op_tcall, new SMPCoreOpArgument() { n = 15 });
            opcode_table[0xf2] = new SMPCoreOperation(op_setbit_dp, new SMPCoreOpArgument() { op = 0, value = 0x80 });
            opcode_table[0xf3] = new SMPCoreOperation(op_bitbranch, new SMPCoreOpArgument() { mask = 0x80, value = Convert.ToInt32(false) });
            opcode_table[0xf4] = new SMPCoreOperation(op_mov_reg_dpr, new SMPCoreOpArgument() { n = (int)OpCode.A, i = (int)OpCode.X });
            opcode_table[0xf5] = new SMPCoreOperation(op_mov_a_addrr, new SMPCoreOpArgument() { i = (int)OpCode.X });
            opcode_table[0xf6] = new SMPCoreOperation(op_mov_a_addrr, new SMPCoreOpArgument() { i = (int)OpCode.Y });
            opcode_table[0xf7] = new SMPCoreOperation(op_mov_a_idpy, null);
            opcode_table[0xf8] = new SMPCoreOperation(op_mov_reg_dp, new SMPCoreOpArgument() { n = (int)OpCode.X });
            opcode_table[0xf9] = new SMPCoreOperation(op_mov_reg_dpr, new SMPCoreOpArgument() { n = (int)OpCode.X, i = (int)OpCode.Y });
            opcode_table[0xfa] = new SMPCoreOperation(op_mov_dp_dp, null);
            opcode_table[0xfb] = new SMPCoreOperation(op_mov_reg_dpr, new SMPCoreOpArgument() { n = (int)OpCode.Y, i = (int)OpCode.X });
            opcode_table[0xfc] = new SMPCoreOperation(op_adjust_reg, new SMPCoreOpArgument() { op_func = op_inc, n = (int)OpCode.Y });
            opcode_table[0xfd] = new SMPCoreOperation(op_mov_reg_reg, new SMPCoreOpArgument() { to = (int)OpCode.Y, from = (int)OpCode.A });
            opcode_table[0xfe] = new SMPCoreOperation(op_dbnz_y, null);
            opcode_table[0xff] = new SMPCoreOperation(op_wait, null);
        }

        public void core_serialize(Serializer s)
        {
            s.integer(regs.pc, "regs.pc");
            s.integer(regs.a.Array[regs.a.Offset], "regs.a");
            s.integer(regs.x.Array[regs.x.Offset], "regs.x");
            s.integer(regs.y.Array[regs.y.Offset], "regs.y");
            s.integer(regs.sp.Array[regs.sp.Offset], "regs.sp");
            s.integer(regs.p.n, "regs.p.n");
            s.integer(regs.p.v, "regs.p.v");
            s.integer(regs.p.p, "regs.p.p");
            s.integer(regs.p.b, "regs.p.b");
            s.integer(regs.p.h, "regs.p.h");
            s.integer(regs.p.i, "regs.p.i");
            s.integer(regs.p.z, "regs.p.z");
            s.integer(regs.p.c, "regs.p.c");

            s.integer(dp, "dp");
            s.integer(sp, "sp");
            s.integer(rd, "rd");
            s.integer(wr, "wr");
            s.integer(bit, "bit");
            s.integer(ya, "ya");
        }

        public SMPCore()
        {
            initialize_opcode_table();
        }
    }
}
