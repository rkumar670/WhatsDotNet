using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhatsDotNet;

namespace Tests
{
    class WhatsAppApiMock : WhatsAppApi
    {
        public WhatsAppApiMock()
        {
            socket = new TcpSocketMock();
        }
    }
}
