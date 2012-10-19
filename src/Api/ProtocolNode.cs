using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatsDotNet
{
    public class ProtocolNode
    {
        public string tag;
        public Dictionary<string, string> attributeHash;
        public List<ProtocolNode> children;
        public string data;

        public ProtocolNode(string tag, Dictionary<string, string> attributeHash, List<ProtocolNode> children, string data)
        {
            this.tag = tag;
            this.attributeHash = attributeHash;
            this.children = children;
            this.data = data;
        }

        public byte[] ToBytes()
        {
            List<byte> message = new List<byte>();

            List<byte> nodeMessage = WriteNode(this);

            message.AddRange(WriteInt16(nodeMessage.Count));
            message.AddRange(nodeMessage);
            return message.ToArray();
        }

        public List<byte> CreateAttributes(Dictionary<string, string> attributes)
        {
            List<byte> attriButesAsBytes = new List<byte>();
            foreach (KeyValuePair<string, string> attr in attributes)
            {
                attriButesAsBytes.AddRange(CreateString(attr.Key));
                attriButesAsBytes.AddRange(CreateString(attr.Value));
            }
            return attriButesAsBytes;
        }

        public List<byte> CreateListStart(int len)
        {
            List<byte> listStart = new List<byte>();
            if (len == 0)
            {
                listStart.Add(01);
            }
            else if (len < 256)
            {
                listStart.AddRange(new byte[] { 0xF8, (byte)len });
            }
            else
            {
                listStart.AddRange(new byte[] { 0xF9, (byte)len }); //TODO: overflow?
            }
            return listStart;
        }

        private List<byte> CreateString(string tag)
        {
            if (ProtocolTable.Instance.CanEncode(tag))
            {
                int key = ProtocolTable.Instance.Encode(tag);
                return WriteToken(key);
            }
            else
            {
                //incomplete
                int index = tag.IndexOf('@');
                if (index >= 0)
                {
                    string server = tag.Substring(index + 1);
                    string user = tag.Substring(0, index);

                    return WriteJid(user, server);
                }
                else
                {
                    return WriteBytes(tag);
                }

            }
        }

        protected List<byte> WriteJid(string user, string server)
        {
            List<byte> jidAsBytes = new List<byte>();
            jidAsBytes.Add(0xfa);
            if (user.Length > 0)
            {
                jidAsBytes.AddRange(CreateString(user));
            }
            else
            {
                jidAsBytes.AddRange(WriteToken(0));
            }
            jidAsBytes.AddRange(CreateString(server));
            return jidAsBytes;
        }

        private List<byte> WriteBytes(string str)
        {
            List<byte> stringAsBytes = new List<byte>();
            byte[] byteString = Encoding.UTF8.GetBytes(str);
            if (byteString.Length >= 0x100)
            {
                stringAsBytes.Add(0xFD);
                stringAsBytes.AddRange(WriteInt24(byteString.Length));
            }
            else
            {
                stringAsBytes.Add(0xFC);
                stringAsBytes.AddRange(WriteInt8(byteString.Length));
            }
            stringAsBytes.AddRange(byteString);
            return stringAsBytes;
        }

        private List<byte> WriteInt8(int v)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)(v & 0xFF));
            return bytes;
        }

        private List<byte> WriteInt16(int v)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)((v & 0xff00) >> 8));
            bytes.Add((byte)((v & 0x00ff) >> 0));
            return bytes;
        }

        private List<byte> WriteInt24(int v)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)((v & 0xff0000) >> 16));
            bytes.Add((byte)((v & 0x00ff00) >> 8));
            bytes.Add((byte)((v & 0x0000ff) >> 0));
            return bytes;
        }

        private List<byte> WriteToken(int token)
        {
            List<byte> tokenAsBytes = new List<byte>();
            if (token < 0xF5)
            {
                tokenAsBytes.Add((byte)token);
            }
            else if (token < 0x1f4)
            {
                tokenAsBytes.AddRange(new byte[] { 0xFE, (byte)(token - 0xF5) });
            }
            return tokenAsBytes;
        }

        public List<byte> WriteNode(ProtocolNode node)
        {
            List<byte> tokenAsBytes = new List<byte>();
            if (node == null)
            {
                tokenAsBytes.Add(0);
            }
            else
            {
                tokenAsBytes.AddRange(WriteNodeInternal(node));
            }
            return tokenAsBytes;
        }

        private List<byte> WriteNodeInternal(ProtocolNode node)
        {
            List<byte> tokenAsBytes = new List<byte>();
            int len = 1;
            if (node.attributeHash != null)
            {
                len += node.attributeHash.Count * 2;
            }
            if (node.children != null && node.children.Count > 0)
            {
                len += 1;
            }
            if (node.data.Length > 0)
            {
                len += 1;
            }
            tokenAsBytes.AddRange(CreateListStart(len));
            tokenAsBytes.AddRange(CreateString(node.tag));
            if (node.attributeHash != null)
            {
                tokenAsBytes.AddRange(CreateAttributes(node.attributeHash));
            }
            if (node.data.Length > 0)
            {
                tokenAsBytes.AddRange(WriteBytes(node.data));
            }

            if (node.children != null)
            {
                tokenAsBytes.AddRange(CreateListStart(node.children.Count));
                foreach (ProtocolNode child in node.children)
                {
                    tokenAsBytes.AddRange(WriteNodeInternal(child));
                }
            }
            return tokenAsBytes;
        }

        public override string ToString()
        {
            return ToString("  ");
        }

        public string ToString(string indent)
        {
            string ret = "\n" + indent + "<" + this.tag;
            if (this.attributeHash != null)
            {
                foreach (KeyValuePair<string, string> attribute in this.attributeHash)
                {
                    ret += " " + attribute.Key + "=\"" + attribute.Value + "\"";
                }
            }

            ret += ">";

            if (this.data.Length > 0)
            {
                ret += this.data;
            }
            if (this.children != null)
            {
                foreach (ProtocolNode child in this.children)
                {
                    ret += child.ToString(indent + "  ");
                }
                ret += "\n" + indent;
            }
            ret += "</" + this.tag + ">";

            return ret;
        }

        public string getAttribute(string attribute)
        {
            string ret = "";
            if (this.attributeHash.ContainsKey(attribute))
            {
                ret = this.attributeHash[attribute];
            }
            return ret;
        }

        public ProtocolNode getChild(string tag)
        {
            ProtocolNode ret = null;
            if (this.children != null)
            {
                foreach (ProtocolNode child in this.children)
                {
                    if (child.tag == tag)
                    {
                        return child;
                    }
                    ret = child.getChild(tag);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
            }
            return null;
        }
    }
}
