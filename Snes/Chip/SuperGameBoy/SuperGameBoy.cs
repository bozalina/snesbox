using System;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    class SuperGameBoy : Memory, ICoprocessor, IMMIO
    {
        public static SuperGameBoy supergameboy = new SuperGameBoy();

        public static void Enter() { throw new NotImplementedException(); }
        public void enter() { throw new NotImplementedException(); }
        public void save() { throw new NotImplementedException(); }

        public IMMIO[] mmio = new IMMIO[3];
        public Task<byte> mmio_read(uint addr) { throw new NotImplementedException(); }
        public Task mmio_write(uint addr, byte data) { throw new NotImplementedException(); }

        public override Task<byte> read(uint addr) { throw new NotImplementedException(); }
        public override Task write(uint addr, byte data) { throw new NotImplementedException(); }

        public void init() {  /*throw new NotImplementedException();*/  }
        public void enable() { throw new NotImplementedException(); }
        public void power() { throw new NotImplementedException(); }
        public void reset() { throw new NotImplementedException(); }
        public void unload() { throw new NotImplementedException(); }

        public void serialize(Serializer s)
        {
            throw new NotImplementedException();
        }

        private uint[] samplebuffer = new uint[4096];
        private uint row;

        public Coprocessor Coprocessor
        {
            get { throw new NotImplementedException(); }
        }
    }
}
