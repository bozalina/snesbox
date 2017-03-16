using System;
using System.Threading;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    class Processor
    {
        public Task thread;
        public uint frequency;
        public long clock;

        public void create(Func<PauseToken, CancellationToken, Task> entryPoint, uint frequency_)
        {
            if (!ReferenceEquals(thread, null))
            {
                Libco.Delete(thread);
            }

            thread = Libco.Create(entryPoint);
            frequency = frequency_;
            clock = 0;
        }

        public void serialize(Serializer s)
        {
            s.integer(frequency, "frequency");
            s.integer(clock, "clock");
        }

        public Processor()
        {
            thread = null;
        }
    }
}
