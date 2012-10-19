using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;
using System.ComponentModel;
using System.Timers;
using WhatsDotNet.IO;

namespace WhatsDotNet
{
    public class WhatsAppApi
    {
        protected ISocket socket;

        MessageFactory messageFactory;

        private static readonly MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();

        public delegate void MessageReceivedEventHandler(object sender, string from, string messageReceived);

        public event MessageReceivedEventHandler MessageReceived;

        public WhatsAppApi()
        {
            socket = new TcpSocket();
            messageFactory = new MessageFactory();
        }

        public bool Login(string number, string imei)
        {
            socket.Connect("bin-short.whatsapp.net", 5222);

            List<byte> helloMessage = messageFactory.CreateHelloMessage();
            socket.Send(helloMessage.ToArray());

            ProtocolNode features = messageFactory.CreateFeaturesNode();
            socket.Send(features.ToBytes());

            ProtocolNode authNode = messageFactory.CreateAuthNode();
            socket.Send(authNode.ToBytes());

            byte[] startMessage = new byte[7];
            socket.Receive(startMessage, SocketFlags.None);

            MessageParser parser = new MessageParser();
            ProtocolNode startNode = parser.ParseNode(startMessage);
            Console.WriteLine(startNode.ToString());

            byte[] serverFeaturesMessage = new byte[10];
            socket.Receive(serverFeaturesMessage, SocketFlags.None);

            parser = new MessageParser();
            ProtocolNode serverFeaturesNode = parser.ParseNode(serverFeaturesMessage);
            Console.WriteLine(serverFeaturesNode.ToString());

            byte[] challengeMessage = new byte[97];
            socket.Receive(challengeMessage, SocketFlags.None);

            parser = new MessageParser();
            ProtocolNode challengeNode = parser.ParseNode(challengeMessage);
            Console.WriteLine(challengeNode.ToString());

            Dictionary<string, string> challengeParameters = messageFactory.ProcessChallenge(challengeNode);
            string nonce = challengeParameters["nonce"];
            string qop = challengeParameters["qop"];
            string charset = challengeParameters["charset"];
            string algorithm = challengeParameters["algorithm"];

            ProtocolNode authResponseNode = messageFactory.CreateAuthResponseNode("s.whatsapp.net", number, EncryptPassword(imei), nonce, qop, charset);
            socket.Send(authResponseNode.ToBytes());

            byte[] serverMessage = new byte[1024];
            socket.Receive(serverMessage, SocketFlags.None);

            parser = new MessageParser();
            ProtocolNode resultNode = parser.ParseNode(serverMessage);
            Console.WriteLine(resultNode.ToString());
            if (resultNode.tag == "success") 
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerAsync();

                return true;
            }
            else if(resultNode.tag == "failure")
            {
                return false;
            } 
            else {
                throw new ApplicationException("Unknown result node " + resultNode.tag);
            }
            
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProtocolNode resultNode = null;
            while (true)
            {
                resultNode = ReceiveNode();
                if (resultNode == null) throw new ApplicationException("Cannot parse message!");
                Console.WriteLine(resultNode.ToString());
                switch (resultNode.tag)
                {
                    case "presence":
                        Console.WriteLine(resultNode.ToString());
                        break;
                    case "message":
                        if (resultNode.getAttribute("from") == "error.us")
                        {
                            throw new ApplicationException("error sending message");
                        }
                        else if (resultNode.getChild("body") != null)
                        {
                            string jid = resultNode.getAttribute("from");
                            string user = jid.Substring(0, jid.IndexOf('@'));
                            MessageReceived(this, user, resultNode.getChild("body").data);
                            Dictionary<string, string> messageParameters = new Dictionary<string, string>();
                            messageParameters["to"] = resultNode.getAttribute("from");
                            messageParameters["type"] = "chat";
                            messageParameters["id"] = resultNode.getAttribute("id");

                            Dictionary<string, string> receivedHash = new Dictionary<string, string>();
                            receivedHash["xmlns"] = "urn:xmpp:receipts";
                            ProtocolNode receivedNode = new ProtocolNode("received", receivedHash, null, "");

                            List<ProtocolNode> childrenMessageReceivedNode = new List<ProtocolNode>();
                            childrenMessageReceivedNode.Add(receivedNode);
                            ProtocolNode messageReceivedNode = new ProtocolNode("message", messageParameters, childrenMessageReceivedNode, "");
                            SendNode(messageReceivedNode);
                        }
                        break;
                    case "stream:error":
                        throw new ApplicationException(resultNode.data);
                        break;
                    default:
                        break;

                }

            }
        }

        public void SendMessage(string to, string txt)
        {
            string msgid = getUnixTimeStamp() + "-1";
            ProtocolNode messageNode = messageFactory.CreateTextMessage(msgid, to, txt);
            SendNode(messageNode);
        }

        public ProtocolNode ReceiveNode()
        {
            List<byte> serverMessage = new List<byte>();
            byte[] buffer = new byte[1024];

            MessageParser parser = new MessageParser();
            socket.Receive(buffer, SocketFlags.None);
            int availableBytes = socket.Available;
            if (availableBytes > 0)
            {
                byte[] oldBuffer = new byte[1024];
                Array.Copy(buffer, oldBuffer, 1024);

                byte[] newBuffer = new byte[availableBytes];
                socket.Receive(newBuffer, SocketFlags.None);

                buffer = new byte[1024 + availableBytes];
                Array.Copy(oldBuffer, buffer, 1024);

                Array.Copy(newBuffer, 0, buffer, 1024, availableBytes);

            }
            return parser.ParseNode(buffer);
        }

        public void SendNode(ProtocolNode messageNode)
        {
            socket.Send(messageNode.ToBytes());
        }

        private string getUnixTimeStamp()
        {
            return ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();
        }

        public string EncryptPassword(string imei)
        {
            if (imei.IndexOf(":") != -1)
            {
                imei = imei.ToUpper();
                return messageFactory.HexString(MD5.ComputeHash(Encoding.ASCII.GetBytes(imei + imei)));
            }
            else
            {
                byte[] arr = Encoding.ASCII.GetBytes(imei);
                Array.Reverse(arr);
                return messageFactory.HexString(MD5.ComputeHash(arr));
            }
        }
    }
}
