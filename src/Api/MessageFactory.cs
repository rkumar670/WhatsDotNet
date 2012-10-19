using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace WhatsDotNet
{
    public class MessageFactory
    {
        private static readonly MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
        private static readonly Encoding ENC = System.Text.Encoding.UTF8;
        public ProtocolNode CreateTextMessage(string msgid, string to, string txt)
        {
            ProtocolNode bodyNode = new ProtocolNode("body", null, null, txt);
            ProtocolNode serverNode = new ProtocolNode("server", null, null, "");

            Dictionary<string, string> xHash = new Dictionary<string, string>();
            xHash["xmlns"] = "jabber:x:event";
            List<ProtocolNode> listOfServerNode = new List<ProtocolNode>();
            listOfServerNode.Add(serverNode);
            ProtocolNode xNode = new ProtocolNode("x", xHash, listOfServerNode, "");

            Dictionary<string, string> messageHash = new Dictionary<string, string>();
            messageHash["to"] = to + "@" + "s.whatsapp.net";
            messageHash["type"] = "chat";
            messageHash["id"] = msgid;
            List<ProtocolNode> listOfMessageNodeChildren = new List<ProtocolNode>();
            listOfMessageNodeChildren.Add(xNode);
            listOfMessageNodeChildren.Add(bodyNode);
            return new ProtocolNode("message", messageHash, listOfMessageNodeChildren, "");
        }

        public List<byte> CreateHelloMessage()
        {
            List<byte> helloMessage = new List<byte>();
            helloMessage.AddRange(ASCIIEncoding.ASCII.GetBytes("WA"));
            helloMessage.AddRange(new byte[] { 0x01, 0x01, 0x00, 0x19 });

            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes["to"] = "s.whatsapp.net";
            attributes["resource"] = "iPhone-2.8.2-5222";
            ProtocolNode helloMessageNode = new ProtocolNode("stream:stream", attributes, null, "");
            helloMessage.AddRange(helloMessageNode.WriteNode(helloMessageNode));

            return helloMessage;
        }


        public ProtocolNode CreateFeaturesNode()
        {
            ProtocolNode child = new ProtocolNode("receipt_acks", null, null, "");
            List<ProtocolNode> children = new List<ProtocolNode>();
            children.Add(child);
            ProtocolNode parent = new ProtocolNode("stream:features", null, children, "");
            return parent;
        }

        public ProtocolNode CreateAuthNode()
        {
            Dictionary<string, string> authHash = new Dictionary<string, string>();
            authHash["xmlns"] = "urn:ietf:params:xml:ns:xmpp-sasl";
            authHash["mechanism"] = "DIGEST-MD5-1";
            ProtocolNode node = new ProtocolNode("auth", authHash, null, "");
            return node;
        }

        public ProtocolNode CreateAuthResponseNode(string m_realm, string m_username, string m_password, string m_nonce, string m_qop, string m_charset)
        {
            byte[] resp = GenerateResponse(m_realm, m_username, m_password, m_nonce, m_qop, m_charset);
            Dictionary<string, string> respHash = new Dictionary<string, string>();
            respHash["xmlns"] = "urn:ietf:params:xml:ns:xmpp-sasl";
            ProtocolNode node = new ProtocolNode("response", respHash, null, Convert.ToBase64String(resp));
            return node;
        }

        public Dictionary<string, string> ProcessChallenge(ProtocolNode node)
        {
            string challenge = Encoding.ASCII.GetString(Convert.FromBase64String(node.data));
            string[] challengeStrs = challenge.Split(new char[] { ',' });
            Dictionary<string, string> challengeArray = new Dictionary<string, string>();
            foreach (string c in challengeStrs)
            {
                string[] d = c.Split(new char[] { '=' });
                challengeArray[d[0]] = d[1].Replace("\"", "");
            }
            return challengeArray;
        }

        private byte[] GenerateResponse(string m_realm, string m_username, string m_password, string m_nonce, string m_qop, string m_charset)
        {
            // here is where we do the md5 foo
            ASCIIEncoding AE = new ASCIIEncoding();
            byte[] H1, H2, H3, temp;
            string A1, A2, A3, uri, p1, p2, m_cnonce, m_ncString, m_response;
            int m_nc = 0;
            uri = "xmpp/" + m_realm;
            Random r = new Random();
            int v = r.Next(1024);

            StringBuilder sb = new StringBuilder();
            sb.Append(v.ToString());
            sb.Append(":");
            sb.Append(m_username);
            sb.Append(":");
            sb.Append(m_password);

            m_cnonce = HexString(AE.GetBytes(sb.ToString())).ToLower();

            m_nc++;
            m_ncString = m_nc.ToString().PadLeft(8, '0');

            sb.Remove(0, sb.Length);
            sb.Append(m_username);
            sb.Append(":");
            sb.Append(m_realm);
            sb.Append(":");
            sb.Append(m_password);
            H1 = MD5.ComputeHash(AE.GetBytes(sb.ToString()));

            sb.Remove(0, sb.Length);
            sb.Append(":");
            sb.Append(m_nonce);
            sb.Append(":");
            sb.Append(m_cnonce);

            A1 = sb.ToString();

            MemoryStream ms = new MemoryStream();
            ms.Write(H1, 0, 16);
            temp = AE.GetBytes(A1);
            ms.Write(temp, 0, temp.Length);
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            H1 = MD5.ComputeHash(ms);

            sb.Remove(0, sb.Length);
            sb.Append("AUTHENTICATE:");
            sb.Append(uri);
            if (m_qop.CompareTo("auth") != 0)
            {
                sb.Append(":00000000000000000000000000000000");
            }
            A2 = sb.ToString();
            H2 = AE.GetBytes(A2);
            H2 = MD5.ComputeHash(H2);

            // create p1 and p2 as the hex representation of H1 and H2
            p1 = HexString(H1).ToLower();
            p2 = HexString(H2).ToLower();

            sb.Remove(0, sb.Length);
            sb.Append(p1);
            sb.Append(":");
            sb.Append(m_nonce);
            sb.Append(":");
            sb.Append(m_ncString);
            sb.Append(":");
            sb.Append(m_cnonce);
            sb.Append(":");
            sb.Append(m_qop);
            sb.Append(":");
            sb.Append(p2);

            A3 = sb.ToString();
            H3 = MD5.ComputeHash(AE.GetBytes(A3));
            m_response = HexString(H3).ToLower();

            sb = new StringBuilder();
            sb.Append("username=\"");
            sb.Append(m_username);
            sb.Append("\",");
            sb.Append("realm=\"");
            sb.Append(m_realm);
            sb.Append("\",");
            sb.Append("nonce=\"");
            sb.Append(m_nonce);
            sb.Append("\",");
            sb.Append("cnonce=\"");
            sb.Append(m_cnonce);
            sb.Append("\",");
            sb.Append("nc=");
            sb.Append(m_ncString);
            sb.Append(",");
            sb.Append("qop=");
            sb.Append(m_qop);
            sb.Append(",");
            sb.Append("digest-uri=\"");
            sb.Append("xmpp/");
            sb.Append(m_realm);
            sb.Append("\",");
            sb.Append("response=");
            sb.Append(m_response);
            sb.Append(",");
            sb.Append("charset=");
            sb.Append(m_charset);
            return ENC.GetBytes(sb.ToString());
        }

        public string HexString(byte[] buf)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in buf)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }


    }
}
