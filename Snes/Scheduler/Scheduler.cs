using System;
using System.Threading;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    class Scheduler
    {
        public static Scheduler scheduler = new Scheduler();

        public enum SynchronizeMode : uint { None, CPU, All }
        public SynchronizeMode sync;

        public enum ExitReason : uint { UnknownEvent, FrameEvent, SynchronizeEvent, DebuggerEvent }
        public ExitReason exit_reason { get; private set; }

        public Task thread; //active emulation thread (used to enter emulation)

        public async Task enter()
        {
            await Libco.Switch(thread);
        }

        public async Task exit(ExitReason reason)
        {
            exit_reason = reason;
	        await Libco.Switch(null);
        }

        public void init()
        {
            thread = CPU.cpu.Processor.thread;
            sync = SynchronizeMode.None;
        }

        public Scheduler()
        {
            thread = null;
            exit_reason = ExitReason.UnknownEvent;
        }
    }
}
