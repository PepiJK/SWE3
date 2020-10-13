using SWE3.SeppMapper;
using SWE3.TestApp.Models;

namespace SWE3.TestApp
{
    public class TestAppContext : SeppContext
    {
        public SeppSet<Person> Persons { get; set; }
    }
}