﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Working.Messaging
{
    public class SocketState
    {
        public Socket Socket = null;
        public const int BUFFER_SIZE = 1024;
        public byte[] Buffer = new byte[BUFFER_SIZE];
        public MemoryStream Content = new MemoryStream();
    }
}