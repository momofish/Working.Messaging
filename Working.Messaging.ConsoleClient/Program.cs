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
        private BsonSerializer _serializer = new BsonSerializer();

        static void Main(string[] args)
        {
            var command = args.FirstOrDefault();

            if (command == "test")
            {
                var loginId = GetOption(args, "l");
                var client = new MessageClient(loginId);
                client.Connect(loginId);
                var to = string.Empty;
                const string TO_PREFIX = "@";
                while (true)
                {
                    var input = Console.ReadLine();
                    if (input.StartsWith(TO_PREFIX))
                    {
                        to = input.TrimStart(TO_PREFIX.ToCharArray());
                    }
                    else
                    {
                        client.Send(new Message { Id = DateTime.Now.Ticks, MsgType = MsgType.Content, To = to, Content = input });
                    }
                }
            }
            else if (command == "ptest")
            {

            }
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
