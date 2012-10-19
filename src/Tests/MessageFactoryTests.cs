using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhatsDotNet;
using System.Net.Sockets;

namespace Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class MessageFactoryTests
    {
        public MessageFactoryTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void LoginTest()
        {
            // send:
            //    - hello
            //    - feature
            //    - auth
            TcpSocketMock socket = new TcpSocketMock();
            MessageFactory messageFactory = new MessageFactory();
            socket.Connect("bin-short.whatsapp.net", 5222);

            List<byte> helloMessage = messageFactory.CreateHelloMessage();
            socket.Send(helloMessage.ToArray());

            ProtocolNode features = messageFactory.CreateFeaturesNode();
            socket.Send(features.ToBytes());

            ProtocolNode authNode = messageFactory.CreateAuthNode();
            socket.Send(authNode.ToBytes());

            // receive
            //    - start (tream)
            //    - features
            //    - challenge

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

            //send
            // - response

            parser = new MessageParser();
            ProtocolNode challengeNode = parser.ParseNode(challengeMessage);
            Console.WriteLine(challengeNode.ToString());

            Dictionary<string, string> challengeParameters = messageFactory.ProcessChallenge(challengeNode);
            string nonce = challengeParameters["nonce"];
            string qop = challengeParameters["qop"];
            string charset = challengeParameters["charset"];
            string algorithm = challengeParameters["algorithm"];

            WhatsAppApi api = new WhatsAppApi();
            ProtocolNode authResponseNode = messageFactory.CreateAuthResponseNode("s.whatsapp.net", "555198765432", api.EncryptPassword("175422846762539"), nonce, qop, charset);
            socket.Send(authResponseNode.ToBytes());

            byte[] serverMessage = new byte[1024];
            socket.Receive(serverMessage, SocketFlags.None);

            parser = new MessageParser();
            ProtocolNode resultNode = parser.ParseNode(serverMessage);
            Console.WriteLine(resultNode.ToString());
            Assert.AreEqual(resultNode.tag, "success");
            
        }
    }
}
