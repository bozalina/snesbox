using System;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    abstract partial class CPUCore
    {
        public async Task<byte> op_readpc()
        {
            return await op_read((uint)((regs.pc.b << 16) + regs.pc.w++));
        }

        public async Task<byte> op_readstack()
        {
            if (regs.e)
            {
                regs.s.l++;
            }
            else
            {
                regs.s.w++;
            }
            return await op_read(regs.s.w);
        }

        public async Task<byte> op_readstackn()
        {
            return await op_read(++regs.s.w);
        }

        public async Task<byte> op_readaddr(uint addr)
        {
            return await op_read(addr & 0xffff);
        }

        public async Task<byte> op_readlong(uint addr)
        {
            return await op_read(addr & 0xffffff);
        }

        public async Task<byte> op_readdbr(uint addr)
        {
            return await op_read((uint)(((regs.db << 16) + addr) & 0xffffff));
        }

        public async Task<byte> op_readpbr(uint addr)
        {
            return await op_read((uint)((regs.pc.b << 16) + (addr & 0xffff)));
        }

        public async Task<byte> op_readdp(uint addr)
        {
            if (regs.e && regs.d.l == 0x00)
            {
                return await op_read((regs.d & 0xff00) + ((regs.d + (addr & 0xffff)) & 0xff));
            }
            else
            {
                return await op_read((regs.d + (addr & 0xffff)) & 0xffff);
            }
        }

        public async Task<byte> op_readsp(uint addr)
        {
            return await op_read((regs.s + (addr & 0xffff)) & 0xffff);
        }

        public async Task op_writestack(byte data)
        {
            await op_write(regs.s.w, data);
            if (regs.e)
            {
                regs.s.l--;
            }
            else
            {
                regs.s.w--;
            }
		}

		public async Task op_writestackn(byte data)
        {
            await op_write(regs.s.w--, data);
		}

		public async Task op_writeaddr(uint addr, byte data)
        {
            await op_write(addr & 0xffff, data);
		}

		public async Task op_writelong(uint addr, byte data)
        {
            await op_write(addr & 0xffffff, data);
		}

		public async Task op_writedbr(uint addr, byte data)
        {
            await op_write((uint)(((regs.db << 16) + addr) & 0xffffff), data);
		}

		public async Task op_writepbr(uint addr, byte data)
        {
            await op_write((uint)((regs.pc.b << 16) + (addr & 0xffff)), data);
		}

		public async Task op_writedp(uint addr, byte data)
        {
            if (regs.e && regs.d.l == 0x00)
            {
                await op_write((regs.d & 0xff00) + ((regs.d + (addr & 0xffff)) & 0xff), data);
            }
            else
            {
				await op_write((regs.d + (addr & 0xffff)) & 0xffff, data);
            }
        }

        public async Task op_writesp(uint addr, byte data)
        {
			await op_write((regs.s + (addr & 0xffff)) & 0xffff, data);
		}

		public enum OpType { DP = 0, DPX, DPY, IDP, IDPX, IDPY, ILDP, ILDPY, ADDR, ADDRX, ADDRY, IADDRX, ILADDR, LONG, LONGX, SR, ISRY, ADDR_PC, IADDR_PC, RELB, RELW }

        private static Reg24 pc = new Reg24();

        private static byte op8(byte op0)
        {
            return ((op0));
        }

        private static ushort op16(byte op0, byte op1)
        {
            return (ushort)((op0) | (op1 << 8));
        }

        private static uint op24(byte op0, byte op1, byte op2)
        {
            return (uint)((op0) | (op1 << 8) | (op2 << 16));
        }

        private static bool a8(Regs regs)
        {
            return regs.e || regs.p.m;
        }

        private static bool x8(Regs regs)
        {
            return regs.e || regs.p.x;
        }

        public async Task<string> disassemble_opcode(uint addr)
        {
            pc.d = addr;
            string s = string.Format("{0:X6} ", ((uint)pc.d));

            byte op = await dreadb(pc.d);
            pc.w++;
            byte op0 = await dreadb(pc.d);
            pc.w++;
            byte op1 = await dreadb(pc.d);
            pc.w++;
            byte op2 = await dreadb(pc.d);

            switch (op)
            {
                case 0x00:
                    s += string.Format("brk #${0:X2}              ", op8(op0));
                    break;
                case 0x01:
                    s += string.Format("ora (${0:X2},x)   [{1:X6}]", op8(op0), decode((byte)OpType.IDPX, op8(op0)));
                    break;
                case 0x02:
                    s += string.Format("cop #${0:X2}              ", op8(op0));
                    break;
                case 0x03:
                    s += string.Format("ora ${0:X2},s     [{1:X6}]", op8(op0), decode((byte)OpType.SR, op8(op0)));
                    break;
                case 0x04:
                    s += string.Format("tsb ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x05:
                    s += string.Format("ora ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x06:
                    s += string.Format("asl ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x07:
                    s += string.Format("ora [${0:X2}]     [{1:X6}]", op8(op0), decode((byte)OpType.ILDP, op8(op0)));
                    break;
                case 0x08:
                    s += string.Format("php                   ");
                    break;
                case 0x09:
                    if (a8(regs))
                    {
                        s += string.Format("ora #${0:X2}              ", op8(op0));
                    }
                    else
                    {
                        s += string.Format("ora #${0:X4}            ", op16(op0, op1));
                    }
                    break;
                case 0x0a:
                    s += string.Format("asl a                 ");
                    break;
                case 0x0b:
                    s += string.Format("phd                   ");
                    break;
                case 0x0c:
                    s += string.Format("tsb ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x0d:
                    s += string.Format("ora ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x0e:
                    s += string.Format("asl ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x0f:
                    s += string.Format("ora ${0:X6}   [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONG, op24(op0, op1, op2)));
                    break;
                case 0x10:
                    s += string.Format("bpl ${0:X4}     [{1:X6}]", (ushort)(await decode((byte)OpType.RELB, op8(op0))), decode((byte)OpType.RELB, op8(op0)));
                    break;
                case 0x11:
                    s += string.Format("ora (${0:X2}),y   [{1:X6}]", op8(op0), decode((byte)OpType.IDPY, op8(op0)));
                    break;
                case 0x12:
                    s += string.Format("ora (${0:X2})     [{1:X6}]", op8(op0), decode((byte)OpType.IDP, op8(op0)));
                    break;
                case 0x13:
                    s += string.Format("ora (${0:X2},s),y [{1:X6}]", op8(op0), decode((byte)OpType.ISRY, op8(op0)));
                    break;
                case 0x14:
                    s += string.Format("trb ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x15:
                    s += string.Format("ora ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0x16:
                    s += string.Format("asl ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0x17:
                    s += string.Format("ora [${0:X2}],y   [{1:X6}]", op8(op0), decode((byte)OpType.ILDPY, op8(op0)));
                    break;
                case 0x18:
                    s += string.Format("clc                   ");
                    break;
                case 0x19:
                    s += string.Format("ora ${0:X4},y   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRY, op16(op0, op1)));
                    break;
                case 0x1a:
                    s += string.Format("inc                   ");
                    break;
                case 0x1b:
                    s += string.Format("tcs                   ");
                    break;
                case 0x1c:
                    s += string.Format("trb ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x1d:
                    s += string.Format("ora ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0x1e:
                    s += string.Format("asl ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0x1f:
                    s += string.Format("ora ${0:X6},x [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONGX, op24(op0, op1, op2)));
                    break;
                case 0x20:
                    s += string.Format("jsr ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR_PC, op16(op0, op1)));
                    break;
                case 0x21:
                    s += string.Format("and (${0:X2},x)   [{1:X6}]", op8(op0), decode((byte)OpType.IDPX, op8(op0)));
                    break;
                case 0x22:
                    s += string.Format("jsl ${0:X6}   [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONG, op24(op0, op1, op2)));
                    break;
                case 0x23:
                    s += string.Format("and ${0:X2},s     [{1:X6}]", op8(op0), decode((byte)OpType.SR, op8(op0)));
                    break;
                case 0x24:
                    s += string.Format("bit ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x25:
                    s += string.Format("and ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x26:
                    s += string.Format("rol ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x27:
                    s += string.Format("and [${0:X2}]     [{1:X6}]", op8(op0), decode((byte)OpType.ILDP, op8(op0)));
                    break;
                case 0x28:
                    s += string.Format("plp                   ");
                    break;
                case 0x29:
                    if (a8(regs))
                    {
                        s += string.Format("and #${0:X2}              ", op8(op0));
                    }
                    else
                    {
                        s += string.Format("and #${0:X4}            ", op16(op0, op1));
                    }
                    break;
                case 0x2a:
                    s += string.Format("rol a                 ");
                    break;
                case 0x2b:
                    s += string.Format("pld                   ");
                    break;
                case 0x2c:
                    s += string.Format("bit ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x2d:
                    s += string.Format("and ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x2e:
                    s += string.Format("rol ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x2f:
                    s += string.Format("and ${0:X6}   [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONG, op24(op0, op1, op2)));
                    break;
                case 0x30:
                    s += string.Format("bmi ${0:X4}     [{1:X6}]", (ushort)(await decode((byte)OpType.RELB, op8(op0))), decode((byte)OpType.RELB, op8(op0)));
                    break;
                case 0x31:
                    s += string.Format("and (${0:X2}),y   [{1:X6}]", op8(op0), decode((byte)OpType.IDPY, op8(op0)));
                    break;
                case 0x32:
                    s += string.Format("and (${0:X2})     [{1:X6}]", op8(op0), decode((byte)OpType.IDP, op8(op0)));
                    break;
                case 0x33:
                    s += string.Format("and (${0:X2},s),y [{1:X6}]", op8(op0), decode((byte)OpType.ISRY, op8(op0)));
                    break;
                case 0x34:
                    s += string.Format("bit ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0x35:
                    s += string.Format("and ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0x36:
                    s += string.Format("rol ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0x37:
                    s += string.Format("and [${0:X2}],y   [{1:X6}]", op8(op0), decode((byte)OpType.ILDPY, op8(op0)));
                    break;
                case 0x38:
                    s += string.Format("sec                   ");
                    break;
                case 0x39:
                    s += string.Format("and ${0:X4},y   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRY, op16(op0, op1)));
                    break;
                case 0x3a:
                    s += string.Format("dec                   ");
                    break;
                case 0x3b:
                    s += string.Format("tsc                   ");
                    break;
                case 0x3c:
                    s += string.Format("bit ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0x3d:
                    s += string.Format("and ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0x3e:
                    s += string.Format("rol ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0x3f:
                    s += string.Format("and ${0:X6},x [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONGX, op24(op0, op1, op2)));
                    break;
                case 0x40:
                    s += string.Format("rti                   ");
                    break;
                case 0x41:
                    s += string.Format("eor (${0:X2},x)   [{1:X6}]", op8(op0), decode((byte)OpType.IDPX, op8(op0)));
                    break;
                case 0x42:
                    s += string.Format("wdm                   ");
                    break;
                case 0x43:
                    s += string.Format("eor ${0:X2},s     [{1:X6}]", op8(op0), decode((byte)OpType.SR, op8(op0)));
                    break;
                case 0x44:
                    s += string.Format("mvp ${0:X2},${1:X2}           ", op1, op8(op0));
                    break;
                case 0x45:
                    s += string.Format("eor ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x46:
                    s += string.Format("lsr ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x47:
                    s += string.Format("eor [${0:X2}]     [{1:X6}]", op8(op0), decode((byte)OpType.ILDP, op8(op0)));
                    break;
                case 0x48:
                    s += string.Format("pha                   ");
                    break;
                case 0x49:
                    if (a8(regs))
                    {
                        s += string.Format("eor #${0:X2}              ", op8(op0));
                    }
                    else
                    {
                        s += string.Format("eor #${0:X4}            ", op16(op0, op1));
                    }
                    break;
                case 0x4a:
                    s += string.Format("lsr a                 ");
                    break;
                case 0x4b:
                    s += string.Format("phk                   ");
                    break;
                case 0x4c:
                    s += string.Format("jmp ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR_PC, op16(op0, op1)));
                    break;
                case 0x4d:
                    s += string.Format("eor ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x4e:
                    s += string.Format("lsr ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x4f:
                    s += string.Format("eor ${0:X6}   [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONG, op24(op0, op1, op2)));
                    break;
                case 0x50:
                    s += string.Format("bvc ${0:X4}     [{1:X6}]", (ushort)(await decode((byte)OpType.RELB, op8(op0))), decode((byte)OpType.RELB, op8(op0)));
                    break;
                case 0x51:
                    s += string.Format("eor (${0:X2}),y   [{1:X6}]", op8(op0), decode((byte)OpType.IDPY, op8(op0)));
                    break;
                case 0x52:
                    s += string.Format("eor (${0:X2})     [{1:X6}]", op8(op0), decode((byte)OpType.IDP, op8(op0)));
                    break;
                case 0x53:
                    s += string.Format("eor (${0:X2},s),y [{1:X6}]", op8(op0), decode((byte)OpType.ISRY, op8(op0)));
                    break;
                case 0x54:
                    s += string.Format("mvn ${0:X2},${1:X2}           ", op1, op8(op0));
                    break;
                case 0x55:
                    s += string.Format("eor ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0x56:
                    s += string.Format("lsr ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0x57:
                    s += string.Format("eor [${0:X2}],y   [{1:X6}]", op8(op0), decode((byte)OpType.ILDPY, op8(op0)));
                    break;
                case 0x58:
                    s += string.Format("cli                   ");
                    break;
                case 0x59:
                    s += string.Format("eor ${0:X4},y   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRY, op16(op0, op1)));
                    break;
                case 0x5a:
                    s += string.Format("phy                   ");
                    break;
                case 0x5b:
                    s += string.Format("tcd                   ");
                    break;
                case 0x5c:
                    s += string.Format("jml ${0:X6}   [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONG, op24(op0, op1, op2)));
                    break;
                case 0x5d:
                    s += string.Format("eor ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0x5e:
                    s += string.Format("lsr ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0x5f:
                    s += string.Format("eor ${0:X6},x [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONGX, op24(op0, op1, op2)));
                    break;
                case 0x60:
                    s += string.Format("rts                   ");
                    break;
                case 0x61:
                    s += string.Format("adc (${0:X2},x)   [{1:X6}]", op8(op0), decode((byte)OpType.IDPX, op8(op0)));
                    break;
                case 0x62:
                    s += string.Format("per ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x63:
                    s += string.Format("adc ${0:X2},s     [{1:X6}]", op8(op0), decode((byte)OpType.SR, op8(op0)));
                    break;
                case 0x64:
                    s += string.Format("stz ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x65:
                    s += string.Format("adc ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x66:
                    s += string.Format("ror ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x67:
                    s += string.Format("adc [${0:X2}]     [{1:X6}]", op8(op0), decode((byte)OpType.ILDP, op8(op0)));
                    break;
                case 0x68:
                    s += string.Format("pla                   ");
                    break;
                case 0x69:
                    if (a8(regs))
                    {
                        s += string.Format("adc #${0:X2}              ", op8(op0));
                    }
                    else
                    {
                        s += string.Format("adc #${0:X4}            ", op16(op0, op1));
                    }
                    break;
                case 0x6a:
                    s += string.Format("ror a                 ");
                    break;
                case 0x6b:
                    s += string.Format("rtl                   ");
                    break;
                case 0x6c:
                    s += string.Format("jmp (${0:X4})   [{1:X6}]", op16(op0, op1), decode((byte)OpType.IADDR_PC, op16(op0, op1)));
                    break;
                case 0x6d:
                    s += string.Format("adc ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x6e:
                    s += string.Format("ror ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x6f:
                    s += string.Format("adc ${0:X6}   [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONG, op24(op0, op1, op2)));
                    break;
                case 0x70:
                    s += string.Format("bvs ${0:X4}     [{1:X6}]", (ushort)(await decode((byte)OpType.RELB, op8(op0))), decode((byte)OpType.RELB, op8(op0)));
                    break;
                case 0x71:
                    s += string.Format("adc (${0:X2}),y   [{1:X6}]", op8(op0), decode((byte)OpType.IDPY, op8(op0)));
                    break;
                case 0x72:
                    s += string.Format("adc (${0:X2})     [{1:X6}]", op8(op0), decode((byte)OpType.IDP, op8(op0)));
                    break;
                case 0x73:
                    s += string.Format("adc (${0:X2},s),y [{1:X6}]", op8(op0), decode((byte)OpType.ISRY, op8(op0)));
                    break;
                case 0x74:
                    s += string.Format("stz ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0x75:
                    s += string.Format("adc ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0x76:
                    s += string.Format("ror ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0x77:
                    s += string.Format("adc [${0:X2}],y   [{1:X6}]", op8(op0), decode((byte)OpType.ILDPY, op8(op0)));
                    break;
                case 0x78:
                    s += string.Format("sei                   ");
                    break;
                case 0x79:
                    s += string.Format("adc ${0:X4},y   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRY, op16(op0, op1)));
                    break;
                case 0x7a:
                    s += string.Format("ply                   ");
                    break;
                case 0x7b:
                    s += string.Format("tdc                   ");
                    break;
                case 0x7c:
                    s += string.Format("jmp (${0:X4},x) [{1:X6}]", op16(op0, op1), decode((byte)OpType.IADDRX, op16(op0, op1)));
                    break;
                case 0x7d:
                    s += string.Format("adc ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0x7e:
                    s += string.Format("ror ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0x7f:
                    s += string.Format("adc ${0:X6},x [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONGX, op24(op0, op1, op2)));
                    break;
                case 0x80:
                    s += string.Format("bra ${0:X4}     [{1:X6}]", (ushort)(await decode((byte)OpType.RELB, op8(op0))), decode((byte)OpType.RELB, op8(op0)));
                    break;
                case 0x81:
                    s += string.Format("sta (${0:X2},x)   [{1:X6}]", op8(op0), decode((byte)OpType.IDPX, op8(op0)));
                    break;
                case 0x82:
                    s += string.Format("brl ${0:X4}     [{1:X6}]", (ushort)(await decode((byte)OpType.RELW, op16(op0, op1))), decode((byte)OpType.RELW, op16(op0, op1)));
                    break;
                case 0x83:
                    s += string.Format("sta ${0:X2},s     [{1:X6}]", op8(op0), decode((byte)OpType.SR, op8(op0)));
                    break;
                case 0x84:
                    s += string.Format("sty ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x85:
                    s += string.Format("sta ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x86:
                    s += string.Format("stx ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0x87:
                    s += string.Format("sta [${0:X2}]     [{1:X6}]", op8(op0), decode((byte)OpType.ILDP, op8(op0)));
                    break;
                case 0x88:
                    s += string.Format("dey                   ");
                    break;
                case 0x89:
                    if (a8(regs))
                    {
                        s += string.Format("bit #${0:X2}              ", op8(op0));
                    }
                    else
                    {
                        s += string.Format("bit #${0:X4}            ", op16(op0, op1));
                    }
                    break;
                case 0x8a:
                    s += string.Format("txa                   ");
                    break;
                case 0x8b:
                    s += string.Format("phb                   ");
                    break;
                case 0x8c:
                    s += string.Format("sty ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x8d:
                    s += string.Format("sta ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x8e:
                    s += string.Format("stx ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x8f:
                    s += string.Format("sta ${0:X6}   [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONG, op24(op0, op1, op2)));
                    break;
                case 0x90:
                    s += string.Format("bcc ${0:X4}     [{1:X6}]", (ushort)(await decode((byte)OpType.RELB, op8(op0))), decode((byte)OpType.RELB, op8(op0)));
                    break;
                case 0x91:
                    s += string.Format("sta (${0:X2}),y   [{1:X6}]", op8(op0), decode((byte)OpType.IDPY, op8(op0)));
                    break;
                case 0x92:
                    s += string.Format("sta (${0:X2})     [{1:X6}]", op8(op0), decode((byte)OpType.IDP, op8(op0)));
                    break;
                case 0x93:
                    s += string.Format("sta (${0:X2},s),y [{1:X6}]", op8(op0), decode((byte)OpType.ISRY, op8(op0)));
                    break;
                case 0x94:
                    s += string.Format("sty ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0x95:
                    s += string.Format("sta ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0x96:
                    s += string.Format("stx ${0:X2},y     [{1:X6}]", op8(op0), decode((byte)OpType.DPY, op8(op0)));
                    break;
                case 0x97:
                    s += string.Format("sta [${0:X2}],y   [{1:X6}]", op8(op0), decode((byte)OpType.ILDPY, op8(op0)));
                    break;
                case 0x98:
                    s += string.Format("tya                   ");
                    break;
                case 0x99:
                    s += string.Format("sta ${0:X4},y   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRY, op16(op0, op1)));
                    break;
                case 0x9a:
                    s += string.Format("txs                   ");
                    break;
                case 0x9b:
                    s += string.Format("txy                   ");
                    break;
                case 0x9c:
                    s += string.Format("stz ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0x9d:
                    s += string.Format("sta ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0x9e:
                    s += string.Format("stz ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0x9f:
                    s += string.Format("sta ${0:X6},x [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONGX, op24(op0, op1, op2)));
                    break;
                case 0xa0:
                    if (x8(regs))
                    {
                        s += string.Format("ldy #${0:X2}              ", op8(op0));
                    }
                    else
                    {
                        s += string.Format("ldy #${0:X4}            ", op16(op0, op1));
                    }
                    break;
                case 0xa1:
                    s += string.Format("lda (${0:X2},x)   [{1:X6}]", op8(op0), decode((byte)OpType.IDPX, op8(op0)));
                    break;
                case 0xa2:
                    if (x8(regs))
                    {
                        s += string.Format("ldx #${0:X2}              ", op8(op0));
                    }
                    else
                    {
                        s += string.Format("ldx #${0:X4}            ", op16(op0, op1));
                    }
                    break;
                case 0xa3:
                    s += string.Format("lda ${0:X2},s     [{1:X6}]", op8(op0), decode((byte)OpType.SR, op8(op0)));
                    break;
                case 0xa4:
                    s += string.Format("ldy ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0xa5:
                    s += string.Format("lda ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0xa6:
                    s += string.Format("ldx ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0xa7:
                    s += string.Format("lda [${0:X2}]     [{1:X6}]", op8(op0), decode((byte)OpType.ILDP, op8(op0)));
                    break;
                case 0xa8:
                    s += string.Format("tay                   ");
                    break;
                case 0xa9:
                    if (a8(regs))
                    {
                        s += string.Format("lda #${0:X2}              ", op8(op0));
                    }
                    else
                    {
                        s += string.Format("lda #${0:X4}            ", op16(op0, op1));
                    }
                    break;
                case 0xaa:
                    s += string.Format("tax                   ");
                    break;
                case 0xab:
                    s += string.Format("plb                   ");
                    break;
                case 0xac:
                    s += string.Format("ldy ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0xad:
                    s += string.Format("lda ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0xae:
                    s += string.Format("ldx ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0xaf:
                    s += string.Format("lda ${0:X6}   [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONG, op24(op0, op1, op2)));
                    break;
                case 0xb0:
                    s += string.Format("bcs ${0:X4}     [{1:X6}]", (ushort)(await decode((byte)OpType.RELB, op8(op0))), decode((byte)OpType.RELB, op8(op0)));
                    break;
                case 0xb1:
                    s += string.Format("lda (${0:X2}),y   [{1:X6}]", op8(op0), decode((byte)OpType.IDPY, op8(op0)));
                    break;
                case 0xb2:
                    s += string.Format("lda (${0:X2})     [{1:X6}]", op8(op0), decode((byte)OpType.IDP, op8(op0)));
                    break;
                case 0xb3:
                    s += string.Format("lda (${0:X2},s),y [{1:X6}]", op8(op0), decode((byte)OpType.ISRY, op8(op0)));
                    break;
                case 0xb4:
                    s += string.Format("ldy ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0xb5:
                    s += string.Format("lda ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0xb6:
                    s += string.Format("ldx ${0:X2},y     [{1:X6}]", op8(op0), decode((byte)OpType.DPY, op8(op0)));
                    break;
                case 0xb7:
                    s += string.Format("lda [${0:X2}],y   [{1:X6}]", op8(op0), decode((byte)OpType.ILDPY, op8(op0)));
                    break;
                case 0xb8:
                    s += string.Format("clv                   ");
                    break;
                case 0xb9:
                    s += string.Format("lda ${0:X4},y   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRY, op16(op0, op1)));
                    break;
                case 0xba:
                    s += string.Format("tsx                   ");
                    break;
                case 0xbb:
                    s += string.Format("tyx                   ");
                    break;
                case 0xbc:
                    s += string.Format("ldy ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0xbd:
                    s += string.Format("lda ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0xbe:
                    s += string.Format("ldx ${0:X4},y   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRY, op16(op0, op1)));
                    break;
                case 0xbf:
                    s += string.Format("lda ${0:X6},x [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONGX, op24(op0, op1, op2)));
                    break;
                case 0xc0:
                    if (x8(regs))
                    {
                        s += string.Format("cpy #${0:X2}              ", op8(op0));
                    }
                    else
                    {
                        s += string.Format("cpy #${0:X4}            ", op16(op0, op1));
                    }
                    break;
                case 0xc1:
                    s += string.Format("cmp (${0:X2},x)   [{1:X6}]", op8(op0), decode((byte)OpType.IDPX, op8(op0)));
                    break;
                case 0xc2:
                    s += string.Format("rep #${0:X2}              ", op8(op0));
                    break;
                case 0xc3:
                    s += string.Format("cmp ${0:X2},s     [{1:X6}]", op8(op0), decode((byte)OpType.SR, op8(op0)));
                    break;
                case 0xc4:
                    s += string.Format("cpy ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0xc5:
                    s += string.Format("cmp ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0xc6:
                    s += string.Format("dec ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0xc7:
                    s += string.Format("cmp [${0:X2}]     [{1:X6}]", op8(op0), decode((byte)OpType.ILDP, op8(op0)));
                    break;
                case 0xc8:
                    s += string.Format("iny                   ");
                    break;
                case 0xc9:
                    if (a8(regs))
                    {
                        s += string.Format("cmp #${0:X2}              ", op8(op0));
                    }
                    else
                    {
                        s += string.Format("cmp #${0:X4}            ", op16(op0, op1));
                    }
                    break;
                case 0xca:
                    s += string.Format("dex                   ");
                    break;
                case 0xcb:
                    s += string.Format("wai                   ");
                    break;
                case 0xcc:
                    s += string.Format("cpy ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0xcd:
                    s += string.Format("cmp ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0xce:
                    s += string.Format("dec ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0xcf:
                    s += string.Format("cmp ${0:X6}   [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONG, op24(op0, op1, op2)));
                    break;
                case 0xd0:
                    s += string.Format("bne ${0:X4}     [{1:X6}]", (ushort)(await decode((byte)OpType.RELB, op8(op0))), decode((byte)OpType.RELB, op8(op0)));
                    break;
                case 0xd1:
                    s += string.Format("cmp (${0:X2}),y   [{1:X6}]", op8(op0), decode((byte)OpType.IDPY, op8(op0)));
                    break;
                case 0xd2:
                    s += string.Format("cmp (${0:X2})     [{1:X6}]", op8(op0), decode((byte)OpType.IDP, op8(op0)));
                    break;
                case 0xd3:
                    s += string.Format("cmp (${0:X2},s),y [{1:X6}]", op8(op0), decode((byte)OpType.ISRY, op8(op0)));
                    break;
                case 0xd4:
                    s += string.Format("pei (${0:X2})     [{1:X6}]", op8(op0), decode((byte)OpType.IDP, op8(op0)));
                    break;
                case 0xd5:
                    s += string.Format("cmp ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0xd6:
                    s += string.Format("dec ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0xd7:
                    s += string.Format("cmp [${0:X2}],y   [{1:X6}]", op8(op0), decode((byte)OpType.ILDPY, op8(op0)));
                    break;
                case 0xd8:
                    s += string.Format("cld                   ");
                    break;
                case 0xd9:
                    s += string.Format("cmp ${0:X4},y   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRY, op16(op0, op1)));
                    break;
                case 0xda:
                    s += string.Format("phx                   ");
                    break;
                case 0xdb:
                    s += string.Format("stp                   ");
                    break;
                case 0xdc:
                    s += string.Format("jmp [${0:X4}]   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ILADDR, op16(op0, op1)));
                    break;
                case 0xdd:
                    s += string.Format("cmp ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0xde:
                    s += string.Format("dec ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0xdf:
                    s += string.Format("cmp ${0:X6},x [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONGX, op24(op0, op1, op2)));
                    break;
                case 0xe0:
                    if (x8(regs))
                    {
                        s += string.Format("cpx #${0:X2}              ", op8(op0));
                    }
                    else
                    {
                        s += string.Format("cpx #${0:X4}            ", op16(op0, op1));
                    }
                    break;
                case 0xe1:
                    s += string.Format("sbc (${0:X2},x)   [{1:X6}]", op8(op0), decode((byte)OpType.IDPX, op8(op0)));
                    break;
                case 0xe2:
                    s += string.Format("sep #${0:X2}              ", op8(op0));
                    break;
                case 0xe3:
                    s += string.Format("sbc ${0:X2},s     [{1:X6}]", op8(op0), decode((byte)OpType.SR, op8(op0)));
                    break;
                case 0xe4:
                    s += string.Format("cpx ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0xe5:
                    s += string.Format("sbc ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0xe6:
                    s += string.Format("inc ${0:X2}       [{1:X6}]", op8(op0), decode((byte)OpType.DP, op8(op0)));
                    break;
                case 0xe7:
                    s += string.Format("sbc [${0:X2}]     [{1:X6}]", op8(op0), decode((byte)OpType.ILDP, op8(op0)));
                    break;
                case 0xe8:
                    s += string.Format("inx                   ");
                    break;
                case 0xe9:
                    if (a8(regs))
                    {
                        s += string.Format("sbc #${0:X2}              ", op8(op0));
                    }
                    else
                    {
                        s += string.Format("sbc #${0:X4}            ", op16(op0, op1));
                    }
                    break;
                case 0xea:
                    s += string.Format("nop                   ");
                    break;
                case 0xeb:
                    s += string.Format("xba                   ");
                    break;
                case 0xec:
                    s += string.Format("cpx ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0xed:
                    s += string.Format("sbc ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0xee:
                    s += string.Format("inc ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0xef:
                    s += string.Format("sbc ${0:X6}   [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONG, op24(op0, op1, op2)));
                    break;
                case 0xf0:
                    s += string.Format("beq ${0:X4}     [{1:X6}]", (ushort)(await decode((byte)OpType.RELB, op8(op0))), decode((byte)OpType.RELB, op8(op0)));
                    break;
                case 0xf1:
                    s += string.Format("sbc (${0:X2}),y   [{1:X6}]", op8(op0), decode((byte)OpType.IDPY, op8(op0)));
                    break;
                case 0xf2:
                    s += string.Format("sbc (${0:X2})     [{1:X6}]", op8(op0), decode((byte)OpType.IDP, op8(op0)));
                    break;
                case 0xf3:
                    s += string.Format("sbc (${0:X2},s),y [{1:X6}]", op8(op0), decode((byte)OpType.ISRY, op8(op0)));
                    break;
                case 0xf4:
                    s += string.Format("pea ${0:X4}     [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDR, op16(op0, op1)));
                    break;
                case 0xf5:
                    s += string.Format("sbc ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0xf6:
                    s += string.Format("inc ${0:X2},x     [{1:X6}]", op8(op0), decode((byte)OpType.DPX, op8(op0)));
                    break;
                case 0xf7:
                    s += string.Format("sbc [${0:X2}],y   [{1:X6}]", op8(op0), decode((byte)OpType.ILDPY, op8(op0)));
                    break;
                case 0xf8:
                    s += string.Format("sed                   ");
                    break;
                case 0xf9:
                    s += string.Format("sbc ${0:X4},y   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRY, op16(op0, op1)));
                    break;
                case 0xfa:
                    s += string.Format("plx                   ");
                    break;
                case 0xfb:
                    s += string.Format("xce                   ");
                    break;
                case 0xfc:
                    s += string.Format("jsr (${0:X4},x) [{1:X6}]", op16(op0, op1), decode((byte)OpType.IADDRX, op16(op0, op1)));
                    break;
                case 0xfd:
                    s += string.Format("sbc ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0xfe:
                    s += string.Format("inc ${0:X4},x   [{1:X6}]", op16(op0, op1), decode((byte)OpType.ADDRX, op16(op0, op1)));
                    break;
                case 0xff:
                    s += string.Format("sbc ${0:X6},x [{1:X6}]", op24(op0, op1, op2), decode((byte)OpType.LONGX, op24(op0, op1, op2)));
                    break;
            }

            s += " ";

            s += string.Format("A:{0:X4} X:{1:X4} Y:{2:X4} S:{3:X4} D:{4:X4} DB:{5:X2} ",
              regs.a.w, regs.x.w, regs.y.w, regs.s.w, regs.d.w, regs.db);

            if (regs.e)
            {
                s += string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                  regs.p.n ? 'N' : 'n', regs.p.v ? 'V' : 'v',
                  regs.p.m ? '1' : '0', regs.p.x ? 'B' : 'b',
                  regs.p.d ? 'D' : 'd', regs.p.i ? 'I' : 'i',
                  regs.p.z ? 'Z' : 'z', regs.p.c ? 'C' : 'c');
            }
            else
            {
                s += string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                  regs.p.n ? 'N' : 'n', regs.p.v ? 'V' : 'v',
                  regs.p.m ? 'M' : 'm', regs.p.x ? 'X' : 'x',
                  regs.p.d ? 'D' : 'd', regs.p.i ? 'I' : 'i',
                  regs.p.z ? 'Z' : 'z', regs.p.c ? 'C' : 'c');
            }

            s += " ";

            s += string.Format("V:{0:##0} H:{1:###0}", CPU.cpu.PPUCounter.vcounter(), CPU.cpu.PPUCounter.hcounter());
	        return s;
        }

        public async Task<byte> dreadb(uint addr)
        {
            if ((addr & 0x40ffff) >= 0x2000 && (addr & 0x40ffff) <= 0x5fff)
            {
                //$[00-3f|80-bf]:[2000-5fff]
                //do not read MMIO registers within debugger
                return 0x00;
            }
            return await Bus.bus.read(new uint24(addr));
        }

        public async Task<ushort> dreadw(uint addr)
        {
            ushort r;
            r = (ushort)(await dreadb((addr + 0) & 0xffffff) << 0);
            r |= (ushort)(await dreadb((addr + 1) & 0xffffff) << 8);
            return r;
        }

        public async Task<uint> dreadl(uint addr)
        {
            uint r;
            r = (uint)(await dreadb((addr + 0) & 0xffffff) << 0);
            r |= (uint)(await dreadb((addr + 1) & 0xffffff) << 8);
            r |= (uint)(await dreadb((addr + 2) & 0xffffff) << 16);
            return r;
        }

        public async Task<uint> decode(byte offset_type, uint addr)
        {
            uint r = 0;

            switch ((OpType)offset_type)
            {
                case OpType.DP:
                    r = (regs.d + (addr & 0xffff)) & 0xffff;
                    break;
                case OpType.DPX:
                    r = ((uint)regs.d + (uint)regs.x + (addr & 0xffff)) & 0xffff;
                    break;
                case OpType.DPY:
                    r = ((uint)regs.d + (uint)regs.y + (addr & 0xffff)) & 0xffff;
                    break;
                case OpType.IDP:
                    addr = (regs.d + (addr & 0xffff)) & 0xffff;
                    r = (uint)((regs.db << 16) + await dreadw(addr));
                    break;
                case OpType.IDPX:
                    addr = ((uint)regs.d + (uint)regs.x + (addr & 0xffff)) & 0xffff;
                    r = (uint)((regs.db << 16) + await dreadw(addr));
                    break;
                case OpType.IDPY:
                    addr = (regs.d + (addr & 0xffff)) & 0xffff;
                    r = (uint)((regs.db << 16) + await dreadw(addr) + (uint)regs.y);
                    break;
                case OpType.ILDP:
                    addr = (regs.d + (addr & 0xffff)) & 0xffff;
                    r = await dreadl(addr);
                    break;
                case OpType.ILDPY:
                    addr = (regs.d + (addr & 0xffff)) & 0xffff;
                    r = await dreadl(addr) + (uint)regs.y;
                    break;
                case OpType.ADDR:
                    r = (uint)((regs.db << 16) + (addr & 0xffff));
                    break;
                case OpType.ADDR_PC:
                    r = (uint)((regs.pc.b << 16) + (addr & 0xffff));
                    break;
                case OpType.ADDRX:
                    r = (uint)((regs.db << 16) + (addr & 0xffff) + (uint)regs.x);
                    break;
                case OpType.ADDRY:
                    r = (uint)((regs.db << 16) + (addr & 0xffff) + (uint)regs.y);
                    break;
                case OpType.IADDR_PC:
                    r = (uint)((regs.pc.b << 16) + (addr & 0xffff));
                    break;
                case OpType.IADDRX:
                    r = (uint)((regs.pc.b << 16) + ((addr + (uint)regs.x) & 0xffff));
                    break;
                case OpType.ILADDR:
                    r = addr;
                    break;
                case OpType.LONG:
                    r = addr;
                    break;
                case OpType.LONGX:
                    r = (addr + (uint)regs.x);
                    break;
                case OpType.SR:
                    r = (regs.s + (addr & 0xff)) & 0xffff;
                    break;
                case OpType.ISRY:
                    addr = (regs.s + (addr & 0xff)) & 0xffff;
                    r = (uint)((regs.db << 16) + await dreadw(addr) + (uint)regs.y);
                    break;
                case OpType.RELB:
                    r = (uint)((regs.pc.b << 16) + ((regs.pc.w + 2) & 0xffff));
                    r += (uint)((sbyte)(addr));
                    break;
                case OpType.RELW:
                    r = (uint)((regs.pc.b << 16) + ((regs.pc.w + 3) & 0xffff));
                    r += (uint)((short)(addr));
                    break;
            }

            return (r & 0xffffff);
        }

        private static readonly byte[] op_len_tbl = new byte[256] 
        { 
          //0, 1, 2, 3, 4, 5, 6, 7, 8, 9, a, b, c, d, e, f
            2, 2, 2, 2, 2, 2, 2, 2, 1, 5, 1, 1, 3, 3, 3, 4,  //0x0n
            2, 2, 2, 2, 2, 2, 2, 2, 1, 3, 1, 1, 3, 3, 3, 4,  //0x1n
            3, 2, 4, 2, 2, 2, 2, 2, 1, 5, 1, 1, 3, 3, 3, 4,  //0x2n
            2, 2, 2, 2, 2, 2, 2, 2, 1, 3, 1, 1, 3, 3, 3, 4,  //0x3n
            1, 2, 2, 2, 3, 2, 2, 2, 1, 5, 1, 1, 3, 3, 3, 4,  //0x4n
            2, 2, 2, 2, 3, 2, 2, 2, 1, 3, 1, 1, 4, 3, 3, 4,  //0x5n
            1, 2, 3, 2, 2, 2, 2, 2, 1, 5, 1, 1, 3, 3, 3, 4,  //0x6n
            2, 2, 2, 2, 2, 2, 2, 2, 1, 3, 1, 1, 3, 3, 3, 4,  //0x7n
            2, 2, 3, 2, 2, 2, 2, 2, 1, 5, 1, 1, 3, 3, 3, 4,  //0x8n
            2, 2, 2, 2, 2, 2, 2, 2, 1, 3, 1, 1, 3, 3, 3, 4,  //0x9n
            6, 2, 6, 2, 2, 2, 2, 2, 1, 5, 1, 1, 3, 3, 3, 4,  //0xan
            2, 2, 2, 2, 2, 2, 2, 2, 1, 3, 1, 1, 3, 3, 3, 4,  //0xbn
            6, 2, 2, 2, 2, 2, 2, 2, 1, 5, 1, 1, 3, 3, 3, 4,  //0xcn
            2, 2, 2, 2, 2, 2, 2, 2, 1, 3, 1, 1, 3, 3, 3, 4,  //0xdn
            6, 2, 2, 2, 2, 2, 2, 2, 1, 5, 1, 1, 3, 3, 3, 4,  //0xen
            2, 2, 2, 2, 3, 2, 2, 2, 1, 3, 1, 1, 3, 3, 3, 4   //0xfn
        };

        public async Task<byte> opcode_length()
        {
            byte op, len;

            op = await dreadb(regs.pc.d);
            len = op_len_tbl[op];
            if (len == 5)
            {
                return (byte)((regs.e || regs.p.m) ? 2 : 3);
            }
            if (len == 6)
            {
                return (byte)((regs.e || regs.p.x) ? 2 : 3);
            }
            return len;
        }

        public Regs regs = new Regs();
        public Reg24 aa = new Reg24();
        public Reg24 rd = new Reg24();
        public byte sp, dp;

        public abstract Task op_io();
        public abstract Task<byte> op_read(uint addr);
        public abstract Task op_write(uint addr, byte data);
        public abstract void last_cycle();
        public abstract bool interrupt_pending();

        public async Task op_io_irq()
        {
            if (interrupt_pending())
            {
                //modify I/O cycle to bus read cycle, do not increment PC
                await op_read(regs.pc.d);
            }
            else
            {
                await op_io();
            }
		}

		public async Task op_io_cond2()
        {
            if (regs.d.l != 0x00)
            {
                await op_io();
            }
		}

		public async Task op_io_cond4(ushort x, ushort y)
        {
            if (!regs.p.x || (x & 0xff00) != (y & 0xff00))
            {
                await op_io();
            }
		}

		public async Task op_io_cond6(ushort addr)
        {
            if (regs.e && (regs.pc.w & 0xff00) != (addr & 0xff00))
            {
                await op_io();
            }
		}

		public Task op_adc_b(CPUCoreOpArgument args)
        {
            int result;

            if (!regs.p.d)
            {
                result = regs.a.l + rd.l + Convert.ToInt32(regs.p.c);
            }
            else
            {
                result = (regs.a.l & 0x0f) + (rd.l & 0x0f) + (Convert.ToInt32(regs.p.c) << 0);
                if (result > 0x09)
                {
                    result += 0x06;
                }
                regs.p.c = result > 0x0f;
                result = (regs.a.l & 0xf0) + (rd.l & 0xf0) + (Convert.ToInt32(regs.p.c) << 4) + (result & 0x0f);
            }

            regs.p.v = Convert.ToBoolean(~(regs.a.l ^ rd.l) & (regs.a.l ^ result) & 0x80);
            if (regs.p.d && result > 0x9f)
            {
                result += 0x60;
            }
            regs.p.c = result > 0xff;
            regs.p.n = Convert.ToBoolean(result & 0x80);
            regs.p.z = (byte)result == 0;

            regs.a.l = (byte)result;
	        return Task.FromResult(false);
		}

		public Task op_adc_w(CPUCoreOpArgument args)
        {
            int result;

            if (!regs.p.d)
            {
                result = regs.a.w + rd.w + Convert.ToInt32(regs.p.c);
            }
            else
            {
                result = (regs.a.w & 0x000f) + (rd.w & 0x000f) + (Convert.ToInt32(regs.p.c) << 0);
                if (result > 0x0009)
                {
                    result += 0x0006;
                }
                regs.p.c = result > 0x000f;
                result = (regs.a.w & 0x00f0) + (rd.w & 0x00f0) + (Convert.ToInt32(regs.p.c) << 4) + (result & 0x000f);
                if (result > 0x009f)
                {
                    result += 0x0060;
                }
                regs.p.c = result > 0x00ff;
                result = (regs.a.w & 0x0f00) + (rd.w & 0x0f00) + (Convert.ToInt32(regs.p.c) << 8) + (result & 0x00ff);
                if (result > 0x09ff)
                {
                    result += 0x0600;
                }
                regs.p.c = result > 0x0fff;
                result = (regs.a.w & 0xf000) + (rd.w & 0xf000) + (Convert.ToInt32(regs.p.c) << 12) + (result & 0x0fff);
            }

            regs.p.v = Convert.ToBoolean(~(regs.a.w ^ rd.w) & (regs.a.w ^ result) & 0x8000);
            if (regs.p.d && result > 0x9fff)
            {
                result += 0x6000;
            }
            regs.p.c = result > 0xffff;
            regs.p.n = Convert.ToBoolean(result & 0x8000);
            regs.p.z = (ushort)result == 0;

            regs.a.w = (ushort)result;
	        return Task.FromResult(false);
		}

		public Task op_and_b(CPUCoreOpArgument args)
        {
            regs.a.l &= rd.l;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = regs.a.l == 0;
	        return Task.FromResult(false);
		}

		public Task op_and_w(CPUCoreOpArgument args)
        {
            regs.a.w &= rd.w;
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = regs.a.w == 0;
	        return Task.FromResult(false);
		}

		public Task op_bit_b(CPUCoreOpArgument args)
        {
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.v = Convert.ToBoolean(rd.l & 0x40);
            regs.p.z = (rd.l & regs.a.l) == 0;
	        return Task.FromResult(false);
		}

		public Task op_bit_w(CPUCoreOpArgument args)
        {
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.v = Convert.ToBoolean(rd.w & 0x4000);
            regs.p.z = (rd.w & regs.a.w) == 0;
	        return Task.FromResult(false);
		}

		public Task op_cmp_b(CPUCoreOpArgument args)
        {
            int r = regs.a.l - rd.l;
            regs.p.n = Convert.ToBoolean(r & 0x80);
            regs.p.z = (byte)r == 0;
            regs.p.c = r >= 0;
	        return Task.FromResult(false);
		}

		public Task op_cmp_w(CPUCoreOpArgument args)
        {
            int r = regs.a.w - rd.w;
            regs.p.n = Convert.ToBoolean(r & 0x8000);
            regs.p.z = (ushort)r == 0;
            regs.p.c = r >= 0;
	        return Task.FromResult(false);
		}

		public Task op_cpx_b(CPUCoreOpArgument args)
        {
            int r = regs.x.l - rd.l;
            regs.p.n = Convert.ToBoolean(r & 0x80);
            regs.p.z = (byte)r == 0;
            regs.p.c = r >= 0;
	        return Task.FromResult(false);
		}

		public Task op_cpx_w(CPUCoreOpArgument args)
        {
            int r = regs.x.w - rd.w;
            regs.p.n = Convert.ToBoolean(r & 0x8000);
            regs.p.z = (ushort)r == 0;
            regs.p.c = r >= 0;
	        return Task.FromResult(false);
		}

		public Task op_cpy_b(CPUCoreOpArgument args)
        {
            int r = regs.y.l - rd.l;
            regs.p.n = Convert.ToBoolean(r & 0x80);
            regs.p.z = (byte)r == 0;
            regs.p.c = r >= 0;
	        return Task.FromResult(false);
		}

		public Task op_cpy_w(CPUCoreOpArgument args)
        {
            int r = regs.y.w - rd.w;
            regs.p.n = Convert.ToBoolean(r & 0x8000);
            regs.p.z = (ushort)r == 0;
            regs.p.c = r >= 0;
	        return Task.FromResult(false);
		}

		public Task op_eor_b(CPUCoreOpArgument args)
        {
            regs.a.l ^= rd.l;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = regs.a.l == 0;
	        return Task.FromResult(false);
		}

		public Task op_eor_w(CPUCoreOpArgument args)
        {
            regs.a.w ^= rd.w;
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = regs.a.w == 0;
	        return Task.FromResult(false);
		}

		public Task op_lda_b(CPUCoreOpArgument args)
        {
            regs.a.l = rd.l;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = regs.a.l == 0;
	        return Task.FromResult(false);
		}

		public Task op_lda_w(CPUCoreOpArgument args)
        {
            regs.a.w = rd.w;
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = regs.a.w == 0;
	        return Task.FromResult(false);
		}

		public Task op_ldx_b(CPUCoreOpArgument args)
        {
            regs.x.l = rd.l;
            regs.p.n = Convert.ToBoolean(regs.x.l & 0x80);
            regs.p.z = regs.x.l == 0;
	        return Task.FromResult(false);
		}

		public Task op_ldx_w(CPUCoreOpArgument args)
        {
            regs.x.w = rd.w;
            regs.p.n = Convert.ToBoolean(regs.x.w & 0x8000);
            regs.p.z = regs.x.w == 0;
	        return Task.FromResult(false);
		}

		public Task op_ldy_b(CPUCoreOpArgument args)
        {
            regs.y.l = rd.l;
            regs.p.n = Convert.ToBoolean(regs.y.l & 0x80);
            regs.p.z = regs.y.l == 0;
	        return Task.FromResult(false);
		}

		public Task op_ldy_w(CPUCoreOpArgument args)
        {
            regs.y.w = rd.w;
            regs.p.n = Convert.ToBoolean(regs.y.w & 0x8000);
            regs.p.z = regs.y.w == 0;
	        return Task.FromResult(false);
		}

		public Task op_ora_b(CPUCoreOpArgument args)
        {
            regs.a.l |= rd.l;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = regs.a.l == 0;
	        return Task.FromResult(false);
		}

		public Task op_ora_w(CPUCoreOpArgument args)
        {
            regs.a.w |= rd.w;
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = regs.a.w == 0;
	        return Task.FromResult(false);
		}

		public Task op_sbc_b(CPUCoreOpArgument args)
        {
            int result;
            rd.l ^= 0xff;

            if (!regs.p.d)
            {
                result = regs.a.l + rd.l + Convert.ToInt32(regs.p.c);
            }
            else
            {
                result = (regs.a.l & 0x0f) + (rd.l & 0x0f) + (Convert.ToInt32(regs.p.c) << 0);
                if (result <= 0x0f)
                {
                    result -= 0x06;
                }
                regs.p.c = result > 0x0f;
                result = (regs.a.l & 0xf0) + (rd.l & 0xf0) + (Convert.ToInt32(regs.p.c) << 4) + (result & 0x0f);
            }

            regs.p.v = Convert.ToBoolean(~(regs.a.l ^ rd.l) & (regs.a.l ^ result) & 0x80);
            if (regs.p.d && result <= 0xff)
            {
                result -= 0x60;
            }
            regs.p.c = result > 0xff;
            regs.p.n = Convert.ToBoolean(result & 0x80);
            regs.p.z = (byte)result == 0;

            regs.a.l = (byte)result;
	        return Task.FromResult(false);
		}

		public Task op_sbc_w(CPUCoreOpArgument args)
        {
            int result;
            rd.w ^= 0xffff;

            if (!regs.p.d)
            {
                result = regs.a.w + rd.w + Convert.ToInt32(regs.p.c);
            }
            else
            {
                result = (regs.a.w & 0x000f) + (rd.w & 0x000f) + (Convert.ToInt32(regs.p.c) << 0);
                if (result <= 0x000f)
                {
                    result -= 0x0006;
                }
                regs.p.c = result > 0x000f;
                result = (regs.a.w & 0x00f0) + (rd.w & 0x00f0) + (Convert.ToInt32(regs.p.c) << 4) + (result & 0x000f);
                if (result <= 0x00ff)
                {
                    result -= 0x0060;
                }
                regs.p.c = result > 0x00ff;
                result = (regs.a.w & 0x0f00) + (rd.w & 0x0f00) + (Convert.ToInt32(regs.p.c) << 8) + (result & 0x00ff);
                if (result <= 0x0fff)
                {
                    result -= 0x0600;
                }
                regs.p.c = result > 0x0fff;
                result = (regs.a.w & 0xf000) + (rd.w & 0xf000) + (Convert.ToInt32(regs.p.c) << 12) + (result & 0x0fff);
            }

            regs.p.v = Convert.ToBoolean(~(regs.a.w ^ rd.w) & (regs.a.w ^ result) & 0x8000);
            if (regs.p.d && result <= 0xffff)
            {
                result -= 0x6000;
            }
            regs.p.c = result > 0xffff;
            regs.p.n = Convert.ToBoolean(result & 0x8000);
            regs.p.z = (ushort)result == 0;

            regs.a.w = (ushort)result;
	        return Task.FromResult(false);
		}

		public Task op_inc_b(CPUCoreOpArgument args)
        {
            rd.l++;
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.z = rd.l == 0;
	        return Task.FromResult(false);
		}

		public Task op_inc_w(CPUCoreOpArgument args)
        {
            rd.w++;
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.z = rd.w == 0;
	        return Task.FromResult(false);
		}

		public Task op_dec_b(CPUCoreOpArgument args)
        {
            rd.l--;
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.z = rd.l == 0;
	        return Task.FromResult(false);
		}

		public Task op_dec_w(CPUCoreOpArgument args)
        {
            rd.w--;
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.z = rd.w == 0;
	        return Task.FromResult(false);
		}

		public Task op_asl_b(CPUCoreOpArgument args)
        {
            regs.p.c = Convert.ToBoolean(rd.l & 0x80);
            rd.l <<= 1;
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.z = rd.l == 0;
	        return Task.FromResult(false);
		}

		public Task op_asl_w(CPUCoreOpArgument args)
        {
            regs.p.c = Convert.ToBoolean(rd.w & 0x8000);
            rd.w <<= 1;
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.z = rd.w == 0;
	        return Task.FromResult(false);
		}

		public Task op_lsr_b(CPUCoreOpArgument args)
        {
            regs.p.c = Convert.ToBoolean(rd.l & 1);
            rd.l >>= 1;
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.z = rd.l == 0;
	        return Task.FromResult(false);
		}

		public Task op_lsr_w(CPUCoreOpArgument args)
        {
            regs.p.c = Convert.ToBoolean(rd.w & 1);
            rd.w >>= 1;
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.z = rd.w == 0;
	        return Task.FromResult(false);
		}

		public Task op_rol_b(CPUCoreOpArgument args)
        {
            uint carry = Convert.ToUInt32(regs.p.c);
            regs.p.c = Convert.ToBoolean(rd.l & 0x80);
            rd.l = (byte)((uint)(rd.l << 1) | carry);
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.z = rd.l == 0;
	        return Task.FromResult(false);
		}

		public Task op_rol_w(CPUCoreOpArgument args)
        {
            uint carry = Convert.ToUInt32(regs.p.c);
            regs.p.c = Convert.ToBoolean(rd.w & 0x8000);
            rd.w = (ushort)((uint)(rd.w << 1) | carry);
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.z = rd.w == 0;
	        return Task.FromResult(false);
		}

		public Task op_ror_b(CPUCoreOpArgument args)
        {
            uint carry = Convert.ToUInt32(regs.p.c) << 7;
            regs.p.c = Convert.ToBoolean(rd.l & 1);
            rd.l = (byte)(carry | (uint)(rd.l >> 1));
            regs.p.n = Convert.ToBoolean(rd.l & 0x80);
            regs.p.z = rd.l == 0;
	        return Task.FromResult(false);
		}

		public Task op_ror_w(CPUCoreOpArgument args)
        {
            uint carry = Convert.ToUInt32(regs.p.c) << 15;
            regs.p.c = Convert.ToBoolean(rd.w & 1);
            rd.w = (ushort)(carry | (uint)(rd.w >> 1));
            regs.p.n = Convert.ToBoolean(rd.w & 0x8000);
            regs.p.z = rd.w == 0;
	        return Task.FromResult(false);
		}

		public Task op_trb_b(CPUCoreOpArgument args)
        {
            regs.p.z = (rd.l & regs.a.l) == 0;
            rd.l &= (byte)(~regs.a.l);
	        return Task.FromResult(false);
		}

		public Task op_trb_w(CPUCoreOpArgument args)
        {
            regs.p.z = (rd.w & regs.a.w) == 0;
            rd.w &= (ushort)(~regs.a.w);
	        return Task.FromResult(false);
		}

		public Task op_tsb_b(CPUCoreOpArgument args)
        {
            regs.p.z = (rd.l & regs.a.l) == 0;
            rd.l |= regs.a.l;
	        return Task.FromResult(false);
		}

		public Task op_tsb_w(CPUCoreOpArgument args)
        {
            regs.p.z = (rd.w & regs.a.w) == 0;
            rd.w |= regs.a.w;
	        return Task.FromResult(false);
        }

        public async Task op_read_const_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            last_cycle();
            rd.l = await op_readpc();
			await op.Invoke(null);
        }

        public async Task op_read_const_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            rd.l = await op_readpc();
            last_cycle();
            rd.h = await op_readpc();
			await op.Invoke(null);
        }

        public async Task op_read_bit_const_b(CPUCoreOpArgument args)
        {
            last_cycle();
            rd.l = await op_readpc();
            regs.p.z = ((rd.l & regs.a.l) == 0);
        }

        public async Task op_read_bit_const_w(CPUCoreOpArgument args)
        {
            rd.l = await op_readpc();
            last_cycle();
            rd.h = await op_readpc();
            regs.p.z = ((rd.w & regs.a.w) == 0);
        }

        public async Task op_read_addr_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
            last_cycle();
            rd.l = await op_readdbr(aa.w);
			await op.Invoke(null);
        }

        public async Task op_read_addr_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
            rd.l = await op_readdbr(aa.w + 0U);
            last_cycle();
            rd.h = await op_readdbr(aa.w + 1U);
			await op.Invoke(null);
        }

        public async Task op_read_addrx_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_io_cond4(aa.w, (ushort)(aa.w + regs.x.w));
            last_cycle();
            rd.l = await op_readdbr((uint)(aa.w + regs.x.w));
			await op.Invoke(null);
        }

        public async Task op_read_addrx_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_io_cond4(aa.w, (ushort)(aa.w + regs.x.w));
            rd.l = await op_readdbr((uint)(aa.w + regs.x.w + 0));
            last_cycle();
            rd.h = await op_readdbr((uint)(aa.w + regs.x.w + 1));
			await op.Invoke(null);
        }

        public async Task op_read_addry_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_io_cond4(aa.w, (ushort)(aa.w + regs.y.w));
            last_cycle();
            rd.l = await op_readdbr((uint)(aa.w + regs.y.w));
			await op.Invoke(null);
        }

        public async Task op_read_addry_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_io_cond4(aa.w, (ushort)(aa.w + regs.y.w));
            rd.l = await op_readdbr((uint)(aa.w + regs.y.w + 0));
            last_cycle();
            rd.h = await op_readdbr((uint)(aa.w + regs.y.w + 1));
			await op.Invoke(null);
        }

        public async Task op_read_long_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
            aa.b = await op_readpc();
            last_cycle();
            rd.l = await op_readlong(aa.d);
			await op.Invoke(null);
        }

        public async Task op_read_long_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
            aa.b = await op_readpc();
            rd.l = await op_readlong(aa.d + 0);
            last_cycle();
            rd.h = await op_readlong(aa.d + 1);
			await op.Invoke(null);
        }

        public async Task op_read_longx_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
            aa.b = await op_readpc();
            last_cycle();
            rd.l = await op_readlong(aa.d + regs.x.w);
			await op.Invoke(null);
        }

        public async Task op_read_longx_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
            aa.b = await op_readpc();
            rd.l = await op_readlong(aa.d + regs.x.w + 0);
            last_cycle();
            rd.h = await op_readlong(aa.d + regs.x.w + 1);
			await op.Invoke(null);
        }

        public async Task op_read_dp_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
            last_cycle();
            rd.l = await op_readdp(dp);
			await op.Invoke(null);
        }

        public async Task op_read_dp_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
            rd.l = await op_readdp(dp + 0U);
            last_cycle();
            rd.h = await op_readdp(dp + 1U);
			await op.Invoke(null);
        }

        public async Task op_read_dpr_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            int n = args.x;
            dp = await op_readpc();
			await op_io_cond2();
			await op_io();
            last_cycle();
            rd.l = await op_readdp((uint)(dp + regs.r[n].w));
			await op.Invoke(null);
        }

        public async Task op_read_dpr_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            int n = args.x;
            dp = await op_readpc();
			await op_io_cond2();
			await op_io();
            rd.l = await op_readdp((uint)(dp + regs.r[n].w + 0));
            last_cycle();
            rd.h = await op_readdp((uint)(dp + regs.r[n].w + 1));
			await op.Invoke(null);
        }

        public async Task op_read_idp_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
            last_cycle();
            rd.l = await op_readdbr(aa.w);
			await op.Invoke(null);
        }

        public async Task op_read_idp_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
            rd.l = await op_readdbr(aa.w + 0U);
            last_cycle();
            rd.h = await op_readdbr(aa.w + 1U);
			await op.Invoke(null);
        }

        public async Task op_read_idpx_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
			await op_io();
            aa.l = await op_readdp((uint)(dp + regs.x.w + 0));
            aa.h = await op_readdp((uint)(dp + regs.x.w + 1));
            last_cycle();
            rd.l = await op_readdbr(aa.w);
			await op.Invoke(null);
        }

        public async Task op_read_idpx_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
			await op_io();
            aa.l = await op_readdp((uint)(dp + regs.x.w + 0));
            aa.h = await op_readdp((uint)(dp + regs.x.w + 1));
            rd.l = await op_readdbr(aa.w + 0U);
            last_cycle();
            rd.h = await op_readdbr(aa.w + 1U);
			await op.Invoke(null);
        }

        public async Task op_read_idpy_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
			await op_io_cond4(aa.w, (ushort)(aa.w + regs.y.w));
            last_cycle();
            rd.l = await op_readdbr((uint)(aa.w + regs.y.w));
			await op.Invoke(null);
        }

        public async Task op_read_idpy_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
			await op_io_cond4(aa.w, (ushort)(aa.w + regs.y.w));
            rd.l = await op_readdbr((uint)(aa.w + regs.y.w + 0));
            last_cycle();
            rd.h = await op_readdbr((uint)(aa.w + regs.y.w + 1));
			await op.Invoke(null);
        }

        public async Task op_read_ildp_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
            aa.b = await op_readdp(dp + 2U);
            last_cycle();
            rd.l = await op_readlong(aa.d);
			await op.Invoke(null);
        }

        public async Task op_read_ildp_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
            aa.b = await op_readdp(dp + 2U);
            rd.l = await op_readlong(aa.d + 0);
            last_cycle();
            rd.h = await op_readlong(aa.d + 1);
			await op.Invoke(null);
        }

        public async Task op_read_ildpy_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
            aa.b = await op_readdp(dp + 2U);
            last_cycle();
            rd.l = await op_readlong(aa.d + regs.y.w);
			await op.Invoke(null);
        }

        public async Task op_read_ildpy_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
            aa.b = await op_readdp(dp + 2U);
            rd.l = await op_readlong(aa.d + regs.y.w + 0);
            last_cycle();
            rd.h = await op_readlong(aa.d + regs.y.w + 1);
			await op.Invoke(null);
        }

        public async Task op_read_sr_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            sp = await op_readpc();
			await op_io();
            last_cycle();
            rd.l = await op_readsp(sp);
			await op.Invoke(null);
        }

        public async Task op_read_sr_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            sp = await op_readpc();
			await op_io();
            rd.l = await op_readsp(sp + 0U);
            last_cycle();
            rd.h = await op_readsp(sp + 1U);
			await op.Invoke(null);
        }

        public async Task op_read_isry_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            sp = await op_readpc();
			await op_io();
            aa.l = await op_readsp(sp + 0U);
            aa.h = await op_readsp(sp + 1U);
			await op_io();
            last_cycle();
            rd.l = await op_readdbr((uint)(aa.w + regs.y.w));
			await op.Invoke(null);
        }

        public async Task op_read_isry_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            sp = await op_readpc();
			await op_io();
            aa.l = await op_readsp(sp + 0U);
            aa.h = await op_readsp(sp + 1U);
			await op_io();
            rd.l = await op_readdbr((uint)(aa.w + regs.y.w + 0));
            last_cycle();
            rd.h = await op_readdbr((uint)(aa.w + regs.y.w + 1));
			await op.Invoke(null);
        }

        public async Task op_write_addr_b(CPUCoreOpArgument args)
        {
            int n = args.x;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
            last_cycle();
			await op_writedbr(aa.w, (byte)regs.r[n]);
        }

        public async Task op_write_addr_w(CPUCoreOpArgument args)
        {
            int n = args.x;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_writedbr(aa.w + 0U, (byte)(regs.r[n] >> 0));
            last_cycle();
			await op_writedbr(aa.w + 1U, (byte)(regs.r[n] >> 8));
        }

        public async Task op_write_addrr_b(CPUCoreOpArgument args)
        {
            int n = args.x;
            int i = args.y;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_io();
            last_cycle();
			await op_writedbr(aa.w + (uint)regs.r[i], (byte)regs.r[n]);
        }

        public async Task op_write_addrr_w(CPUCoreOpArgument args)
        {
            int n = args.x;
            int i = args.y;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_io();
			await op_writedbr(aa.w + (uint)regs.r[i] + 0, (byte)(regs.r[n] >> 0));
            last_cycle();
			await op_writedbr(aa.w + (uint)regs.r[i] + 1, (byte)(regs.r[n] >> 8));
        }

        public async Task op_write_longr_b(CPUCoreOpArgument args)
        {
            int i = args.x;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
            aa.b = await op_readpc();
            last_cycle();
			await op_writelong(aa.d + (uint)regs.r[i], regs.a.l);
        }

        public async Task op_write_longr_w(CPUCoreOpArgument args)
        {
            int i = args.x;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
            aa.b = await op_readpc();
			await op_writelong(aa.d + (uint)regs.r[i] + 0, regs.a.l);
            last_cycle();
			await op_writelong(aa.d + (uint)regs.r[i] + 1, regs.a.h);
        }

        public async Task op_write_dp_b(CPUCoreOpArgument args)
        {
            int n = args.x;
            dp = await op_readpc();
			await op_io_cond2();
            last_cycle();
			await op_writedp(dp, (byte)regs.r[n]);
        }

        public async Task op_write_dp_w(CPUCoreOpArgument args)
        {
            int n = args.x;
            dp = await op_readpc();
			await op_io_cond2();
			await op_writedp(dp + 0U, (byte)(regs.r[n] >> 0));
            last_cycle();
			await op_writedp(dp + 1U, (byte)(regs.r[n] >> 8));
        }

        public async Task op_write_dpr_b(CPUCoreOpArgument args)
        {
            int n = args.x;
            int i = args.y;
            dp = await op_readpc();
			await op_io_cond2();
			await op_io();
            last_cycle();
			await op_writedp(dp + (uint)regs.r[i], (byte)regs.r[n]);
        }

        public async Task op_write_dpr_w(CPUCoreOpArgument args)
        {
            int n = args.x;
            int i = args.y;
            dp = await op_readpc();
			await op_io_cond2();
			await op_io();
			await op_writedp(dp + (uint)regs.r[i] + 0, (byte)(regs.r[n] >> 0));
            last_cycle();
			await op_writedp(dp + (uint)regs.r[i] + 1, (byte)(regs.r[n] >> 8));
        }

        public async Task op_sta_idp_b(CPUCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
            last_cycle();
			await op_writedbr(aa.w, regs.a.l);
        }

        public async Task op_sta_idp_w(CPUCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
			await op_writedbr(aa.w + 0U, regs.a.l);
            last_cycle();
			await op_writedbr(aa.w + 1U, regs.a.h);
        }

        public async Task op_sta_ildp_b(CPUCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
            aa.b = await op_readdp(dp + 2U);
            last_cycle();
			await op_writelong(aa.d, regs.a.l);
        }

        public async Task op_sta_ildp_w(CPUCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
            aa.b = await op_readdp(dp + 2U);
			await op_writelong(aa.d + 0, regs.a.l);
            last_cycle();
			await op_writelong(aa.d + 1, regs.a.h);
        }

        public async Task op_sta_idpx_b(CPUCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io_cond2();
			await op_io();
            aa.l = await op_readdp((uint)(dp + regs.x.w + 0));
            aa.h = await op_readdp((uint)(dp + regs.x.w + 1));
            last_cycle();
			await op_writedbr(aa.w, regs.a.l);
        }

        public async Task op_sta_idpx_w(CPUCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io_cond2();
			await op_io();
            aa.l = await op_readdp((uint)(dp + regs.x.w + 0));
            aa.h = await op_readdp((uint)(dp + regs.x.w + 1));
			await op_writedbr(aa.w + 0U, regs.a.l);
            last_cycle();
			await op_writedbr(aa.w + 1U, regs.a.h);
        }

        public async Task op_sta_idpy_b(CPUCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
			await op_io();
            last_cycle();
			await op_writedbr((uint)(aa.w + regs.y.w), regs.a.l);
        }

        public async Task op_sta_idpy_w(CPUCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
			await op_io();
			await op_writedbr((uint)(aa.w + regs.y.w + 0), regs.a.l);
            last_cycle();
			await op_writedbr((uint)(aa.w + regs.y.w + 1), regs.a.h);
        }

        public async Task op_sta_ildpy_b(CPUCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
            aa.b = await op_readdp(dp + 2U);
            last_cycle();
			await op_writelong(aa.d + regs.y.w, regs.a.l);
        }

        public async Task op_sta_ildpy_w(CPUCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
            aa.b = await op_readdp(dp + 2U);
			await op_writelong(aa.d + regs.y.w + 0, regs.a.l);
            last_cycle();
			await op_writelong(aa.d + regs.y.w + 1, regs.a.h);
        }

        public async Task op_sta_sr_b(CPUCoreOpArgument args)
        {
            sp = await op_readpc();
			await op_io();
            last_cycle();
			await op_writesp(sp, regs.a.l);
        }

        public async Task op_sta_sr_w(CPUCoreOpArgument args)
        {
            sp = await op_readpc();
			await op_io();
			await op_writesp(sp + 0U, regs.a.l);
            last_cycle();
			await op_writesp(sp + 1U, regs.a.h);
        }

        public async Task op_sta_isry_b(CPUCoreOpArgument args)
        {
            sp = await op_readpc();
			await op_io();
            aa.l = await op_readsp(sp + 0U);
            aa.h = await op_readsp(sp + 1U);
			await op_io();
            last_cycle();
			await op_writedbr((uint)(aa.w + regs.y.w), regs.a.l);
        }

        public async Task op_sta_isry_w(CPUCoreOpArgument args)
        {
            sp = await op_readpc();
			await op_io();
            aa.l = await op_readsp(sp + 0U);
            aa.h = await op_readsp(sp + 1U);
			await op_io();
			await op_writedbr((uint)(aa.w + regs.y.w + 0), regs.a.l);
            last_cycle();
			await op_writedbr((uint)(aa.w + regs.y.w + 1), regs.a.h);
        }

        public async Task op_adjust_imm_b(CPUCoreOpArgument args)
        {
            int n = args.x;
            int adjust = args.y;
            last_cycle();
            await op_io_irq();
            regs.r[n].l += (byte)adjust;
            regs.p.n = Convert.ToBoolean(regs.r[n].l & 0x80);
            regs.p.z = (regs.r[n].l == 0);
        }

        public async Task op_adjust_imm_w(CPUCoreOpArgument args)
        {
            int n = args.x;
            int adjust = args.y;
            last_cycle();
            await op_io_irq();
            regs.r[n].w += (ushort)adjust;
            regs.p.n = Convert.ToBoolean(regs.r[n].w & 0x8000);
            regs.p.z = (regs.r[n].w == 0);
		}

		public async Task op_asl_imm_b(CPUCoreOpArgument args)
        {
            last_cycle();
            await op_io_irq();
            regs.p.c = Convert.ToBoolean(regs.a.l & 0x80);
            regs.a.l <<= 1;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = (regs.a.l == 0);
		}

		public async Task op_asl_imm_w(CPUCoreOpArgument args)
        {
            last_cycle();
            await op_io_irq();
            regs.p.c = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.a.w <<= 1;
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = (regs.a.w == 0);
		}

		public async Task op_lsr_imm_b(CPUCoreOpArgument args)
        {
            last_cycle();
            await op_io_irq();
            regs.p.c = Convert.ToBoolean(regs.a.l & 0x01);
            regs.a.l >>= 1;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = (regs.a.l == 0);
		}

		public async Task op_lsr_imm_w(CPUCoreOpArgument args)
        {
            last_cycle();
            await op_io_irq();
            regs.p.c = Convert.ToBoolean(regs.a.w & 0x0001);
            regs.a.w >>= 1;
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = (regs.a.w == 0);
		}

		public async Task op_rol_imm_b(CPUCoreOpArgument args)
        {
            last_cycle();
            await op_io_irq();
            bool carry = regs.p.c;
            regs.p.c = Convert.ToBoolean(regs.a.l & 0x80);
            regs.a.l = (byte)((regs.a.l << 1) | Convert.ToInt32(carry));
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = (regs.a.l == 0);
		}

		public async Task op_rol_imm_w(CPUCoreOpArgument args)
        {
            last_cycle();
            await op_io_irq();
            bool carry = regs.p.c;
            regs.p.c = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.a.w = (ushort)((regs.a.w << 1) | Convert.ToInt32(carry));
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = (regs.a.w == 0);
		}

		public async Task op_ror_imm_b(CPUCoreOpArgument args)
        {
            last_cycle();
            await op_io_irq();
            bool carry = regs.p.c;
            regs.p.c = Convert.ToBoolean(regs.a.l & 0x01);
            regs.a.l = (byte)((Convert.ToInt32(carry) << 7) | (regs.a.l >> 1));
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = (regs.a.l == 0);
		}

		public async Task op_ror_imm_w(CPUCoreOpArgument args)
        {
            last_cycle();
            await op_io_irq();
            bool carry = regs.p.c;
            regs.p.c = Convert.ToBoolean(regs.a.w & 0x0001);
            regs.a.w = (ushort)((Convert.ToInt32(carry) << 15) | (regs.a.w >> 1));
            regs.p.n = Convert.ToBoolean(regs.a.w & 0x8000);
            regs.p.z = (regs.a.w == 0);
		}

		public async Task op_adjust_addr_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
            rd.l = await op_readdbr(aa.w);
			await op_io();
			await op.Invoke(null);
            last_cycle();
			await op_writedbr(aa.w, rd.l);
        }

        public async Task op_adjust_addr_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
            rd.l = await op_readdbr(aa.w + 0U);
            rd.h = await op_readdbr(aa.w + 1U);
			await op_io();
			await op.Invoke(null);
			await op_writedbr(aa.w + 1U, rd.h);
            last_cycle();
			await op_writedbr(aa.w + 0U, rd.l);
        }

        public async Task op_adjust_addrx_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_io();
            rd.l = await op_readdbr((uint)(aa.w + regs.x.w));
			await op_io();
			await op.Invoke(null);
            last_cycle();
			await op_writedbr((uint)(aa.w + regs.x.w), rd.l);
        }

        public async Task op_adjust_addrx_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_io();
            rd.l = await op_readdbr((uint)(aa.w + regs.x.w + 0));
            rd.h = await op_readdbr((uint)(aa.w + regs.x.w + 1));
			await op_io();
			await op.Invoke(null);
			await op_writedbr((uint)(aa.w + regs.x.w + 1), rd.h);
            last_cycle();
			await op_writedbr((uint)(aa.w + regs.x.w + 0), rd.l);
        }

        public async Task op_adjust_dp_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
            rd.l = await op_readdp(dp);
			await op_io();
			await op.Invoke(null);
            last_cycle();
			await op_writedp(dp, rd.l);
        }

        public async Task op_adjust_dp_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
            rd.l = await op_readdp(dp + 0U);
            rd.h = await op_readdp(dp + 1U);
			await op_io();
			await op.Invoke(null);
			await op_writedp(dp + 1U, rd.h);
            last_cycle();
			await op_writedp(dp + 0U, rd.l);
        }

        public async Task op_adjust_dpx_b(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
			await op_io();
            rd.l = await op_readdp((uint)(dp + regs.x.w));
			await op_io();
			await op.Invoke(null);
            last_cycle();
			await op_writedp((uint)(dp + regs.x.w), rd.l);
        }

        public async Task op_adjust_dpx_w(CPUCoreOpArgument args)
        {
            CPUCoreOp op = args.op;
            dp = await op_readpc();
			await op_io_cond2();
			await op_io();
            rd.l = await op_readdp((uint)(dp + regs.x.w + 0));
            rd.h = await op_readdp((uint)(dp + regs.x.w + 1));
			await op_io();
			await op.Invoke(null);
			await op_writedp((uint)(dp + regs.x.w + 1), rd.h);
            last_cycle();
			await op_writedp((uint)(dp + regs.x.w + 0), rd.l);
        }

        public async Task op_branch(CPUCoreOpArgument args)
        {
            int bit = args.x;
            int val = args.y;
            if (Bit.ToBit(regs.p & (uint)bit) != val)
            {
                last_cycle();
                rd.l = await op_readpc();
            }
            else
            {
                rd.l = await op_readpc();
                aa.w = (ushort)(regs.pc.d + (sbyte)rd.l);
				await op_io_cond6(aa.w);
                last_cycle();
                await op_io();
                regs.pc.w = aa.w;
            }
        }

        public async Task op_bra(CPUCoreOpArgument args)
        {
            rd.l = await op_readpc();
            aa.w = (ushort)(regs.pc.d + (sbyte)rd.l);
			await op_io_cond6(aa.w);
            last_cycle();
			await op_io();
            regs.pc.w = aa.w;
        }

        public async Task op_brl(CPUCoreOpArgument args)
        {
            rd.l = await op_readpc();
            rd.h = await op_readpc();
            last_cycle();
			await op_io();
            regs.pc.w = (ushort)(regs.pc.d + (short)rd.w);
        }

        public async Task op_jmp_addr(CPUCoreOpArgument args)
        {
            rd.l = await op_readpc();
            last_cycle();
            rd.h = await op_readpc();
            regs.pc.w = rd.w;
        }

        public async Task op_jmp_long(CPUCoreOpArgument args)
        {
            rd.l = await op_readpc();
            rd.h = await op_readpc();
            last_cycle();
            rd.b = await op_readpc();
            regs.pc.d = rd.d & 0xffffff;
        }

        public async Task op_jmp_iaddr(CPUCoreOpArgument args)
        {
            aa.l = await op_readpc();
            aa.h = await op_readpc();
            rd.l = await op_readaddr(aa.w + 0U);
            last_cycle();
            rd.h = await op_readaddr(aa.w + 1U);
            regs.pc.w = rd.w;
        }

        public async Task op_jmp_iaddrx(CPUCoreOpArgument args)
        {
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_io();
            rd.l = await op_readpbr((uint)(aa.w + regs.x.w + 0));
            last_cycle();
            rd.h = await op_readpbr((uint)(aa.w + regs.x.w + 1));
            regs.pc.w = rd.w;
        }

        public async Task op_jmp_iladdr(CPUCoreOpArgument args)
        {
            aa.l = await op_readpc();
            aa.h = await op_readpc();
            rd.l = await op_readaddr(aa.w + 0U);
            rd.h = await op_readaddr(aa.w + 1U);
            last_cycle();
            rd.b = await op_readaddr(aa.w + 2U);
            regs.pc.d = rd.d & 0xffffff;
        }

        public async Task op_jsr_addr(CPUCoreOpArgument args)
        {
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_io();
            regs.pc.w--;
			await op_writestack(regs.pc.h);
            last_cycle();
			await op_writestack(regs.pc.l);
            regs.pc.w = aa.w;
        }

        public async Task op_jsr_long_e(CPUCoreOpArgument args)
        {
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_writestackn(regs.pc.b);
			await op_io();
            aa.b = await op_readpc();
            regs.pc.w--;
			await op_writestackn(regs.pc.h);
            last_cycle();
			await op_writestackn(regs.pc.l);
            regs.pc.d = aa.d & 0xffffff;
            regs.s.h = 0x01;
        }

        public async Task op_jsr_long_n(CPUCoreOpArgument args)
        {
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_writestackn(regs.pc.b);
			await op_io();
            aa.b = await op_readpc();
            regs.pc.w--;
			await op_writestackn(regs.pc.h);
            last_cycle();
			await op_writestackn(regs.pc.l);
            regs.pc.d = aa.d & 0xffffff;
        }

        public async Task op_jsr_iaddrx_e(CPUCoreOpArgument args)
        {
            aa.l = await op_readpc();
			await op_writestackn(regs.pc.h);
			await op_writestackn(regs.pc.l);
            aa.h = await op_readpc();
			await op_io();
            rd.l = await op_readpbr((uint)(aa.w + regs.x.w + 0));
            last_cycle();
            rd.h = await op_readpbr((uint)(aa.w + regs.x.w + 1));
            regs.pc.w = rd.w;
            regs.s.h = 0x01;
        }

        public async Task op_jsr_iaddrx_n(CPUCoreOpArgument args)
        {
            aa.l = await op_readpc();
			await op_writestackn(regs.pc.h);
			await op_writestackn(regs.pc.l);
            aa.h = await op_readpc();
			await op_io();
            rd.l = await op_readpbr((uint)(aa.w + regs.x.w + 0));
            last_cycle();
            rd.h = await op_readpbr((uint)(aa.w + regs.x.w + 1));
            regs.pc.w = rd.w;
        }

        public async Task op_rti_e(CPUCoreOpArgument args)
        {
			await op_io();
			await op_io();
            regs.p.Assign((byte)(await op_readstack() | 0x30));
            rd.l = await op_readstack();
            last_cycle();
            rd.h = await op_readstack();
            regs.pc.w = rd.w;
        }

        public async Task op_rti_n(CPUCoreOpArgument args)
        {
			await op_io();
			await op_io();
            regs.p.Assign(await op_readstack());
            if (regs.p.x)
            {
                regs.x.h = 0x00;
                regs.y.h = 0x00;
            }
            rd.l = await op_readstack();
            rd.h = await op_readstack();
            last_cycle();
            rd.b = await op_readstack();
            regs.pc.d = rd.d & 0xffffff;
            update_table();
        }

        public async Task op_rts(CPUCoreOpArgument args)
        {
			await op_io();
			await op_io();
            rd.l = await op_readstack();
            rd.h = await op_readstack();
            last_cycle();
			await op_io();
            regs.pc.w = ++rd.w;
        }

        public async Task op_rtl_e(CPUCoreOpArgument args)
        {
			await op_io();
			await op_io();
            rd.l = await op_readstackn();
            rd.h = await op_readstackn();
            last_cycle();
            rd.b = await op_readstackn();
            regs.pc.b = rd.b;
            regs.pc.w = ++rd.w;
            regs.s.h = 0x01;
        }

        public async Task op_rtl_n(CPUCoreOpArgument args)
        {
			await op_io();
			await op_io();
            rd.l = await op_readstackn();
            rd.h = await op_readstackn();
            last_cycle();
            rd.b = await op_readstackn();
            regs.pc.b = rd.b;
            regs.pc.w = ++rd.w;
        }

        public async Task op_nop(CPUCoreOpArgument args)
        {
            last_cycle();
            await op_io_irq();
		}

		public async Task op_wdm(CPUCoreOpArgument args)
        {
            last_cycle();
			await op_readpc();
        }

        public async Task op_xba(CPUCoreOpArgument args)
        {
            await op_io();
            last_cycle();
            await op_io();
            regs.a.l ^= regs.a.h;
            regs.a.h ^= regs.a.l;
            regs.a.l ^= regs.a.h;
            regs.p.n = Convert.ToBoolean(regs.a.l & 0x80);
            regs.p.z = (regs.a.l == 0);
		}

		public async Task op_move_b(CPUCoreOpArgument args)
        {
            int adjust = args.x;
            dp = await op_readpc();
            sp = await op_readpc();
            regs.db = dp;
            rd.l = await op_readlong((uint)((sp << 16) | regs.x.w));
			await op_writelong((uint)((dp << 16) | regs.y.w), rd.l);
			await op_io();
            regs.x.l += (byte)adjust;
            regs.y.l += (byte)adjust;
            last_cycle();
			await op_io();
            if (Convert.ToBoolean(regs.a.w--))
            {
                regs.pc.w -= 3;
            }
        }

        public async Task op_move_w(CPUCoreOpArgument args)
        {
            int adjust = args.x;
            dp = await op_readpc();
            sp = await op_readpc();
            regs.db = dp;
            rd.l = await op_readlong((uint)((sp << 16) | regs.x.w));
			await op_writelong((uint)((dp << 16) | regs.y.w), rd.l);
			await op_io();
            regs.x.w += (ushort)adjust;
            regs.y.w += (ushort)adjust;
            last_cycle();
			await op_io();
            if (Convert.ToBoolean(regs.a.w--))
            {
                regs.pc.w -= 3;
            }
        }

        public async Task op_interrupt_e(CPUCoreOpArgument args)
        {
            int vectorE = args.x;
            int vectorN = args.y;
			await op_readpc();
			await op_writestack(regs.pc.h);
			await op_writestack(regs.pc.l);
			await op_writestack((byte)regs.p);
            rd.l = await op_readlong((uint)(vectorE + 0));
            regs.pc.b = 0;
            regs.p.i = Convert.ToBoolean(1);
            regs.p.d = Convert.ToBoolean(0);
            last_cycle();
            rd.h = await op_readlong((uint)(vectorE + 1));
            regs.pc.w = rd.w;
        }

        public async Task op_interrupt_n(CPUCoreOpArgument args)
        {
            int vectorE = args.x;
            int vectorN = args.y;
			await op_readpc();
			await op_writestack(regs.pc.b);
			await op_writestack(regs.pc.h);
			await op_writestack(regs.pc.l);
			await op_writestack((byte)regs.p);
            rd.l = await op_readlong((uint)(vectorN + 0));
            regs.pc.b = 0x00;
            regs.p.i = Convert.ToBoolean(1);
            regs.p.d = Convert.ToBoolean(0);
            last_cycle();
            rd.h = await op_readlong((uint)(vectorN + 1));
            regs.pc.w = rd.w;
        }

        public async Task op_stp(CPUCoreOpArgument args)
        {
            regs.wai = true;
            while (regs.wai)
            {
                last_cycle();
                await op_io();
            }
        }

        public async Task op_wai(CPUCoreOpArgument args)
        {
            regs.wai = true;
            while (regs.wai)
            {
                last_cycle();
                await op_io();
            }
            await op_io();
		}

		public async Task op_xce(CPUCoreOpArgument args)
        {
            last_cycle();
            await op_io_irq();
            bool carry = regs.p.c;
            regs.p.c = regs.e;
            regs.e = carry;
            if (regs.e)
            {
                regs.p.Assign((byte)(regs.p | 0x30));
                regs.s.h = 0x01;
            }
            if (regs.p.x)
            {
                regs.x.h = 0x00;
                regs.y.h = 0x00;
            }
            update_table();
		}

		public async Task op_flag(CPUCoreOpArgument args)
        {
            int mask = args.x;
            int value = args.y;
            last_cycle();
            await op_io_irq();
            regs.p.Assign((byte)(((uint)regs.p & ~mask) | (uint)value));
		}

		public async Task op_pflag_e(CPUCoreOpArgument args)
        {
            int mode = args.x;
            rd.l = await op_readpc();
            last_cycle();
			await op_io();
            regs.p.Assign((byte)(Convert.ToBoolean(mode) ? regs.p | rd.l : (uint)regs.p & ~rd.l));
            regs.p.Assign((byte)(regs.p | 0x30));
            if (regs.p.x)
            {
                regs.x.h = 0x00;
                regs.y.h = 0x00;
            }
            update_table();
        }

        public async Task op_pflag_n(CPUCoreOpArgument args)
        {
            int mode = args.x;
            rd.l = await op_readpc();
            last_cycle();
			await op_io();
            regs.p.Assign((byte)(Convert.ToBoolean(mode) ? regs.p | rd.l : (uint)regs.p & ~rd.l));
            if (regs.p.x)
            {
                regs.x.h = 0x00;
                regs.y.h = 0x00;
            }
            update_table();
        }

        public async Task op_transfer_b(CPUCoreOpArgument args)
        {
            int from = args.x;
            int to = args.y;
            last_cycle();
            await op_io_irq();
            regs.r[to].l = regs.r[from].l;
            regs.p.n = Convert.ToBoolean(regs.r[to].l & 0x80);
            regs.p.z = (regs.r[to].l == 0);
        }

        public async Task op_transfer_w(CPUCoreOpArgument args)
        {
            int from = args.x;
            int to = args.y;
            last_cycle();
            await op_io_irq();
            regs.r[to].w = regs.r[from].w;
            regs.p.n = Convert.ToBoolean(regs.r[to].w & 0x8000);
            regs.p.z = (regs.r[to].w == 0);
		}

		public async Task op_tcs_e(CPUCoreOpArgument args)
        {
            last_cycle();
            await op_io_irq();
            regs.s.l = regs.a.l;
		}

		public async Task op_tcs_n(CPUCoreOpArgument args)
        {
            last_cycle();
			await op_io_irq();
            regs.s.w = regs.a.w;
		}

		public async Task op_tsx_b(CPUCoreOpArgument args)
        {
            last_cycle();
			await op_io_irq();
            regs.x.l = regs.s.l;
            regs.p.n = Convert.ToBoolean(regs.x.l & 0x80);
            regs.p.z = (regs.x.l == 0);
		}

		public async Task op_tsx_w(CPUCoreOpArgument args)
        {
            last_cycle();
			await op_io_irq();
            regs.x.w = regs.s.w;
            regs.p.n = Convert.ToBoolean(regs.x.w & 0x8000);
            regs.p.z = (regs.x.w == 0);
		}

		public async Task op_txs_e(CPUCoreOpArgument args)
        {
            last_cycle();
			await op_io_irq();
            regs.s.l = regs.x.l;
		}

		public async Task op_txs_n(CPUCoreOpArgument args)
        {
            last_cycle();
			await op_io_irq();
            regs.s.w = regs.x.w;
		}

		public async Task op_push_b(CPUCoreOpArgument args)
        {
            int n = args.x;
			await op_io();
            last_cycle();
			await op_writestack(regs.r[n].l);
		}

		public async Task op_push_w(CPUCoreOpArgument args)
        {
            int n = args.x;
			await op_io();
			await op_writestack(regs.r[n].h);
            last_cycle();
			await op_writestack(regs.r[n].l);
		}

		public async Task op_phd_e(CPUCoreOpArgument args)
        {
			await op_io();
			await op_writestackn(regs.d.h);
            last_cycle();
			await op_writestackn(regs.d.l);
            regs.s.h = 0x01;
		}

		public async Task op_phd_n(CPUCoreOpArgument args)
        {
			await op_io();
			await op_writestackn(regs.d.h);
            last_cycle();
			await op_writestackn(regs.d.l);
		}

		public async Task op_phb(CPUCoreOpArgument args)
        {
			await op_io();
            last_cycle();
			await op_writestack(regs.db);
		}

		public async Task op_phk(CPUCoreOpArgument args)
        {
			await op_io();
            last_cycle();
			await op_writestack(regs.pc.b);
		}

		public async Task op_php(CPUCoreOpArgument args)
        {
			await op_io();
            last_cycle();
			await op_writestack((byte)regs.p);
		}

		public async Task op_pull_b(CPUCoreOpArgument args)
        {
            int n = args.x;
			await op_io();
			await op_io();
            last_cycle();
            regs.r[n].l = await op_readstack();
            regs.p.n = Convert.ToBoolean(regs.r[n].l & 0x80);
            regs.p.z = (regs.r[n].l == 0);
        }

        public async Task op_pull_w(CPUCoreOpArgument args)
        {
            int n = args.x;
			await op_io();
			await op_io();
            regs.r[n].l = await op_readstack();
            last_cycle();
            regs.r[n].h = await op_readstack();
            regs.p.n = Convert.ToBoolean(regs.r[n].w & 0x8000);
            regs.p.z = (regs.r[n].w == 0);
        }

        public async Task op_pld_e(CPUCoreOpArgument args)
        {
			await op_io();
			await op_io();
            regs.d.l = await op_readstackn();
            last_cycle();
            regs.d.h = await op_readstackn();
            regs.p.n = Convert.ToBoolean(regs.d.w & 0x8000);
            regs.p.z = (regs.d.w == 0);
            regs.s.h = 0x01;
        }

        public async Task op_pld_n(CPUCoreOpArgument args)
        {
			await op_io();
			await op_io();
            regs.d.l = await op_readstackn();
            last_cycle();
            regs.d.h = await op_readstackn();
            regs.p.n = Convert.ToBoolean(regs.d.w & 0x8000);
            regs.p.z = (regs.d.w == 0);
        }

        public async Task op_plb(CPUCoreOpArgument args)
        {
			await op_io();
			await op_io();
            last_cycle();
            regs.db = await op_readstack();
            regs.p.n = Convert.ToBoolean(regs.db & 0x80);
            regs.p.z = (regs.db == 0);
        }

        public async Task op_plp_e(CPUCoreOpArgument args)
        {
			await op_io();
			await op_io();
            last_cycle();
            regs.p.Assign((byte)(await op_readstack() | 0x30));
            if (regs.p.x)
            {
                regs.x.h = 0x00;
                regs.y.h = 0x00;
            }
            update_table();
        }

        public async Task op_plp_n(CPUCoreOpArgument args)
        {
			await op_io();
			await op_io();
            last_cycle();
            regs.p.Assign(await op_readstack());
            if (regs.p.x)
            {
                regs.x.h = 0x00;
                regs.y.h = 0x00;
            }
            update_table();
        }

        public async Task op_pea_e(CPUCoreOpArgument args)
        {
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_writestackn(aa.h);
            last_cycle();
			await op_writestackn(aa.l);
            regs.s.h = 0x01;
        }

        public async Task op_pea_n(CPUCoreOpArgument args)
        {
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_writestackn(aa.h);
            last_cycle();
			await op_writestackn(aa.l);
        }

        public async Task op_pei_e(CPUCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
			await op_writestackn(aa.h);
            last_cycle();
			await op_writestackn(aa.l);
            regs.s.h = 0x01;
        }

        public async Task op_pei_n(CPUCoreOpArgument args)
        {
            dp = await op_readpc();
			await op_io_cond2();
            aa.l = await op_readdp(dp + 0U);
            aa.h = await op_readdp(dp + 1U);
			await op_writestackn(aa.h);
            last_cycle();
			await op_writestackn(aa.l);
        }

        public async Task op_per_e(CPUCoreOpArgument args)
        {
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_io();
            rd.w = (ushort)(regs.pc.d + (short)aa.w);
			await op_writestackn(rd.h);
            last_cycle();
			await op_writestackn(rd.l);
            regs.s.h = 0x01;
        }

        public async Task op_per_n(CPUCoreOpArgument args)
        {
            aa.l = await op_readpc();
            aa.h = await op_readpc();
			await op_io();
            rd.w = (ushort)(regs.pc.d + (short)aa.w);
			await op_writestackn(rd.h);
            last_cycle();
			await op_writestackn(rd.l);
        }

        public ArraySegment<CPUCoreOperation> opcode_table;
        public CPUCoreOperation[] op_table = new CPUCoreOperation[256 * 5];

        private CPUCoreOp GetCoreOp(string name)
        {
            return GetCoreOp(name, string.Empty);
        }

        private CPUCoreOp GetCoreOp(string name, string modifier)
        {
            return (CPUCoreOp)Delegate.CreateDelegate(typeof(CPUCoreOp), this, "op_" + name + modifier);
        }

        private void opA(byte id, string name)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name), null);
        }

        private void opAII(byte id, string name, int x, int y)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name), new CPUCoreOpArgument() { x = x, y = y });
        }

        private void opE(byte id, string name)
        {
            op_table[(int)Table.EM + id] = new CPUCoreOperation(GetCoreOp(name, "_e"), null);
            op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_n"), null);
        }

        private void opEI(byte id, string name, int x)
        {
            op_table[(int)Table.EM + id] = new CPUCoreOperation(GetCoreOp(name, "_e"), new CPUCoreOpArgument() { x = x });
            op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_n"), new CPUCoreOpArgument() { x = x });
        }

        private void opEII(byte id, string name, int x, int y)
        {
            op_table[(int)Table.EM + id] = new CPUCoreOperation(GetCoreOp(name, "_e"), new CPUCoreOpArgument() { x = x, y = y });
            op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_n"), new CPUCoreOpArgument() { x = x, y = y });
        }

        private void opM(byte id, string name)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), null);
            op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), null);
        }

        private void opMI(byte id, string name, int x)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { x = x });
            op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { x = x });
        }

        private void opMII(byte id, string name, int x, int y)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { x = x, y = y });
            op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { x = x, y = y });
        }

        private void opMF(byte id, string name, string fn)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_b") });
            op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_w") });
        }

        private void opMFI(byte id, string name, string fn, int x)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.Mx + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_b"), x = x });
            op_table[(int)Table.mX + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_w"), x = x });
        }

        private void opX(byte id, string name)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.mX + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), null);
            op_table[(int)Table.Mx + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), null);
        }

        private void opXI(byte id, string name, int x)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.mX + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { x = x });
            op_table[(int)Table.Mx + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { x = x });
        }

        private void opXII(byte id, string name, int x, int y)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.mX + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { x = x, y = y });
            op_table[(int)Table.Mx + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { x = x, y = y });
        }

        private void opXF(byte id, string name, string fn)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.mX + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_b") });
            op_table[(int)Table.Mx + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_w") });
        }

        private void opXFI(byte id, string name, string fn, int x)
        {
            op_table[(int)Table.EM + id] = op_table[(int)Table.MX + id] = op_table[(int)Table.mX + id] = new CPUCoreOperation(GetCoreOp(name, "_b"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_b"), x = x });
            op_table[(int)Table.Mx + id] = op_table[(int)Table.mx + id] = new CPUCoreOperation(GetCoreOp(name, "_w"), new CPUCoreOpArgument() { op = GetCoreOp(fn, "_w"), x = x });
        }

        public void initialize_opcode_table()
        {
            opEII(0x00, "interrupt", 0xfffe, 0xffe6);
            opMF(0x01, "read_idpx", "ora");
            opEII(0x02, "interrupt", 0xfff4, 0xffe4);
            opMF(0x03, "read_sr", "ora");
            opMF(0x04, "adjust_dp", "tsb");
            opMF(0x05, "read_dp", "ora");
            opMF(0x06, "adjust_dp", "asl");
            opMF(0x07, "read_ildp", "ora");
            opA(0x08, "php");
            opMF(0x09, "read_const", "ora");
            opM(0x0a, "asl_imm");
            opE(0x0b, "phd");
            opMF(0x0c, "adjust_addr", "tsb");
            opMF(0x0d, "read_addr", "ora");
            opMF(0x0e, "adjust_addr", "asl");
            opMF(0x0f, "read_long", "ora");
            opAII(0x10, "branch", 0x80, Convert.ToInt32(false));
            opMF(0x11, "read_idpy", "ora");
            opMF(0x12, "read_idp", "ora");
            opMF(0x13, "read_isry", "ora");
            opMF(0x14, "adjust_dp", "trb");
            opMFI(0x15, "read_dpr", "ora", (int)OpCode.X);
            opMF(0x16, "adjust_dpx", "asl");
            opMF(0x17, "read_ildpy", "ora");
            opAII(0x18, "flag", 0x01, 0x00);
            opMF(0x19, "read_addry", "ora");
            opMII(0x1a, "adjust_imm", (int)OpCode.A, +1);
            opE(0x1b, "tcs");
            opMF(0x1c, "adjust_addr", "trb");
            opMF(0x1d, "read_addrx", "ora");
            opMF(0x1e, "adjust_addrx", "asl");
            opMF(0x1f, "read_longx", "ora");
            opA(0x20, "jsr_addr");
            opMF(0x21, "read_idpx", "and");
            opE(0x22, "jsr_long");
            opMF(0x23, "read_sr", "and");
            opMF(0x24, "read_dp", "bit");
            opMF(0x25, "read_dp", "and");
            opMF(0x26, "adjust_dp", "rol");
            opMF(0x27, "read_ildp", "and");
            opE(0x28, "plp");
            opMF(0x29, "read_const", "and");
            opM(0x2a, "rol_imm");
            opE(0x2b, "pld");
            opMF(0x2c, "read_addr", "bit");
            opMF(0x2d, "read_addr", "and");
            opMF(0x2e, "adjust_addr", "rol");
            opMF(0x2f, "read_long", "and");
            opAII(0x30, "branch", 0x80, Convert.ToInt32(true));
            opMF(0x31, "read_idpy", "and");
            opMF(0x32, "read_idp", "and");
            opMF(0x33, "read_isry", "and");
            opMFI(0x34, "read_dpr", "bit", (int)OpCode.X);
            opMFI(0x35, "read_dpr", "and", (int)OpCode.X);
            opMF(0x36, "adjust_dpx", "rol");
            opMF(0x37, "read_ildpy", "and");
            opAII(0x38, "flag", 0x01, 0x01);
            opMF(0x39, "read_addry", "and");
            opMII(0x3a, "adjust_imm", (int)OpCode.A, -1);
            opAII(0x3b, "transfer_w", (int)OpCode.S, (int)OpCode.A);
            opMF(0x3c, "read_addrx", "bit");
            opMF(0x3d, "read_addrx", "and");
            opMF(0x3e, "adjust_addrx", "rol");
            opMF(0x3f, "read_longx", "and");
            opE(0x40, "rti");
            opMF(0x41, "read_idpx", "eor");
            opA(0x42, "wdm");
            opMF(0x43, "read_sr", "eor");
            opXI(0x44, "move", -1);
            opMF(0x45, "read_dp", "eor");
            opMF(0x46, "adjust_dp", "lsr");
            opMF(0x47, "read_ildp", "eor");
            opMI(0x48, "push", (int)OpCode.A);
            opMF(0x49, "read_const", "eor");
            opM(0x4a, "lsr_imm");
            opA(0x4b, "phk");
            opA(0x4c, "jmp_addr");
            opMF(0x4d, "read_addr", "eor");
            opMF(0x4e, "adjust_addr", "lsr");
            opMF(0x4f, "read_long", "eor");
            opAII(0x50, "branch", 0x40, Convert.ToInt32(false));
            opMF(0x51, "read_idpy", "eor");
            opMF(0x52, "read_idp", "eor");
            opMF(0x53, "read_isry", "eor");
            opXI(0x54, "move", +1);
            opMFI(0x55, "read_dpr", "eor", (int)OpCode.X);
            opMF(0x56, "adjust_dpx", "lsr");
            opMF(0x57, "read_ildpy", "eor");
            opAII(0x58, "flag", 0x04, 0x00);
            opMF(0x59, "read_addry", "eor");
            opXI(0x5a, "push", (int)OpCode.Y);
            opAII(0x5b, "transfer_w", (int)OpCode.A, (int)OpCode.D);
            opA(0x5c, "jmp_long");
            opMF(0x5d, "read_addrx", "eor");
            opMF(0x5e, "adjust_addrx", "lsr");
            opMF(0x5f, "read_longx", "eor");
            opA(0x60, "rts");
            opMF(0x61, "read_idpx", "adc");
            opE(0x62, "per");
            opMF(0x63, "read_sr", "adc");
            opMI(0x64, "write_dp", (int)OpCode.Z);
            opMF(0x65, "read_dp", "adc");
            opMF(0x66, "adjust_dp", "ror");
            opMF(0x67, "read_ildp", "adc");
            opMI(0x68, "pull", (int)OpCode.A);
            opMF(0x69, "read_const", "adc");
            opM(0x6a, "ror_imm");
            opE(0x6b, "rtl");
            opA(0x6c, "jmp_iaddr");
            opMF(0x6d, "read_addr", "adc");
            opMF(0x6e, "adjust_addr", "ror");
            opMF(0x6f, "read_long", "adc");
            opAII(0x70, "branch", 0x40, Convert.ToInt32(true));
            opMF(0x71, "read_idpy", "adc");
            opMF(0x72, "read_idp", "adc");
            opMF(0x73, "read_isry", "adc");
            opMII(0x74, "write_dpr", (int)OpCode.Z, (int)OpCode.X);
            opMFI(0x75, "read_dpr", "adc", (int)OpCode.X);
            opMF(0x76, "adjust_dpx", "ror");
            opMF(0x77, "read_ildpy", "adc");
            opAII(0x78, "flag", 0x04, 0x04);
            opMF(0x79, "read_addry", "adc");
            opXI(0x7a, "pull", (int)OpCode.Y);
            opAII(0x7b, "transfer_w", (int)OpCode.D, (int)OpCode.A);
            opA(0x7c, "jmp_iaddrx");
            opMF(0x7d, "read_addrx", "adc");
            opMF(0x7e, "adjust_addrx", "ror");
            opMF(0x7f, "read_longx", "adc");
            opA(0x80, "bra");
            opM(0x81, "sta_idpx");
            opA(0x82, "brl");
            opM(0x83, "sta_sr");
            opXI(0x84, "write_dp", (int)OpCode.Y);
            opMI(0x85, "write_dp", (int)OpCode.A);
            opXI(0x86, "write_dp", (int)OpCode.X);
            opM(0x87, "sta_ildp");
            opXII(0x88, "adjust_imm", (int)OpCode.Y, -1);
            opM(0x89, "read_bit_const");
            opMII(0x8a, "transfer", (int)OpCode.X, (int)OpCode.A);
            opA(0x8b, "phb");
            opXI(0x8c, "write_addr", (int)OpCode.Y);
            opMI(0x8d, "write_addr", (int)OpCode.A);
            opXI(0x8e, "write_addr", (int)OpCode.X);
            opMI(0x8f, "write_longr", (int)OpCode.Z);
            opAII(0x90, "branch", 0x01, Convert.ToInt32(false));
            opM(0x91, "sta_idpy");
            opM(0x92, "sta_idp");
            opM(0x93, "sta_isry");
            opXII(0x94, "write_dpr", (int)OpCode.Y, (int)OpCode.X);
            opMII(0x95, "write_dpr", (int)OpCode.A, (int)OpCode.X);
            opXII(0x96, "write_dpr", (int)OpCode.X, (int)OpCode.Y);
            opM(0x97, "sta_ildpy");
            opMII(0x98, "transfer", (int)OpCode.Y, (int)OpCode.A);
            opMII(0x99, "write_addrr", (int)OpCode.A, (int)OpCode.Y);
            opE(0x9a, "txs");
            opXII(0x9b, "transfer", (int)OpCode.X, (int)OpCode.Y);
            opMI(0x9c, "write_addr", (int)OpCode.Z);
            opMII(0x9d, "write_addrr", (int)OpCode.A, (int)OpCode.X);
            opMII(0x9e, "write_addrr", (int)OpCode.Z, (int)OpCode.X);
            opMI(0x9f, "write_longr", (int)OpCode.X);
            opXF(0xa0, "read_const", "ldy");
            opMF(0xa1, "read_idpx", "lda");
            opXF(0xa2, "read_const", "ldx");
            opMF(0xa3, "read_sr", "lda");
            opXF(0xa4, "read_dp", "ldy");
            opMF(0xa5, "read_dp", "lda");
            opXF(0xa6, "read_dp", "ldx");
            opMF(0xa7, "read_ildp", "lda");
            opXII(0xa8, "transfer", (int)OpCode.A, (int)OpCode.Y);
            opMF(0xa9, "read_const", "lda");
            opXII(0xaa, "transfer", (int)OpCode.A, (int)OpCode.X);
            opA(0xab, "plb");
            opXF(0xac, "read_addr", "ldy");
            opMF(0xad, "read_addr", "lda");
            opXF(0xae, "read_addr", "ldx");
            opMF(0xaf, "read_long", "lda");
            opAII(0xb0, "branch", 0x01, Convert.ToInt32(true));
            opMF(0xb1, "read_idpy", "lda");
            opMF(0xb2, "read_idp", "lda");
            opMF(0xb3, "read_isry", "lda");
            opXFI(0xb4, "read_dpr", "ldy", (int)OpCode.X);
            opMFI(0xb5, "read_dpr", "lda", (int)OpCode.X);
            opXFI(0xb6, "read_dpr", "ldx", (int)OpCode.Y);
            opMF(0xb7, "read_ildpy", "lda");
            opAII(0xb8, "flag", 0x40, 0x00);
            opMF(0xb9, "read_addry", "lda");
            opX(0xba, "tsx");
            opXII(0xbb, "transfer", (int)OpCode.Y, (int)OpCode.X);
            opXF(0xbc, "read_addrx", "ldy");
            opMF(0xbd, "read_addrx", "lda");
            opXF(0xbe, "read_addry", "ldx");
            opMF(0xbf, "read_longx", "lda");
            opXF(0xc0, "read_const", "cpy");
            opMF(0xc1, "read_idpx", "cmp");
            opEI(0xc2, "pflag", 0);
            opMF(0xc3, "read_sr", "cmp");
            opXF(0xc4, "read_dp", "cpy");
            opMF(0xc5, "read_dp", "cmp");
            opMF(0xc6, "adjust_dp", "dec");
            opMF(0xc7, "read_ildp", "cmp");
            opXII(0xc8, "adjust_imm", (int)OpCode.Y, +1);
            opMF(0xc9, "read_const", "cmp");
            opXII(0xca, "adjust_imm", (int)OpCode.X, -1);
            opA(0xcb, "wai");
            opXF(0xcc, "read_addr", "cpy");
            opMF(0xcd, "read_addr", "cmp");
            opMF(0xce, "adjust_addr", "dec");
            opMF(0xcf, "read_long", "cmp");
            opAII(0xd0, "branch", 0x02, Convert.ToInt32(false));
            opMF(0xd1, "read_idpy", "cmp");
            opMF(0xd2, "read_idp", "cmp");
            opMF(0xd3, "read_isry", "cmp");
            opE(0xd4, "pei");
            opMFI(0xd5, "read_dpr", "cmp", (int)OpCode.X);
            opMF(0xd6, "adjust_dpx", "dec");
            opMF(0xd7, "read_ildpy", "cmp");
            opAII(0xd8, "flag", 0x08, 0x00);
            opMF(0xd9, "read_addry", "cmp");
            opXI(0xda, "push", (int)OpCode.X);
            opA(0xdb, "stp");
            opA(0xdc, "jmp_iladdr");
            opMF(0xdd, "read_addrx", "cmp");
            opMF(0xde, "adjust_addrx", "dec");
            opMF(0xdf, "read_longx", "cmp");
            opXF(0xe0, "read_const", "cpx");
            opMF(0xe1, "read_idpx", "sbc");
            opEI(0xe2, "pflag", 1);
            opMF(0xe3, "read_sr", "sbc");
            opXF(0xe4, "read_dp", "cpx");
            opMF(0xe5, "read_dp", "sbc");
            opMF(0xe6, "adjust_dp", "inc");
            opMF(0xe7, "read_ildp", "sbc");
            opXII(0xe8, "adjust_imm", (int)OpCode.X, +1);
            opMF(0xe9, "read_const", "sbc");
            opA(0xea, "nop");
            opA(0xeb, "xba");
            opXF(0xec, "read_addr", "cpx");
            opMF(0xed, "read_addr", "sbc");
            opMF(0xee, "adjust_addr", "inc");
            opMF(0xef, "read_long", "sbc");
            opAII(0xf0, "branch", 0x02, Convert.ToInt32(true));
            opMF(0xf1, "read_idpy", "sbc");
            opMF(0xf2, "read_idp", "sbc");
            opMF(0xf3, "read_isry", "sbc");
            opE(0xf4, "pea");
            opMFI(0xf5, "read_dpr", "sbc", (int)OpCode.X);
            opMF(0xf6, "adjust_dpx", "inc");
            opMF(0xf7, "read_ildpy", "sbc");
            opAII(0xf8, "flag", 0x08, 0x08);
            opMF(0xf9, "read_addry", "sbc");
            opXI(0xfa, "pull", (int)OpCode.X);
            opA(0xfb, "xce");
            opE(0xfc, "jsr_iaddrx");
            opMF(0xfd, "read_addrx", "sbc");
            opMF(0xfe, "adjust_addrx", "inc");
            opMF(0xff, "read_longx", "sbc");
        }

        public void update_table()
        {
            if (regs.e)
            {
                opcode_table = new ArraySegment<CPUCoreOperation>(op_table, (int)Table.EM, op_table.Length - (int)Table.EM);
            }
            else if (regs.p.m)
            {
                if (regs.p.x)
                {
                    opcode_table = new ArraySegment<CPUCoreOperation>(op_table, (int)Table.MX, op_table.Length - (int)Table.MX);
                }
                else
                {
                    opcode_table = new ArraySegment<CPUCoreOperation>(op_table, (int)Table.Mx, op_table.Length - (int)Table.Mx);
                }
            }
            else
            {
                if (regs.p.x)
                {
                    opcode_table = new ArraySegment<CPUCoreOperation>(op_table, (int)Table.mX, op_table.Length - (int)Table.mX);
                }
                else
                {
                    opcode_table = new ArraySegment<CPUCoreOperation>(op_table, (int)Table.mx, op_table.Length - (int)Table.mx);
                }
            }
        }

        public enum Table { EM = 0, MX = 256, Mx = 512, mX = 768, mx = 1024 }
        private enum OpCode { A = 0, X = 1, Y = 2, Z = 3, S = 4, D = 5 };

        public void core_serialize(Serializer s)
        {
            s.integer(regs.pc.d, "regs.pc.d");

            s.integer(regs.a.w, "regs.a.w");
            s.integer(regs.x.w, "regs.x.w");
            s.integer(regs.y.w, "regs.y.w");
            s.integer(regs.z.w, "regs.z.w");
            s.integer(regs.s.w, "regs.s.w");
            s.integer(regs.d.w, "regs.d.w");

            s.integer(regs.p.n, "regs.p.n");
            s.integer(regs.p.v, "regs.p.v");
            s.integer(regs.p.m, "regs.p.m");
            s.integer(regs.p.x, "regs.p.x");
            s.integer(regs.p.d, "regs.p.d");
            s.integer(regs.p.i, "regs.p.i");
            s.integer(regs.p.z, "regs.p.z");
            s.integer(regs.p.c, "regs.p.c");

            s.integer(regs.db, "regs.db");
            s.integer(regs.e, "regs.e");
            s.integer(regs.irq, "regs.irq");
            s.integer(regs.wai, "regs.wai");
            s.integer(regs.mdr, "regs.mdr");

            s.integer(aa.d, "aa.d");
            s.integer(rd.d, "rd.d");
            s.integer(sp, "sp");
            s.integer(dp, "dp");

            update_table();
        }

        public CPUCore()
        {
            initialize_opcode_table();
        }
    }
}
