using System;
using System.IO;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    partial class MSU1 : ICoprocessor, IMMIO
    {
        public static MSU1 msu1 = new MSU1();

        public static void Enter() { throw new NotImplementedException(); }
        public void enter() { throw new NotImplementedException(); }
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

        private FileStream datafile;
        private FileStream audiofile;

        private enum Flag { DataBusy = 0x80, AudioBusy = 0x40, AudioRepeating = 0x20, AudioPlaying = 0x10, Revision = 0x01 }
        private MMIO mmio;

        public Coprocessor Coprocessor
        {
            get { throw new NotImplementedException(); }
        }
    }
}
