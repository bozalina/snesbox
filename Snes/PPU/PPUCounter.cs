using System;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    public delegate Task Scanline();

    //PPUcounter emulates the H/V latch counters of the S-PPU2.
    //
    //real hardware has the S-CPU maintain its own copy of these counters that are
    //updated based on the state of the S-PPU Vblank and Hblank pins. emulating this
    //would require full lock-step synchronization for every clock tick.
    //to bypass this and allow the two to run out-of-order, both the CPU and PPU
    //classes inherit PPUcounter and keep their own counters.
    //the timers are kept in sync, as the only differences occur on V=240 and V=261,
    //based on interlace. thus, we need only synchronize and fetch interlace at any
    //point before this in the frame, which is handled internally by this class at
    //V=128.

    partial class PPUCounter
    {
        public async Task tick()
        {
            status.hcounter += 2;  //increment by smallest unit of time
            if (status.hcounter >= 1360 && status.hcounter == lineclocks())
            {
                status.hcounter = 0;
                await vcounter_tick();
            }

            history.index = (history.index + 1) & 2047;
            history.field[history.index] = status.field;
            history.vcounter[history.index] = status.vcounter;
            history.hcounter[history.index] = status.hcounter;
        }

        public async Task tick(uint clocks)
        {
            status.hcounter += (ushort)clocks;
            if (status.hcounter >= lineclocks())
            {
                status.hcounter -= lineclocks();
                await vcounter_tick();
            }
        }

        public bool field()
        {
            return status.field;
        }

        public ushort vcounter()
        {
            return status.vcounter;
        }

        public ushort hcounter()
        {
            return status.hcounter;
        }

        public ushort hdot()
        {
            if (System.system.region == System.Region.NTSC && status.interlace == false && vcounter() == 240 && field() == Convert.ToBoolean(1))
            {
                return (ushort)(hcounter() >> 2);
            }
            else
            {
                return (ushort)((hcounter() - (Convert.ToInt32(hcounter() > 1292) << 1) - (Convert.ToInt32(hcounter() > 1310) << 1)) >> 2);
            }
        }

        public ushort lineclocks()
        {
            if (System.system.region == System.Region.NTSC && status.interlace == false && vcounter() == 240 && field() == Convert.ToBoolean(1))
            {
                return 1360;
            }
            return 1364;
        }

        public bool field(uint offset)
        {
            return history.field[(history.index - (offset >> 1)) & 2047];
        }

        public ushort vcounter(uint offset)
        {
            return history.vcounter[(history.index - (offset >> 1)) & 2047];
        }

        public ushort hcounter(uint offset)
        {
            return history.hcounter[(history.index - (offset >> 1)) & 2047];
        }

        public void reset()
        {
            status.interlace = false;
            status.field = Convert.ToBoolean(0);
            status.vcounter = 0;
            status.hcounter = 0;
            history.index = 0;

            for (uint i = 0; i < 2048; i++)
            {
                history.field[i] = Convert.ToBoolean(0);
                history.vcounter[i] = 0;
                history.hcounter[i] = 0;
            }
        }

        public Scanline Scanline = null;

        public void serialize(Serializer s)
        {
            s.integer(status.interlace, "status.interlace");
            s.integer(status.field, "status.field");
            s.integer(status.vcounter, "status.vcounter");
            s.integer(status.hcounter, "status.hcounter");

            s.array(history.field, "history.field");
            s.array(history.vcounter, "history.vcounter");
            s.array(history.hcounter, "history.hcounter");
            s.integer(history.index, "history.index");
        }

        private async Task vcounter_tick()
        {
            if (++status.vcounter == 128)
            {
                status.interlace = PPU.ppu.interlace();
            }

            if ((System.system.region == System.Region.NTSC && status.interlace == false && status.vcounter == 262)
            || (System.system.region == System.Region.NTSC && status.interlace == true && status.vcounter == 263)
            || (System.system.region == System.Region.NTSC && status.interlace == true && status.vcounter == 262 && status.field == Convert.ToBoolean(1))
            || (System.system.region == System.Region.PAL && status.interlace == false && status.vcounter == 312)
            || (System.system.region == System.Region.PAL && status.interlace == true && status.vcounter == 313)
            || (System.system.region == System.Region.PAL && status.interlace == true && status.vcounter == 312 && status.field == Convert.ToBoolean(1))
            )
            {
                status.vcounter = 0;
                status.field = !status.field;
            }
            if (!ReferenceEquals(Scanline, null))
            {
                await Scanline();
            }
        }

        private Status status = new Status();
        private History history = new History();
    }
}
