namespace SWE3.SeppMapper
{
    /// <summary>Holds entities in form of SeppSet lists and initializes database.</summary>
    public class SeppContext
    {
        /// <summary>Initializes database.</summary>
        /// <param name="connection"></param>
        protected SeppContext(string connection)
        {
            SeppContextController.Initialize(this, connection);
        }
    }
}
