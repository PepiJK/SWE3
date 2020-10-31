using System;
using SWE3.SeppMapper;
using SWE3.TestApp.Models;

namespace SWE3.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new TestAppContext("Server=localhost;Port=5432;Database=sepptest;User Id=sepp;Password=123456;");
        }
    }
}
