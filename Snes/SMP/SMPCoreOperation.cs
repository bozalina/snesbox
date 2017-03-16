
using System.Threading.Tasks;

namespace Snes
{
    public delegate Task<SMPCoreOpResult> SMPCoreOp(SMPCoreOpArgument args);

    public class SMPCoreOperation
    {
        private SMPCoreOp op { get; set; }
        private SMPCoreOpArgument args { get; set; }

        public SMPCoreOperation(SMPCoreOp op, SMPCoreOpArgument args)
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
