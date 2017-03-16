#if COMPATIBILITY || PERFORMANCE
using System.IO;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    class DSP : IProcessor
    {
        public static DSP dsp = new DSP();

        public void step(uint clocks)
        {
            Processor.clock += clocks;
        }

        public async Task synchronize_smp()
        {
            if (Processor.clock >= 0 && Scheduler.scheduler.sync != Scheduler.SynchronizeMode.All)
            {
                await Libco.Switch(SMP.smp.Processor.thread);
            }
        }

        public byte read(byte addr)
        {
            return (byte)spc_dsp.read(addr);
        }

        public void write(byte addr, byte data)
        {
            spc_dsp.write(addr, data);
        }

        public void enter()
        {
            spc_dsp.run(1);
            step(24);

            int count = spc_dsp.sample_count();
            if (count > 0)
            {
                for (uint n = 0; n < count; n += 2)
                {
                    Audio.audio.sample(samplebuffer[n + 0], samplebuffer[n + 1]);
                }
                spc_dsp.set_output(samplebuffer, 8192);
            }
        }

        public void power()
        {
            spc_dsp.init(StaticRAM.apuram.data());
            spc_dsp.reset();
            spc_dsp.set_output(samplebuffer, 8192);
        }

        public void reset()
        {
            spc_dsp.soft_reset();
            spc_dsp.set_output(samplebuffer, 8192);
        }

        public void channel_enable(uint channel, bool enable)
        {
            channel_enabled[channel & 7] = enable;
            uint mask = 0;
            for (uint i = 0; i < 8; i++)
            {
                if (channel_enabled[i] == false)
                {
                    mask |= (uint)(1 << (int)i);
                }
            }
            spc_dsp.mute_voices((int)mask);
        }

        public static void dsp_state_save(Stream _out, byte[] _in, uint size)
        {
            _out.Write(_in, 0, (int)size);
        }

        public static void dsp_state_load(Stream _in, byte[] _out, uint size)
        {
            _in.Read(_out, 0, (int)size);
        }

        public void serialize(Serializer s)
        {
            Processor.serialize(s);
            s.array(samplebuffer, "samplebuffer");

            byte[] state = new byte[5000];
            MemoryStream p = new MemoryStream();
            if (s.mode() == Serializer.Mode.Save)
            {
                spc_dsp.copy_state(p, dsp_state_save);
                p.Position = 0;
                p.Read(state, 0, (int)p.Length);
                s.array(state, (uint)p.Length, "state");
            }
            else if (s.mode() == Serializer.Mode.Load)
            {
                s.array(state, "state");
                p.Write(state, 0, state.Length);
                p.Position = 0;
                spc_dsp.copy_state(p, dsp_state_load);
            }
            else
            {
                s.array(state, "state");
            }
        }

        public bool property(uint id, string name, string value)
        {
            return false;
        }

        public DSP()
        {
            for (uint i = 0; i < 8; i++)
            {
                channel_enabled[i] = true;
            }
        }

        private SPCDSP spc_dsp = new SPCDSP();
        private short[] samplebuffer = new short[8192];
        bool[] channel_enabled = new bool[8];

        private Processor _processor = new Processor();
        public Processor Processor
        {
            get
            {
                return _processor;
            }
        }
    }
}
#endif
