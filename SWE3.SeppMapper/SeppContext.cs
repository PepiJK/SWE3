namespace SWE3.SeppMapper
{
    public class SeppContext
    {
        public SeppContext()
        {
            // Setup Logger
            // Setup SQLite connection

            SeppController.Inititalize(this);
            var entities = SeppController.Entities;
        }
    }
}
