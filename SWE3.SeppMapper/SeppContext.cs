namespace SWE3.SeppMapper
{
    public class SeppContext
    {
        private readonly SeppController _controller;

        public SeppContext()
        {
            // Setup Logger
            // Setup SQLite connection

            _controller = new SeppController(this);
        }
    }
}
