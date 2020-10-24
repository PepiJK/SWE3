using Serilog;

namespace SWE3.SeppMapper
{
    public class SeppContext
    {
        public SeppContext()
        {
            SeppController.Inititalize(this);
        }
    }
}
