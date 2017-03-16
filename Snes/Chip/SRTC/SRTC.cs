using System;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    class SRTC : IMMIO
    {
        public static SRTC srtc = new SRTC();

        public void init() { /*throw new NotImplementedException();*/ }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }

        public Task<byte> mmio_read(uint addr) { throw new NotImplementedException(); }
        public Task mmio_write(uint addr, byte data) { throw new NotImplementedException(); }

        public void serialize(Serializer s)
        {
            throw new NotImplementedException();
        }

        public SRTC() { /*throw new NotImplementedException();*/ }

        private static readonly uint[] months = new uint[12];
        private enum RtcMode { RtcReady, RtcCommand, RtcRead, RtcWrite }
        private uint rtc_mode;
        private int rtc_index;

        private void update_time() { throw new NotImplementedException(); }
        private uint weekday(uint year, uint month, uint day) { throw new NotImplementedException(); }
    }
}
