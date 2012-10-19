using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace WhatsDotNet.IO
{
    public interface ISocket
    {
        int Available { get; }
        void Connect(string host, int port);
        int Send(byte[] buffer);
        int Receive(byte[] buffer, SocketFlags socketFlags);
    }
}
