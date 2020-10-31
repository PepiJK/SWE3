using Serilog;

namespace SWE3.SeppMapper
{
    public class SeppContext
    {
        public SeppContext(string connection)
        {
            SeppController.Inititalize(this, connection);
        }
    }
}
