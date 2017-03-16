using System;
using System.Collections.ObjectModel;
using System.Text;
using Nall;

namespace Snes
{
    class Cheat : Collection<CheatCode>
    {
        public static Cheat cheat = new Cheat();

        public enum Type : uint { ProActionReplay, GameGenie }

        public bool enabled()
        {
            return system_enabled;
        }

        public void enable(bool state)
        {
            system_enabled = state;
            cheat_enabled = system_enabled && code_enabled;
        }

        public void synchronize()
        {
            bitmask.Initialize();
            code_enabled = false;

            for (uint i = 0; i < Count; i++)
            {
                CheatCode code = this[(int)i];
                if (code.enabled == false)
                {
                    continue;
                }

                for (uint n = 0; n < code.addr.Count; n++)
                {
                    code_enabled = true;

                    uint addr = mirror(code.addr[(int)n]);
                    bitmask[addr >> 3] |= (byte)(1 << (int)(addr & 7));
                    if ((addr & 0xffe000) == 0x7e0000)
                    {
                        //mirror $7e:0000-1fff to $00-3f|80-bf:0000-1fff
                        uint mirroraddr;
                        for (uint x = 0; x <= 0x3f; x++)
                        {
                            mirroraddr = ((0x00 + x) << 16) + (addr & 0x1fff);
                            bitmask[mirroraddr >> 3] |= (byte)(1 << (int)(mirroraddr & 7));

                            mirroraddr = ((0x80 + x) << 16) + (addr & 0x1fff);
                            bitmask[mirroraddr >> 3] |= (byte)(1 << (int)(mirroraddr & 7));
                        }
                    }
                }
            }

            cheat_enabled = system_enabled && code_enabled;
        }

        public bool read(uint addr, out byte data)
        {
            addr = mirror(addr);

            for (uint i = 0; i < Count; i++)
            {
                CheatCode code = this[(int)i];
                if (code.enabled == false)
                {
                    continue;
                }

                for (uint n = 0; n < code.addr.Count; n++)
                {
                    if (addr == mirror(code.addr[(int)n]))
                    {
                        data = code.data[(int)n];
                        return true;
                    }
                }
            }

            data = 0;
            return false;
        }

        public bool active()
        {
            return cheat_enabled;
        }

        public bool exists(uint addr)
        {
            return Convert.ToBoolean((bitmask[addr >> 3] & 1 << (int)(addr & 7)));
        }

        public Cheat()
        {
            system_enabled = true;
            synchronize();
        }

        private static bool ischr(char n)
        {
            return ((n >= '0' && n <= '9') || (n >= 'a' && n <= 'f'));
        }

        private static string transform(string dest, string before, string after)
        {
            StringBuilder Dest = new StringBuilder(dest);

            if (ReferenceEquals(dest, null) || ReferenceEquals(before, null) || ReferenceEquals(after, null))
            {
                return Dest.ToString();
            }
            int sl = Dest.Length, bsl = before.Length, asl = after.Length;

            if (bsl != asl || bsl == 0)
            {
                return Dest.ToString();  //patterns must be the same length for 1:1 replace
            }
            for (uint i = 0; i < sl; i++)
            {
                for (uint l = 0; l < bsl; l++)
                {
                    if (Dest[(int)i] == before[(int)l])
                    {
                        Dest[(int)i] = after[(int)l];
                        break;
                    }
                }
            }

            return Dest.ToString();
        }

