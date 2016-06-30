using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Working.Messaging.Utils;

namespace Working.Messaging.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var command = args.FirstOrDefault();

            var serverName = GetOption(args, "s");

            if (command == "test")
            {
                var loginId = GetOption(args, "l");
                var client = new MessageClient(serverName, loginId);
                client.Connect();
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
                var userCount = Convert.ToInt32(GetOption(args, "c"));
                var threads = new Thread[userCount];
                var messageCount = 0;

                for (var i = 0; i < userCount; i++)
                {
                    var thread = new Thread(new ParameterizedThreadStart((state) =>
                    {
                        var client = new MessageClient(serverName, state.ToString());
                        client.LogMsg = false;
                        client.Connect();

                        var messageDatas = new byte[userCount][];
                        for (var j = 0; j < userCount; j++)
                            messageDatas[j] = new Message { Id = DateTime.Now.Ticks, MsgType = MsgType.Content, To = j.ToString(), Content = "新华社北京6月29日电（记者吴晶、姜潇）旗帜飞扬，歌声嘹亮。" }.Serialize();
                        while (true)
                        {
                            for (var j = 0; j < userCount; j++)
                            {
                                client.Send(messageDatas[j]);
                                messageCount++;
                            }
                        }
                    }));
                    threads[i] = thread;
                }
                for (var i = 0; i < userCount; i++)
                {
                    threads[i].Start(i);
                    Thread.Sleep(100);
                }

                var timer = new System.Timers.Timer(1000);
                var lastMessageCount = 0;
                var maxMessagetCount = 0;
                messageCount = 0;
                timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
                {
                    var count = messageCount - lastMessageCount;
                    if (count > maxMessagetCount) maxMessagetCount = count;
                    Console.WriteLine(string.Format("send rate: {0}, max: {1}", count, maxMessagetCount));
                    lastMessageCount = messageCount;
                };
                timer.Start();

                Console.ReadLine();
            }
        }

        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            throw new NotImplementedException();
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
