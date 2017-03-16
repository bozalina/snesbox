
using System.Threading.Tasks;

namespace Snes
{
    public delegate Task CPUCoreOp(CPUCoreOpArgument args);

    public class CPUCoreOperation
    {
        private CPUCoreOp op { get; set; }
        private CPUCoreOpArgument args { get; set; }

        public CPUCoreOperation(CPUCoreOp op, CPUCoreOpArgument args)
        {
            this.op = op;
            this.args = args;
        }

        public async Task Invoke()
        {
            await op(args);
        }
    }
}
