using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Working.Messaging.Utils;

namespace Working.Messaging.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var command = args.FirstOrDefault();

            if (command == "test")
            {
                var loginId = GetOption(args, "l");
            }

            Console.ReadLine();
        }

        private static string GetOption(string[] args, string opt)
        {
            var fullOpt = "-" + opt;
            var index = args.IndexOf(fullOpt);

            if (index == -1)
                return null;

            return args.ElementAt(index + 1);
        }
    }
}
