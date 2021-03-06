﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Working.Messaging.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<MessageServer>(s =>
                {
                    s.ConstructUsing(name => new MessageServer());
                    s.WhenStarted(server => server.Start());
                    s.WhenStopped(server => server.Stop());
                });
                x.RunAsLocalSystem();
            });
        }
    }
}
