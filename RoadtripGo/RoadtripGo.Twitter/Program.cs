using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadtripGo.Twitter
{
    class Program
    {
        static void Main(string[] args)
        {
            var twitterConsumer = new Consumer();

            twitterConsumer.Start();

            while (true)
            {
                var key = Console.ReadKey();
            }
        }
    }
}
