using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace WhatsDotNet.IO
{
    public class TcpSocket :Socket, ISocket
    {
        public TcpSocket()
            : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
        }
    }
}