        public static bool decode(string s, out uint addr, out byte data, out Type type)
        {
            string t = s;
            t = t.ToLower();

            addr = data = 0;
            type = Type.GameGenie;

            if (t.Length == 8 || (t.Length == 9 && t[6] == ':'))
            {
                //strip ':'
                if (t.Length == 9 && t[6] == ':')
                {
                    t = string.Empty + t.Substring(0, 6) + t.Substring(7);
                }
                //validate input
                for (uint i = 0; i < 8; i++)
                {
                    if (!ischr(t[(int)i]))
                    {
                        return false;
                    }
                }

                type = Type.ProActionReplay;
                uint r = Convert.ToUInt32(t, 16);
                addr = r >> 8;
                data = (byte)(r & 0xff);
                return true;
            }
            else if (t.Length == 9 && t[4] == '-')
            {
                //strip '-'
                t = string.Empty + t.Substring(0, 4) + t.Substring(5);
                //validate input
                for (uint i = 0; i < 8; i++)
                {
                    if (!ischr(t[(int)i]))
                    {
                        return false;
                    }
                }

                type = Type.GameGenie;
                transform(t, "df4709156bc8a23e", "0123456789abcdef");
                uint r = Convert.ToUInt32(t, 16);
                //8421 8421 8421 8421 8421 8421
                //abcd efgh ijkl mnop qrst uvwx
                //ijkl qrst opab cduv wxef ghmn
                addr = (Bit.ToBit(r & 0x002000) << 23) | (Bit.ToBit(r & 0x001000) << 22)
                     | (Bit.ToBit(r & 0x000800) << 21) | (Bit.ToBit(r & 0x000400) << 20)
                     | (Bit.ToBit(r & 0x000020) << 19) | (Bit.ToBit(r & 0x000010) << 18)
                     | (Bit.ToBit(r & 0x000008) << 17) | (Bit.ToBit(r & 0x000004) << 16)
                     | (Bit.ToBit(r & 0x800000) << 15) | (Bit.ToBit(r & 0x400000) << 14)
                     | (Bit.ToBit(r & 0x200000) << 13) | (Bit.ToBit(r & 0x100000) << 12)
                     | (Bit.ToBit(r & 0x000002) << 11) | (Bit.ToBit(r & 0x000001) << 10)
                     | (Bit.ToBit(r & 0x008000) << 9) | (Bit.ToBit(r & 0x004000) << 8)
                     | (Bit.ToBit(r & 0x080000) << 7) | (Bit.ToBit(r & 0x040000) << 6)
                     | (Bit.ToBit(r & 0x020000) << 5) | (Bit.ToBit(r & 0x010000) << 4)
                     | (Bit.ToBit(r & 0x000200) << 3) | (Bit.ToBit(r & 0x000100) << 2)
                     | (Bit.ToBit(r & 0x000080) << 1) | (Bit.ToBit(r & 0x000040) << 0);
                data = (byte)(r >> 24);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool encode(string s, uint addr, byte data, Type type)
        {
            char[] t = new char[16];

            if (type == Type.ProActionReplay)
            {
                s = addr.ToString("X6") + data.ToString("X2");
                return true;
            }
            else if (type == Type.GameGenie)
            {
                uint r = addr;
                addr = (Bit.ToBit(r & 0x008000) << 23) | (Bit.ToBit(r & 0x004000) << 22)
                     | (Bit.ToBit(r & 0x002000) << 21) | (Bit.ToBit(r & 0x001000) << 20)
                     | (Bit.ToBit(r & 0x000080) << 19) | (Bit.ToBit(r & 0x000040) << 18)
                     | (Bit.ToBit(r & 0x000020) << 17) | (Bit.ToBit(r & 0x000010) << 16)
                     | (Bit.ToBit(r & 0x000200) << 15) | (Bit.ToBit(r & 0x000100) << 14)
                     | (Bit.ToBit(r & 0x800000) << 13) | (Bit.ToBit(r & 0x400000) << 12)
                     | (Bit.ToBit(r & 0x200000) << 11) | (Bit.ToBit(r & 0x100000) << 10)
                     | (Bit.ToBit(r & 0x000008) << 9) | (Bit.ToBit(r & 0x000004) << 8)
                     | (Bit.ToBit(r & 0x000002) << 7) | (Bit.ToBit(r & 0x000001) << 6)
                     | (Bit.ToBit(r & 0x080000) << 5) | (Bit.ToBit(r & 0x040000) << 4)
                     | (Bit.ToBit(r & 0x020000) << 3) | (Bit.ToBit(r & 0x010000) << 2)
                     | (Bit.ToBit(r & 0x000800) << 1) | (Bit.ToBit(r & 0x000400) << 0);
                s = data.ToString("X2") + (addr >> 16).ToString("X2") + "-" + (addr & 0xffff).ToString("X4");
                transform(s, "0123456789abcdef", "df4709156bc8a23e");
                return true;
            }
            else
            {
                return false;
            }
        }

        private byte[] bitmask = new byte[0x200000];
        private bool system_enabled;
        private bool code_enabled;
        private bool cheat_enabled;

        private uint mirror(uint addr)
        {
            //$00-3f|80-bf:0000-1fff -> $7e:0000-1fff
            if ((addr & 0x40e000) == 0x000000)
            {
                return (0x7e0000 + (addr & 0x1fff));
            }
            return addr;
        }
    }
}
