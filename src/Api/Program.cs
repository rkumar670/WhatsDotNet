using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;
using System.ComponentModel;

namespace WhatsDotNet
{
    class Program
    {

        static WhatsAppApi api;
        static void Main(string[] args)
        {
            api = new WhatsAppApi();
            string myNumber = System.Configuration.ConfigurationManager.AppSettings["number"];
            string myIMEI = System.Configuration.ConfigurationManager.AppSettings["IMEI"];
            api.Login(myNumber, myIMEI);

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork+=new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerAsync();
            while(true){
                Console.Write(">");
                string message  = Console.ReadLine();
                api.SendMessage("tonumber", message);
            }
            
        }

        static void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProtocolNode resultNode = null;
            while (true)
            {
                resultNode = api.ReceiveNode();
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
                            Console.WriteLine("{0} > {1}", resultNode.getAttribute("from"), resultNode.getChild("body").data);
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
                            api.SendNode(messageReceivedNode);
                        }
                        else
                        {
                            Console.WriteLine(resultNode.ToString());
                        }
                        break;
                    case "stream:error":
                        throw new ApplicationException(resultNode.data);
                        break;
                    default:
                        Console.WriteLine(resultNode.ToString());
                        break;

                }

            }
        }


    }
}
