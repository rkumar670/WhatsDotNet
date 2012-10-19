using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhatsDotNet.IO;
using WhatsDotNet;

namespace Tests
{
    class TcpSocketMock :ISocket
    {
        List<byte> receiveBuffer;
        public TcpSocketMock()
        {
            protocolPhase = 0;
        }
        private int protocolPhase;
        public int Available
        {
            get { throw new NotImplementedException(); }
        }

        public void Connect(string host, int port)
        {

        }

        public int Send(byte[] buffer)
        {
            if (buffer[0] == 'W' && buffer[1] == 'A')
            {
                //received hello, must wait for features
                protocolPhase = 1;
            }
            else
            {
                MessageParser parser = new MessageParser();
                ProtocolNode node = parser.ParseNode(buffer);
                if (node.tag == "stream:features")
                {
                    //received features, must wait for auth
                    protocolPhase = 2;
                }
                else if (node.tag == "auth")
                {
                    protocolPhase = 3;
                    receiveBuffer = new List<byte>();

                    //<stream:stream from="s.whatsapp.net"></stream:stream>
                    Dictionary<string, string> startNodeAttributes = new Dictionary<string, string>();
                    startNodeAttributes["from"] = "s.whatsapp.net";
                    ProtocolNode startNode = new ProtocolNode("stream:stream", startNodeAttributes, null, "");
                    receiveBuffer.AddRange(startNode.ToBytes());

                    // <stream:features>
                    //   <receipt_acks></receipt_acks>
                    //</stream:features>
                    ProtocolNode receiptAcksNode = new ProtocolNode("receipt_acks", null, null, "");
                    List<ProtocolNode> featuresNodeChildren = new List<ProtocolNode>();
                    featuresNodeChildren.Add(receiptAcksNode);
                    ProtocolNode featuresNode = new ProtocolNode("stream:features", null, featuresNodeChildren, "");
                    receiveBuffer.AddRange(featuresNode.ToBytes());

                    //  <challenge xmlns="urn:ietf:params:xml:ns:xmpp-sasl">bm9uY2U9IjE0OTcxOTQ2MzkyMSIscW9wPSJhdXRoIixjaGFyc2V0PXV0Zi04LGFsZ29yaXRobT1tZDUtc2Vzcw==</challenge>
                    Dictionary<string, string> attributes = new Dictionary<string, string>();
                    attributes["xmlns"] = "urn:ietf:params:xml:ns:xmpp-sasl";
                    ProtocolNode challenge = new ProtocolNode("challenge", attributes, null, "bm9uY2U9IjE0OTcxOTQ2MzkyMSIscW9wPSJhdXRoIixjaGFyc2V0PXV0Zi04LGFsZ29yaXRobT1tZDUtc2Vzcw==");
                    receiveBuffer.AddRange(challenge.ToBytes());
                }
                else if (node.tag == "response")
                {
                    //received
                    //<response xmlns="urn:ietf:params:xml:ns:xmpp-sasl">dXNlcm5hbWU9IjU1NTE5ODc2NTQzMiIscmVhbG09InMud2hhdHNhcHAubmV0Iixub25jZT0iMTQ5NzE5NDYzOTIxIixjbm9uY2U9IjM2MzgzNTNhMzUzNTM1MzEzOTM4MzczNjM1MzQzMzMyM2EzODMxMzIzODYxMzQzMDM4MzMzNzMxMzE2MTM4NjU2MTYzMzk2MTMzNjM2MzY0MzYzODMwNjM2MzMzNjMzOTMyIixuYz0wMDAwMDAwMSxxb3A9YXV0aCxkaWdlc3QtdXJpPSJ4bXBwL3Mud2hhdHNhcHAubmV0IixyZXNwb25zZT1lM2Q5MjIxOTRlN2M3MTBlYmE3YjEyNjI4Mjc5MTgzMyxjaGFyc2V0PXV0Zi04</response>
                    //must verify and send success
                    MessageFactory factory = new MessageFactory();
                    Dictionary<string,string> encodedAttributes = factory.ProcessChallenge(node);
                    if (encodedAttributes["username"] == "555198765432"
                        && encodedAttributes["realm"] == "s.whatsapp.net"
                        && encodedAttributes["nonce"] == "149719463921"
                        && encodedAttributes["nc"] == "00000001"
                        && encodedAttributes["qop"] == "auth"
                        && encodedAttributes["digest-uri"] == "xmpp/s.whatsapp.net"
                        && encodedAttributes["charset"] == "utf-8")

                   {
                       //&& encodedAttributes["cnonce"] == "aaaaa"
                       //&& encodedAttributes["response"] == "e3d922194e7c710eba7b126282791833"
                        //TODO: validate cnonce e response
                       //send
                       //<success xmlns="urn:ietf:params:xml:ns:xmpp-sasl" status="active" kind="free" creation="1338065926" expiration="1369601926"></success>
                       ProtocolNode success = new ProtocolNode("success", null, null, "");
                       receiveBuffer.AddRange(success.ToBytes());

                   }
                }
            }
            return buffer.Length;
        }

        public int Receive(byte[] buffer, System.Net.Sockets.SocketFlags socketFlags)
        {
            int totalBytes = Math.Min(buffer.Length, receiveBuffer.Count);
            Array.Copy(receiveBuffer.ToArray(), buffer, totalBytes);
            receiveBuffer.RemoveRange(0, totalBytes);
            return totalBytes;
        }
    }
}
